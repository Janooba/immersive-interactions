
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

public enum BoneType
{
    Capsule,
    Box,
    Sphere
}

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerBone : UdonSharpBehaviour
{
    public const float SIZE_RATIO_HIP = 20f;
    public const float SIZE_RATIO_HEAD = 20f;
    public const float SIZE_RATIO_FEET = 8f;

    public VRC_Pickup.PickupHand hand;
    public BoneType boneType; 
    
    public CapsuleCollider capsule;
    public GameObject capsule_debugVisual;
    
    public BoxCollider box;
    public GameObject box_debugVisual;
    
    public SphereCollider sphere;
    public GameObject sphere_debugVisual;

    public void SetType(BoneType boneType)
    {
        this.boneType = boneType;
        
        switch (boneType)
        {
            case BoneType.Capsule:
                Destroy(box);
                Destroy(box_debugVisual);
        
                Destroy(sphere);
                Destroy(sphere_debugVisual);
                break;
            
            case BoneType.Box:
                Destroy(capsule);
                Destroy(capsule_debugVisual);
        
                Destroy(sphere);
                Destroy(sphere_debugVisual);
                break;
            
            case BoneType.Sphere:
                Destroy(box);
                Destroy(box_debugVisual);
        
                Destroy(capsule);
                Destroy(capsule_debugVisual);
                break;
        }
    }

    public GameObject GetDebugVisual()
    {
        switch (boneType)
        {
            case BoneType.Capsule:
                if (capsule_debugVisual == null) Debug.LogError("capsule_debugVisual was null!");
                return capsule_debugVisual;
            
            case BoneType.Box:
                if (box_debugVisual == null) Debug.LogError("box_debugVisual was null!");
                return box_debugVisual;
            
            case BoneType.Sphere:
                if (sphere_debugVisual == null) Debug.LogError("sphere_debugVisual was null!");
                return sphere_debugVisual;
        }

        Debug.LogError("bonetype was invalid!");
        return null;
    }
}
