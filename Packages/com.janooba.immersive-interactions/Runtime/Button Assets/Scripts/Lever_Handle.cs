
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace JanoobaAssets.ImmersiveInteractions
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Lever_Handle : UdonSharpBehaviour
    {
        [Header("- References -")]
        public Flippable_Switch lever;
        public VRC_Pickup pickup;
        
        public bool IsGrabbed => pickup.IsHeld || forceGrabbed;

        [Header("- DEBUG -")]
        public bool forceGrabbed;
        
        private Vector3 _pickupOffset;
        
        [SerializeField, HideInInspector]
        private Vector3 _homePosition;
        [SerializeField, HideInInspector]
        private Quaternion _homeRotation;
        
        private Rigidbody _rigidbody;
        
#if !COMPILER_UDONSHARP
        private void OnValidate()
        {
            if (lever)
            {
                _homePosition = lever.transform.InverseTransformPoint(transform.position);
                _homeRotation = Quaternion.Inverse(lever.transform.rotation) * transform.rotation;
            }
            
            if (!_rigidbody) 
                _rigidbody = GetComponent<Rigidbody>();
            
            if (_rigidbody)
                _rigidbody.isKinematic = true;
            
            pickup = GetComponent<VRC_Pickup>();
        }
#endif

        public override void OnPickup()
        {
            _pickupOffset = transform.position - lever.transform.TransformPoint(_homePosition);
            lever.Wake();
            Networking.SetOwner(pickup.currentPlayer, lever.gameObject);
        }

        public override void OnDrop()
        {
            SnapHandleHome();
        }

        private void FixedUpdate()
        {
            if (IsGrabbed)
                return;
            
            if (!lever.IsSleeping)
                SnapHandleHome();
        }

        /// <summary>
        /// Set this handle to its home position and rotation.
        /// </summary>
        private void SnapHandleHome()
        {
            transform.position = lever.transform.TransformPoint(_homePosition);
            transform.rotation = lever.transform.rotation * _homeRotation;
        }
    }
}