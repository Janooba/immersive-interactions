using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanoobaAssets.ImmersiveInteractions
{
    public enum Axis
    {
        X,
        Y,
        Z
    }
    
    public static class Common
    {
        public static bool IsGameObjectLayerInBlacklist(GameObject go)
        {
            var layer = 1 << go.layer;
            var mask = LayerMask.GetMask(new string[] 
            {
                "Interactive",
                "PlayerLocal",
                "Player",
                "MirrorReflection"
            });

            return (layer & mask) != 0;
        }
        
        public static void IgnoreCollider(Transform transform, Collider[] triggers, Collider other)
        {
            if (other == null) return;

            // This was meant to optimize something, but I can't remember what
            // and it's causing issues with the lever handle system
            // var rb = other.GetComponentInParent<Rigidbody>();
            // if (rb && !transform.IsChildOf(rb.transform)) return;
            
            for (var i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                Physics.IgnoreCollision(trigger, other);
            }
        }
        
        public static void SendEvents(Transform transform, bool isCurrentlyToggled, string coreEvent, bool isToggleButton, bool forceSendStateless, UdonBehaviour[] udonReceivers)
        {
            if (isToggleButton)
            {
                if (forceSendStateless)
                {
                    for (var i = 0; i < udonReceivers.Length; i++)
                    {
                        var receiver = udonReceivers[i];
                        if (receiver == null)
                        {
                            Debug.LogError($"Null reference in receivers for button {transform.parent.name}");
                            continue;
                        }

                        receiver.SendCustomEvent(coreEvent);
                    }
                }

                coreEvent += isCurrentlyToggled ? "_On" : "_Off";
            }

            for (var i = 0; i < udonReceivers.Length; i++)
            {
                var receiver = udonReceivers[i];
                if (receiver == null)
                {
                    Debug.LogError($"Null reference in receivers for button {transform.parent.name}");
                    continue;
                }

                receiver.SendCustomEvent(coreEvent);
            }
        }
        
        public static void SetOwner(GameObject gameObject, bool transferReceiverOwnership, UdonBehaviour[] udonReceivers, VRCPlayerApi player = null)
        {
            if (Networking.IsOwner(gameObject)) return;
            if (player == null) player = Networking.LocalPlayer;
            Networking.SetOwner(player, gameObject);

            if (transferReceiverOwnership)
            {
                for (var i = 0; i < udonReceivers.Length; i++)
                {
                    var receiver = udonReceivers[i];
                    Networking.SetOwner(Networking.LocalPlayer, receiver.gameObject);
                }
            }
        }
        
        public static float EstimatePenetration(Transform transform, Vector3 axis, Collider incomingCollider)
        {
            Vector3 toIncoming = incomingCollider.transform.position - transform.position;
            float rawDistance = Vector3.Project(toIncoming, axis.normalized).sqrMagnitude;
            rawDistance *= Mathf.Sign(Vector3.Dot(-axis.normalized, toIncoming.normalized));
            return rawDistance;
        }

        public static string GetHierarchy(Transform transform)
        {
            string hierarchy = "";
            Transform current = transform;
            while (current != null)
            {
                hierarchy = current.name + "/" + hierarchy;
                current = current.parent;
            }

            return hierarchy;
        }
    }
   
}