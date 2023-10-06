
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace JanoobaAssets.ImmersiveInteractions
{
    [RequireComponent(typeof(Rigidbody))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Flippable_Switch : UdonSharpBehaviour
    {
        private const int SLEEP_FRAMES = 30;

        // Runtime
        [UdonSynced] public bool isLocked;

        // Detection
        public bool detectPlayers = true;
        public bool detectRigidbodies = true;
        public bool detectStatic = true;

        [Tooltip("Colliders that the button should completely ignore.")]
        public Collider[] ignoredColliders;

        // Settings
        public bool isToggleSwitch = false;
        public bool startToggledOn = false;

        [Tooltip("How far to rotate the switch in world units."), Min(0.00001f)]
        public float maxRotation = 20f;

        public float returnRate = 2f;

        [Range(0f, 1f), Tooltip("If depressed further than this button, it is considered pushed. Between 0 and 1, where 1 is fully depressed.")]
        public float triggerZone = 0.5f;

        [Min(0f), Tooltip("Cooldown for pressing so that it can't be spammed.")]
        public float cooldown = 0.1f;

        // Fallback
        [Tooltip("Will be disabled and not checked against for VR users. Required for desktop users if the button colliders are children (compound collider)")]
        public Collider fallbackCollider;

        [Tooltip("How long the button will be depressed for when interacted via desktop user.")]
        public float fallbackPressTime = 0.2f;

        // Networking
        [Tooltip("Sync the switch state across network?")]
        public bool networkSync = false;

        [Tooltip("Whether the button should only work for the instance master.")]
        public bool masterOnly = false;

        [Tooltip("Attempt to transfer ownership of receivers to the presser.")]
        public bool transferReceiverOwnership = true;

        [Tooltip("An anti-spam measure. When the network gets backed up, this button can disable itself until things clear up.")]
        public bool disableWhenNetworkClogged = true;

        // Sound
        public bool playAudio;

        [Tooltip("Optional source to play button sounds from.")]
        public AudioSource buttonSource;

        public AudioClip pressClip;
        public AudioClip depressClip;

        // Tint
        public bool applyTint;
        public Renderer tintRenderer;
        public string tintShaderKeyword = "_EmissionColor";
        [ColorUsage(false, true)] public Color offTint;
        [ColorUsage(false, true)] public Color inTint;

        [Tooltip("If the button uses a toggle, this will be used while it is on.")] [ColorUsage(false, true)]
        public Color onTint;

        [ColorUsage(false, true)] public Color lockedTint;

        // Texture
        public bool applyTexture;
        public Renderer textureRenderer;
        public string textureShaderKeyword = "_Icon";
        public Texture2D offTexture;
        public Texture2D inTexture;

        [Tooltip("If the button uses a toggle, this will be used while it is on.")]
        public Texture2D onTexture;

        public Texture2D lockedTexture;

        // Haptics
        public bool enableHaptics = true;
        public float hapticsDuration = 0.05f;

        [Range(0f, 1f), Tooltip("How strong the haptic feedback is.")]
        public float hapticsAmplitude = 0.7f;

        [Tooltip("How fast the vibration is in Hz")]
        public float hapticsFrequency = 100f;

        // Animation
        public bool enableAnimation;
        public Animator animator;
        public string progressParameter = "Press_Progress";

        // Events
        public bool sendPressedEvent;
        public string pressedEventName = "Button_Pressed";

        public bool sendDepressedEvent;
        public string depressedEventName = "Button_Depressed";

        [Tooltip("Send stateless events even if this is a toggle button (Without \"_On\" and \"_Off\")")]
        public bool forceSendStateless;

        [Tooltip("Will send the above events to all UdonBehaviours in this list." +
                 "Note: If switch is set to toggleable, \"_On\" and \"_Off\" will optionally be appended.")]
        public UdonBehaviour[] udonReceivers;

        // PRIVATE DECLARATIONS //

        private float _penetration = 0f;
        [UdonSynced] private float _currentRotation = 0f;
        private float _lastRotation;
        private VRC_Pickup.PickupHand _touchingHand;

        private double _timePressed = 0;
        private double TimeSincePressed => Networking.GetServerTimeInSeconds() - _timePressed;

        private Quaternion TopRotation => transform.parent.rotation * cached_top;
        private Quaternion BotRotation => transform.parent.rotation * cached_bottom;

        [HideInInspector, SerializeField] private Quaternion cached_top;
        [HideInInspector, SerializeField] private Quaternion cached_bottom;
        [HideInInspector, SerializeField] private Rigidbody _rigidbody;
        [HideInInspector, SerializeField] private PlayerSkeletonInfo _skeleton;
        private Collider[] _triggers;

        public bool MissingSkeletonInfo => _skeleton == null;

        private float _currentUnitProgress;

        public float CurrentUnitProgress
        {
            get => _currentUnitProgress;
            private set => _currentUnitProgress = value;
        }

        public bool IsToggled
        {
            get => _isToggled;
            private set => _isToggled = value;
        }

        public bool IsSleeping
        {
            get => _sleeping;
            private set => _sleeping = value;
        }

        public bool IsTriggered { get; private set; }
        [UdonSynced] private bool _isToggled;
        
        public bool UseFallback => (Networking.LocalPlayer != null && !Networking.LocalPlayer.IsUserInVR()) || !_skeleton.HasHands || _skeleton.forceFallback;

        private Vector3 upAxis => transform.parent.forward;
        private Vector3 crossAxis => transform.parent.right;
        private Vector3 outAxis => transform.parent.up;

        // Sleeping
        private bool _sleeping = false;
        private int _framesIdle = 0;

        // Computation Optimization
        private const int MAX_TOUCHES = 10;
        private Collider[] _collectedColliders = new Collider[MAX_TOUCHES];
        private Vector3[] _collectedPositions = new Vector3[MAX_TOUCHES];
        private PlayerBone[] _collectedBones = new PlayerBone[MAX_TOUCHES];

        private Vector3 _lastButtonPosition;

        // Rendering Optimization
        private MaterialPropertyBlock _materialPropertyBlock;

        private bool _initialized = false;
        private bool _fallbackPress = false;

#if !COMPILER_UDONSHARP
        private void OnValidate()
        {
            if (!_rigidbody)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.isKinematic = true;

            if (!_skeleton)
            {
                _skeleton = FindObjectOfType<PlayerSkeletonInfo>();
            }

            if (fallbackCollider)
                fallbackCollider.isTrigger = true;

            if (!Application.isPlaying)
            {
                if (_materialPropertyBlock == null) _materialPropertyBlock = new MaterialPropertyBlock();
                CacheRotations();
                ApplyTint();
                ApplyTexture();
            }
        }
#endif

        #region Initialization

        private void Start()
        {
            _timePressed = Networking.GetServerTimeInSeconds();
            _triggers = GetComponentsInChildren<Collider>();
            if (fallbackCollider) fallbackCollider.enabled = UseFallback;
            CacheRotations();
            _materialPropertyBlock = new MaterialPropertyBlock();

            for (int i = 0; i < ignoredColliders.Length; i++)
            {
                Common.IgnoreCollider(transform, _triggers, ignoredColliders[i]);
            }

            if (startToggledOn)
            {
                _currentRotation = maxRotation;
                IsToggled = true;
                UpdateRotation();
            }
        }

        public void _SetInitialized()
        {
            _initialized = true;
        }

        private void CacheRotations()
        {
            cached_top = transform.localRotation;
            cached_bottom = transform.localRotation * Quaternion.Euler(-maxRotation, 0, 0);
        }

        #endregion

        #region VRC Callbacks

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            // Only let the master take ownership and trigger the button if masterOnly is set to true.
            return !masterOnly;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == Networking.LocalPlayer)
            {
                SendCustomEventDelayedSeconds(nameof(_SetInitialized), 0.5f);
            }
        }

        public override void OnDeserialization()
        {
            // First update
            if (!_initialized)
            {
                UpdateRotation();
                ApplyTint();
                ApplyTexture();
            }
        }

        public override void Interact()
        {
            if (isLocked || (disableWhenNetworkClogged && Networking.IsClogged))
            {
                ApplyTint();
                ApplyTexture();
                return;
            }

            if (TimeSincePressed < cooldown || (!IsToggled && IsTriggered))
            {
                //Debug.Log($"Cannot press, cooldown hasn't elapsed yet! TimeSincePressed: {TimeSincePressed}");
                return;
            }

            if (!Networking.IsOwner(gameObject)) Common.SetOwner(gameObject, transferReceiverOwnership, udonReceivers);
            if (Networking.IsOwner(gameObject))
            {
                IsTriggered = true;
                NonVR_Interact();
            }
            else
            {
                Debug.Log($"Cannot interact, not owner!");
            }
        }

        #endregion

        #region Updates

        private void FixedUpdate()
        {
            if (_sleeping) return;

            if (_fallbackPress)
            {
                UpdateRotation();
                return;
            }

            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid())
                return;

            _lastRotation = _currentRotation;

            if (!networkSync || Networking.IsOwner(gameObject))
            {
                // Respect cooldown too
                if (TimeSincePressed > cooldown && !IsTriggered && (IsToggled ? CurrentUnitProgress < 1 - triggerZone : CurrentUnitProgress > triggerZone))
                {
                    IsTriggered = true;
                    // Press
                    TriggerPress();
                }

                // No need to respect cooldown for depresses
                if (IsTriggered && (IsToggled ? CurrentUnitProgress > 1 - triggerZone : CurrentUnitProgress < triggerZone))
                {
                    IsTriggered = false;
                    // Unpress
                    if (networkSync)
                    {
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Released));
                    }
                    else
                        Released();
                }

                if (_penetration == 0 && !IdleCollidersExist())
                {
                    // Retract
                    // * 90f for a more intuitive return number due to it being angles
                    _currentRotation = Mathf.MoveTowards(_currentRotation, IsToggled ? maxRotation : 0, returnRate * 90f * Time.fixedDeltaTime);
                }

                // Push logic is in OnTriggerStay because it needs to run afterwards

                _penetration = 0;
            }

            if (isLocked || (disableWhenNetworkClogged && Networking.IsClogged))
            {
                ApplyTint();
                ApplyTexture();
                return;
            }

            // Estimate deepest collider
            float deepestPenetration = float.MinValue;
            Collider deepestCollider = null;
            float closestTrigger = float.MaxValue;
            Collider penetratedTrigger = null;
            PlayerBone touchingBone = null;

            for (int c = 0; c < _collectedColliders.Length; c++)
            {
                var other = _collectedColliders[c];
                var bone = _collectedBones[c];
                if (other == null) continue;

                // Early return based off penetration estimate
                var depth = Common.EstimatePenetration(transform, outAxis, other);
                if (depth > deepestPenetration)
                {
                    deepestPenetration = depth;
                    deepestCollider = other;
                    touchingBone = bone;
                }

                // Get closest trigger
                for (var t = 0; t < _triggers.Length; t++)
                {
                    var trigger = _triggers[t];
                    if (trigger == fallbackCollider) continue;

                    var dist = Vector3.Distance(trigger.transform.position, other.transform.position);
                    if (dist < closestTrigger)
                    {
                        closestTrigger = dist;
                        penetratedTrigger = trigger;
                    }
                }
            }

            // Process deepest

            // Don't recalculate idle colliders
            if (deepestCollider && !ColliderIdle(deepestCollider))
            {
                if (CalculatePenetration_Slow(deepestCollider, penetratedTrigger, out float penetration, out float sign) && penetration > _penetration)
                {
                    _penetration = penetration;
                    _touchingHand = touchingBone ? touchingBone.hand : VRC_Pickup.PickupHand.None;

                    // Rotation direction
                    _currentRotation += _penetration * sign;

                    //Clamp
                    _currentRotation = Mathf.Clamp(_currentRotation, 0, maxRotation);
                    UpdateRotation();

                    if (_sleeping)
                    {
                        if (networkSync)
                            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                        else
                            Wake();
                    }
                }
            }

            // Final Position

            UpdateRotation();

            if (Math.Abs(_lastRotation - _currentRotation) < float.Epsilon)
                _framesIdle++;
            else
                _framesIdle = 0;

            if (_framesIdle > SLEEP_FRAMES)
            {
                Sleep();
            }
        }

        private void Update()
        {
            if (fallbackCollider) fallbackCollider.enabled = UseFallback;
            DisableInteractive = !UseFallback;

            if (isLocked || (disableWhenNetworkClogged && Networking.IsClogged))
            {
                ApplyTint();
                ApplyTexture();
                return;
            }
            else
            {
                ApplyTint();
                ApplyTexture();
            }

            if (_sleeping) return;

            if (networkSync &&
                Networking.LocalPlayer != null &&
                Networking.LocalPlayer.IsValid() &&
                Networking.LocalPlayer.IsOwner(gameObject))
                RequestSerialization();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_initialized)
            {
                Common.IgnoreCollider(transform, _triggers, other);
                return;
            }

            if (Common.IsGameObjectLayerInBlacklist(other.gameObject))
                return;

            // this is probably really slow
            var bone = other.GetComponent<PlayerBone>();
            if (bone) Common.SetOwner(gameObject, transferReceiverOwnership, udonReceivers);

            var rbody = other.GetComponentInParent<Rigidbody>();
            var syncer = other.GetComponentInParent<VRCObjectSync>();

            // Detection short circuits
            if (!detectPlayers && bone) return;
            if (!detectRigidbodies && rbody) return;
            if (!detectStatic && !bone && !rbody) return;

            // Allows physics to set the proper owner to the person who last touched the physics
            var incomingOwner = Networking.GetOwner(other.gameObject);
            if (networkSync && rbody && Networking.GetOwner(gameObject) != incomingOwner)
                Common.SetOwner(gameObject, transferReceiverOwnership, udonReceivers);

            // Only allow owners to update, unless not networked
            if (networkSync && !Networking.IsOwner(gameObject))
                return;

            // When unsynced: Don't allow pressing from unowned physics
            // (Stops others from ghost-pressing your local button with synced physics)
            if ((!networkSync || masterOnly) && rbody && syncer && !Networking.IsOwner(other.gameObject))
                return;

            TryAddCollider(other, bone);

            if (isLocked || (disableWhenNetworkClogged && Networking.IsClogged))
                return;

            // Immediate update for fresh colliders
            for (var i = 0; i < _triggers.Length; i++)
            {
                var trigger = _triggers[i];
                if (trigger == fallbackCollider) continue;

                if (CalculatePenetration_Slow(other, trigger, out float penetration, out float sign) && penetration > _penetration)
                {
                    _penetration = penetration;
                    if (bone) _touchingHand = bone.hand;

                    // Rotation direction
                    _currentRotation += _penetration * sign;

                    //Clamp
                    _currentRotation = Mathf.Clamp(_currentRotation, 0, maxRotation);
                    UpdateRotation();
                }
            }

            if (_sleeping)
            {
                if (networkSync)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                else
                    Wake();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_sleeping)
            {
                if (networkSync)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                else
                    Wake();
            }

            TryRemoveCollider(other);
        }

        private void UpdateRotation()
        {
            CurrentUnitProgress = Mathf.Clamp01(_currentRotation / maxRotation);
            transform.rotation = Quaternion.Slerp(TopRotation, BotRotation, CurrentUnitProgress);

            ApplyAnimation();
        }

        #endregion

        #region Interactions

        public void NonVR_Interact()
        {
            if (_sleeping)
            {
                if (networkSync)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                else
                    Wake();
            }

            TriggerPress();

            if (!isToggleSwitch)
            {
                _fallbackPress = true;
                SendCustomEventDelayedSeconds(nameof(NonVR_Release), fallbackPressTime);
            }
        }

        public void NonVR_Release()
        {
            _fallbackPress = false;
        }

        public void Pressed(bool isToggledOn)
        {
            if (_sleeping)
            {
                if (networkSync)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                else
                    Wake();
            }

            _timePressed = Networking.GetServerTimeInSeconds();
            IsTriggered = true;

            if (isToggleSwitch)
            {
                IsToggled = isToggledOn;
                PlayAudio(IsToggled ? pressClip : depressClip);
            }
            else
            {
                PlayAudio(pressClip);
            }

            if (sendPressedEvent)
            {
                Common.SendEvents(transform, IsToggled, pressedEventName, isToggleSwitch, forceSendStateless, udonReceivers);
            }

            if (UseFallback)
            {
                _currentRotation = maxRotation;
            }
            else
            {
                PlayHaptics();
            }

            ApplyTint();
            ApplyTexture();
        }

        public void Released()
        {
            if (_sleeping)
            {
                if (networkSync)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                else
                    Wake();
            }

            IsTriggered = false;

            if (!isToggleSwitch)
                PlayAudio(depressClip);

            if (sendDepressedEvent)
            {
                Common.SendEvents(transform, IsToggled, depressedEventName, isToggleSwitch, forceSendStateless, udonReceivers);
            }

            ApplyTint();
            ApplyTexture();

            if (UseFallback)
            {
                _currentRotation = IsToggled ? maxRotation : 0f;
            }
            else
            {
                PlayHaptics();
            }
        }

        #endregion

        #region Utilities

        private void TriggerPress()
        {
            if (isToggleSwitch)
            {
                if (IsToggled)
                {
                    if (networkSync)
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Pressed_Off));
                    else
                        Pressed_Off();
                }
                else
                {
                    if (networkSync)
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Pressed_On));
                    else
                        Pressed_On();
                }
            }
            else
            {
                if (networkSync)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Pressed));
                else
                    Pressed();
            }
        }

        private bool CalculatePenetration_Slow(Collider incomingCollider, Collider thisCollider, out float penetration, out float sign)
        {
            if (Physics.ComputePenetration(
                    thisCollider, thisCollider.transform.position, thisCollider.transform.rotation,
                    incomingCollider, incomingCollider.transform.position, incomingCollider.transform.rotation,
                    out var direction, out var distance))
            {
                var origin = transform.position;
                var closestPoint = incomingCollider.ClosestPoint(transform.position);
                var closestDepenetration = closestPoint + distance * direction;

                var originToClosest = closestPoint - origin;
                var originToDepenetration = closestDepenetration - origin;
                var closestToDepenetration = closestDepenetration - closestPoint;

                Debug.DrawLine(origin, origin + originToClosest, new Color(1f, 0.5f, 0f));
                Debug.DrawLine(origin + originToClosest, origin + originToDepenetration, Color.magenta);

                // Try to limit pushing the button from behind
                var isDown = !(Vector3.Dot(outAxis, direction) > 0.1f || Vector3.Dot(outAxis, originToClosest.normalized) < 0.1f);

                penetration = Mathf.Acos((originToClosest.sqrMagnitude + originToDepenetration.sqrMagnitude - closestToDepenetration.sqrMagnitude) / Mathf.Max(0.000001f, 2 * originToClosest.magnitude * originToDepenetration.magnitude));
                penetration *= Mathf.Rad2Deg;

                sign = -Mathf.Sign(Vector3.SignedAngle(originToClosest, originToDepenetration, crossAxis));
                return isDown;
            }

            penetration = 0;
            sign = 0;
            return false;
        }

        #endregion

        #region Array Bullshit

        public bool IdleCollidersExist()
        {
            for (int i = 0; i < _collectedColliders.Length; i++)
            {
                if (_collectedColliders[i] != null) return true;
            }

            return false;
        }

        public bool ColliderIdle(Collider collider)
        {
            int existingIndex = Array.IndexOf(_collectedColliders, collider);
            if (existingIndex >= 0) // exists
            {
                float colDist = Vector3.Distance(_collectedPositions[existingIndex], collider.transform.position);
                float btnDist = Vector3.Distance(_lastButtonPosition, transform.parent.position);
                _collectedPositions[existingIndex] = collider.transform.position;
                _lastButtonPosition = transform.parent.position;
                return colDist < float.Epsilon && btnDist < float.Epsilon;
            }

            return false;
        }

        public void TryAddCollider(Collider collider, PlayerBone bone)
        {
            int existingIndex = Array.IndexOf(_collectedColliders, collider);
            if (existingIndex >= 0) // already exists
            {
                _collectedPositions[existingIndex] = collider.transform.position;
                _lastButtonPosition = transform.parent.position;
            }
            else // doesnt exist
            {
                int emptyIndex = Array.IndexOf(_collectedColliders, null);
                if (emptyIndex >= 0) // empty spot found
                {
                    _collectedColliders[emptyIndex] = collider;
                    _collectedPositions[emptyIndex] = collider.transform.position;
                    _collectedBones[emptyIndex] = bone;
                    _lastButtonPosition = transform.parent.position;
                }
                else // Array full 
                    return;
            }
        }

        public void TryRemoveCollider(Collider collider)
        {
            int existingIndex = Array.IndexOf(_collectedColliders, collider);
            if (existingIndex >= 0) // Collider found
            {
                _collectedColliders[existingIndex] = null;
                _collectedPositions[existingIndex] = Vector3.zero;
                _lastButtonPosition = transform.parent.position;
            }
            else // Collider doesn't exist
                return;
        }

        #endregion

        #region Feedback Modules

        private void PlayAudio(AudioClip clip)
        {
            if (playAudio && buttonSource && clip)
                buttonSource.PlayOneShot(clip);
        }

        private void PlayHaptics()
        {
            if (Networking.IsOwner(gameObject) && enableHaptics && _touchingHand != VRC_Pickup.PickupHand.None)
                Networking.LocalPlayer.PlayHapticEventInHand(_touchingHand, hapticsDuration, hapticsAmplitude, hapticsFrequency);
        }

        private void ApplyTint()
        {
            if (applyTint && tintRenderer)
            {
                var tint = offTint;
                if (isLocked) tint = lockedTint;
                else if (IsTriggered) tint = inTint;
                else if (IsToggled) tint = onTint;

                _materialPropertyBlock.SetColor(tintShaderKeyword, tint);
                tintRenderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }

        private void ApplyTexture()
        {
            if (applyTexture && textureRenderer && offTexture)
            {
                var texture = offTexture;
                if (isLocked && lockedTexture) texture = lockedTexture;
                else if (IsTriggered && inTexture) texture = inTexture;
                else if (IsToggled && onTexture) texture = onTexture;

                _materialPropertyBlock.SetTexture(textureShaderKeyword, texture);
                textureRenderer.SetPropertyBlock(_materialPropertyBlock);
            }
            else if (applyTexture && textureRenderer)
            {
                _materialPropertyBlock.SetTexture(textureShaderKeyword, Texture2D.blackTexture);
                textureRenderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }

        private void ApplyAnimation()
        {
            if (enableAnimation && animator)
                animator.SetFloat(progressParameter, CurrentUnitProgress);
        }

        #endregion

        #region Public API

        public void Pressed()
        {
            Pressed(false);
        }

        public void Pressed_On()
        {
            Pressed(true);
        }

        public void Pressed_Off()
        {
            Pressed(false);
        }
        
        public void Lock_On()
        {
            isLocked = true;
            ApplyTint();
            ApplyTexture();
        }

        public void Lock_Off()
        {
            isLocked = false;
            ApplyTint();
            ApplyTexture();
        }
        
        public void Wake()
        {
            _framesIdle = 0;
            _sleeping = false;
        }

        public void Sleep()
        {
            _sleeping = true;
        }

        #endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + upAxis * 0.1f);
            Gizmos.DrawLine(transform.position, transform.position + outAxis * 0.1f);
            Gizmos.DrawLine(transform.position, transform.position + crossAxis * 0.1f);
        }
    }
}