
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
    public class Physical_Door : UdonSharpBehaviour
    {
        private const int SLEEP_FRAMES = 30;
        private const float FORCE_MULTIPLIER = 1f;

        // Detection
        public bool detectPlayers = true;

        [Tooltip("Colliders that the button should completely ignore.")]
        public Collider[] ignoredColliders;

        // PRIVATE DECLARATIONS //

        private Vector3 _penetration = Vector3.zero;

        private double _timePressed = 0;
        
        [HideInInspector, SerializeField] private Rigidbody _rigidbody;
        [HideInInspector, SerializeField] private PlayerSkeletonInfo _skeleton;
        public Collider[] _triggers;

        public bool MissingSkeletonInfo => _skeleton == null;
        
        [UdonSynced] private bool _isToggled;
        
        public bool UseFallback => (Networking.LocalPlayer != null && !Networking.LocalPlayer.IsUserInVR()) || !_skeleton.HasHands || _skeleton.forceFallback;

        // Computation Optimization
        private const int MAX_TOUCHES = 10;
        public Collider[] _collectedColliders = new Collider[MAX_TOUCHES];

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

            if (!Application.isPlaying)
            {
                if (_materialPropertyBlock == null) _materialPropertyBlock = new MaterialPropertyBlock();
            }
        }
#endif

        #region Initialization

        private void Start()
        {
            _triggers = GetComponentsInChildren<Collider>();
            _materialPropertyBlock = new MaterialPropertyBlock();

            for (int i = 0; i < ignoredColliders.Length; i++)
            {
                Common.IgnoreCollider(transform, _triggers, ignoredColliders[i]);
            }

            _initialized = true;
        }

        #endregion

        #region Updates

        private void FixedUpdate()
        {
            if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid())
            {
                Debug.Log("Returning because local player not valid");
                return;
            }

            if (Networking.IsOwner(gameObject))
            {
                // Push logic is in OnTriggerStay because it needs to run afterwards
                _penetration = Vector3.zero;
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
                if (other == null) continue;

                // Early return based off penetration estimate
                var depth = (other.transform.position - transform.position).sqrMagnitude;
                if (depth > deepestPenetration)
                {
                    deepestPenetration = depth;
                    deepestCollider = other;
                }

                // Get closest trigger
                for (var t = 0; t < _triggers.Length; t++)
                {
                    var trigger = _triggers[t];

                    var dist = (other.transform.position - trigger.transform.position).sqrMagnitude;
                    if (dist < closestTrigger)
                    {
                        closestTrigger = dist;
                        penetratedTrigger = trigger;
                    }
                }
            }

            // Process deepest

            // Don't recalculate idle colliders
            if (deepestCollider)
            {
                if (CalculatePenetration_Slow(deepestCollider, penetratedTrigger, out Vector3 penetration))
                {
                    _penetration = penetration;
                    var force = _penetration * FORCE_MULTIPLIER;
                    var closestPoint = deepestCollider.ClosestPoint(transform.position);
                    _rigidbody.AddForceAtPosition(force, closestPoint, ForceMode.VelocityChange);
                    Debug.Log($"Adding force: {force}");
                }
            }
            else
            {
                Debug.Log("No collider registered as deepest");
            }
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
                Debug.Log($"{gameObject.name} is in the layer blacklist. Ignoring!");
                return;
            }

            // this is probably really slow
            var bone = other.GetComponent<PlayerBone>();

            // Detection short circuits
            if (!detectPlayers && bone)
            {
                Debug.Log($"Detect players off and this is a bone");
                return;
            }

            TryAddCollider(other, bone);
            
            // Immediate update for fresh colliders
            for (var i = 0; i < _triggers.Length; i++)
            {
                var trigger = _triggers[i];

                if (CalculatePenetration_Slow(other, trigger, out Vector3 penetration))
                {
                    _penetration = penetration;
                    var force = _penetration * FORCE_MULTIPLIER;
                    var closestPoint = other.ClosestPoint(transform.position);
                    _rigidbody.AddForceAtPosition(_penetration, closestPoint, ForceMode.VelocityChange);
                    Debug.Log($"Adding force: {force}");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            TryRemoveCollider(other);
        }

        #endregion

        #region Utilities

        private bool CalculatePenetration_Slow(Collider incomingCollider, Collider thisCollider, out Vector3 penetration)
        {
            if (Physics.ComputePenetration(
                    thisCollider, thisCollider.transform.position, thisCollider.transform.rotation,
                    incomingCollider, incomingCollider.transform.position, incomingCollider.transform.rotation,
                    out var direction, out var distance))
            {
                penetration = direction * distance;
                return true;
            }

            penetration = Vector3.zero;
            return false;
        }

        #endregion
        
        public void TryAddCollider(Collider collider, PlayerBone bone)
        {
            int existingIndex = Array.IndexOf(_collectedColliders, collider);
            if (existingIndex >= 0) // already exists
            {
                _lastButtonPosition = transform.parent.position;
            }
            else // doesnt exist
            {
                int emptyIndex = Array.IndexOf(_collectedColliders, null);
                if (emptyIndex >= 0) // empty spot found
                {
                    _collectedColliders[emptyIndex] = collider;
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
                _lastButtonPosition = transform.parent.position;
            }
            else // Collider doesn't exist
                return;
        }
    }
}