
using System;
using JanoobaAssets.ImmersiveInteractions;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SpringCollider : UdonSharpBehaviour
{
    private const int SLEEP_FRAMES = 30;
    
    // Detection
    public bool detectPlayers = true;
    public bool detectRigidbodies = true;
    public bool detectStatic = true;
    
    [Tooltip("Colliders that the button should completely ignore.")]
    public Collider[] ignoredColliders;
    
    // Networking
    [Tooltip("Sync the button state across network?")]
    public bool networkSync = false;

    [Tooltip("Whether the button should only work for the instance master.")]
    public bool masterOnly = false;
    
    [Tooltip("Attempt to transfer ownership of receivers to the presser.")]
    public bool transferReceiverOwnership = true;
    
    // Events
    [Tooltip("Will send the above events to all UdonBehaviours in this list" +
             "Note: If button is set to toggleable, \"_On\" and \"_Off\" will optionally be appended.")]
    public UdonBehaviour[] udonReceivers;
    
    // PRIVATE DECLARATIONS //

    public Vector3 _penetration;
    [UdonSynced] private Vector3 _currentPosition;
    private VRC_Pickup.PickupHand _touchingHand;
        
    private Collider[] _triggers;
    private bool _initialized = false;
    
    // Sleeping
    public bool _sleeping = false;
    private int _framesIdle = 0;
    
    // Computation Optimization
    private const int MAX_TOUCHES = 10;
    public Collider[] _collectedColliders = new Collider[MAX_TOUCHES];
    public Vector3[] _collectedPositions = new Vector3[MAX_TOUCHES];
    public PlayerBone[] _collectedBones = new PlayerBone[MAX_TOUCHES];
    
    private Vector3 _lastPosition = Vector3.zero;
    
    #region Initialization
    
    private void Start()
    {
        _triggers = GetComponentsInChildren<Collider>();

        for (int i = 0; i < ignoredColliders.Length; i++)
        {
            Common.IgnoreCollider(transform, _triggers, ignoredColliders[i]);
        }

        SendCustomEventDelayedSeconds(nameof(_SetInitialized), 0.5f);
    }

    public void _SetInitialized()
    {
        _initialized = true;
    }
    
    #endregion
    
    #region Updates
    private void FixedUpdate()
    {
        if (_sleeping) return;

        if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid())
            return;

        _lastPosition = _currentPosition;

        if (!networkSync || Networking.IsOwner(gameObject))
        {
            if (_penetration.sqrMagnitude == 0)
            {
                // Move back to original position
                _currentPosition = Vector3.Lerp(_currentPosition, transform.parent.position, 10 * Time.deltaTime);
            }

            // Push logic is in OnTriggerStay because it needs to run afterwards

            _penetration = Vector3.zero;
        }
        
        for (int c = 0; c < _collectedColliders.Length; c++)
        {
            var other = _collectedColliders[c];
            var bone = _collectedBones[c];
            if (other == null) continue;

            for (int i = 0; i < _triggers.Length; i++)
            {
                var trigger = _triggers[i];
                
                if (CalculatePenetration_Slow(other, trigger, out var penetration) && penetration.sqrMagnitude > _penetration.sqrMagnitude)
                {
                    _penetration = penetration;
                    _touchingHand = bone ? bone.hand : VRC_Pickup.PickupHand.None;

                    // Push
                    var movement = _penetration / Mathf.Max(float.Epsilon, transform.parent.localScale.y);
                    _currentPosition += movement;

                    UpdatePosition();

                    if (_sleeping)
                    {
                        if (networkSync)
                            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Wake));
                        else
                            Wake();
                    }
                }
            }
        }
        
        // Final Position

        UpdatePosition();

        if (Math.Abs((_lastPosition - _currentPosition).sqrMagnitude) < float.Epsilon)
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
        {
            Debug.Log($"Ignoring {other.gameObject.name} due to it being on the blacklist");
            return;
        }

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

        // Immediate update for fresh colliders
        for (var t = 0; t < _triggers.Length; t++)
        {
            var trigger = _triggers[t];

            if (CalculatePenetration_Slow(other, trigger, out var penetration) && penetration.sqrMagnitude > _penetration.sqrMagnitude)
            {
                _penetration = penetration;
                _touchingHand = bone ? bone.hand : VRC_Pickup.PickupHand.None;

                // Push
                _currentPosition += _penetration / Mathf.Max(float.Epsilon, transform.parent.localScale.y);

                UpdatePosition();
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
    
    private void UpdatePosition()
    {
        if (!networkSync || Networking.IsOwner(gameObject))
            transform.position = _currentPosition;
        else
            transform.position = Vector3.Lerp(transform.position, _currentPosition, 10 * Time.deltaTime);
    }
    
    #endregion
    
    private bool CalculatePenetration_Slow(Collider incomingCollider, Collider thisCollider, out Vector3 penetration)
    {
        if (Physics.ComputePenetration(
                incomingCollider, incomingCollider.transform.position, incomingCollider.transform.rotation,
                thisCollider, thisCollider.transform.position, thisCollider.transform.rotation,
                out var direction, out var distance))
        {
            var origin = transform.position;
            penetration = -direction * distance;

            Debug.DrawLine(origin, origin + penetration, new Color(1f, 0.5f, 0f));
            return true;
        }

        penetration = Vector3.zero;
        return false;
    }
    
    #region Array Bullshit

    public void TryAddCollider(Collider collider, PlayerBone bone)
    {
        int existingIndex = Array.IndexOf(_collectedColliders, collider);
        if (existingIndex >= 0) // already exists
        {
            _collectedPositions[existingIndex] = collider.transform.position;
            _lastPosition = transform.parent.position;
        }
        else // doesnt exist
        {
            int emptyIndex = Array.IndexOf(_collectedColliders, null);
            if (emptyIndex >= 0) // empty spot found
            {
                _collectedColliders[emptyIndex] = collider;
                _collectedPositions[emptyIndex] = collider.transform.position;
                _collectedBones[emptyIndex] = bone;
                _lastPosition = transform.parent.position;
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
            _lastPosition = transform.parent.position;
        }
        else // Collider doesn't exist
            return;
    }

    private void ClearColliders()
    {
        _collectedColliders = new Collider[MAX_TOUCHES];
    }

    #endregion
    
    #region Public API
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
}
