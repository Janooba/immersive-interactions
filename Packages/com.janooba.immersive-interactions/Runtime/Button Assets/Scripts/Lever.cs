
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanoobaAssets.ImmersiveInteractions
{
    [RequireComponent(typeof(Rigidbody))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Lever : UdonSharpBehaviour
    {
        [Tooltip("Colliders that the button should completely ignore.")]
        public Collider[] ignoredColliders;
        
        // Settings
        [Tooltip("How far to rotate the switch in world units.")]
        public Vector2 minMaxRotation = new Vector2(-20f, 20f);
        
        // PRIVATE DECLARATIONS //
        
        [HideInInspector, SerializeField] private Rigidbody _rigidbody;
        [HideInInspector, SerializeField] private PlayerSkeletonInfo _skeleton;
        private Collider[] _triggers;
        
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
        }
#endif
        
        private void Start()
        {
            _triggers = GetComponentsInChildren<Collider>();
            
            for (int i = 0; i < ignoredColliders.Length; i++)
            {
                Common.IgnoreCollider(transform, _triggers, ignoredColliders[i]);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            for (var i = 0; i < _triggers.Length; i++)
            {
                var trigger = _triggers[i];
                CalculatePenetration_Slow(other, trigger, out float deltaRotation);
                transform.Rotate(transform.right * deltaRotation);
            }
        }

        private void CalculatePenetration_Slow(Collider incomingCollider, Collider thisCollider, out float rotation)
        {
            rotation = 0;
            if (Physics.ComputePenetration(
                    thisCollider, thisCollider.transform.position, thisCollider.transform.rotation,
                    incomingCollider, incomingCollider.transform.position, incomingCollider.transform.rotation,
                    out var direction, out var distance))
            {
                //Debug.Log($"Penetration: {incomingCollider.name}:{incomingCollider.GetType().Name} - {thisCollider.name}:{thisCollider.GetType().Name}");
                Vector3 originalPosition = transform.position;
                float paddedDistance = distance + 0.001f;
                transform.position += direction * paddedDistance;
                Physics.SyncTransforms();

                Vector3 contactPoint;
                Debug.DrawLine(transform.position, transform.position - (direction * distance), new Color(0.52f, 1f, 0.26f));
                if (_rigidbody.SweepTest(-direction, out RaycastHit hit, Mathf.Infinity, QueryTriggerInteraction.Collide))
                {
                    contactPoint = hit.point;
                    
                    Debug.DrawLine(contactPoint, contactPoint + (direction * paddedDistance), new Color(1f, 0.61f, 0.13f));
                    
                    Vector3 origin = originalPosition;
                    Vector3 p1 = contactPoint;
                    Vector3 p2 = contactPoint + (direction * paddedDistance);
                    
                    Vector3 oToP1 = p1 - origin;
                    Vector3 oToP2 = p2 - origin;

                    rotation = Vector3.SignedAngle(oToP1, oToP2, transform.right);
                    //transform.RotateAround(contactPoint, transform.right, angle);
                }
                
                transform.position = originalPosition;
            }
        }
    }
}