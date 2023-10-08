
using System;
using System.Collections.Generic;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerSkeletonInfo : UdonSharpBehaviour
{
    public GameObject bonePrefab;

    public bool forceFallback;
    
    public bool includeHead;
    public bool includeHips;
    public bool includeFeet;
    public bool includeFullHands;

    [Header("Debug")]
    public bool showDebug;
    public TextMeshProUGUI debugStatus;

    public bool HasHands { get; private set; }

    private PlayerBone bone_hip;
    private PlayerBone bone_head;
    
    private PlayerBone bone_left_foot;
    private PlayerBone bone_right_foot;

    private PlayerBone bone_left_hand;
    private PlayerBone bone_right_hand;

    private PlayerBone bone_left_littleFinger;
    private PlayerBone bone_left_ringFinger;
    private PlayerBone bone_left_middleFinger;
    private PlayerBone bone_left_indexFinger;
    private PlayerBone bone_left_thumb;

    private PlayerBone bone_right_littleFinger;
    private PlayerBone bone_right_ringFinger;
    private PlayerBone bone_right_middleFinger;
    private PlayerBone bone_right_indexFinger;
    private PlayerBone bone_right_thumb;

    public PlayerBone[] allBones = new PlayerBone[0];
    
    // For checking if avatar has changed size
    private float _prevArmLength;
    private bool _recalcSize;

    private void Start()
    {
        SetupBones();
        _recalcSize = true;
        
        SetDebug(showDebug);
    }

    private void SetupBones()
    {
        if (includeHips)
            bone_hip = CreateBone(HumanBodyBones.Hips, BoneType.Sphere);
        
        if (includeHead)
           bone_head = CreateBone(HumanBodyBones.Head, BoneType.Sphere);

        if (includeFeet)
        {
            bone_left_foot = CreateBone(HumanBodyBones.LeftFoot, BoneType.Capsule);
            bone_right_foot = CreateBone(HumanBodyBones.RightFoot, BoneType.Capsule);   
        }

        bone_left_indexFinger = CreateBone(HumanBodyBones.LeftIndexIntermediate, BoneType.Capsule);
        if (includeFullHands)
        {
            bone_left_hand = CreateBone(HumanBodyBones.LeftHand, BoneType.Box);
            bone_left_littleFinger = CreateBone(HumanBodyBones.LeftLittleIntermediate, BoneType.Capsule);
            bone_left_ringFinger = CreateBone(HumanBodyBones.LeftRingIntermediate, BoneType.Capsule);
            bone_left_middleFinger = CreateBone(HumanBodyBones.LeftMiddleIntermediate, BoneType.Capsule);
            bone_left_thumb = CreateBone(HumanBodyBones.LeftThumbIntermediate, BoneType.Capsule);
        }
            
        bone_right_indexFinger = CreateBone(HumanBodyBones.RightIndexIntermediate, BoneType.Capsule);
        if (includeFullHands)
        {
            bone_right_hand = CreateBone(HumanBodyBones.RightHand, BoneType.Box);
            bone_right_littleFinger = CreateBone(HumanBodyBones.RightLittleIntermediate, BoneType.Capsule);
            bone_right_ringFinger = CreateBone(HumanBodyBones.RightRingIntermediate, BoneType.Capsule);
            bone_right_middleFinger = CreateBone(HumanBodyBones.RightMiddleIntermediate, BoneType.Capsule);
            bone_right_thumb = CreateBone(HumanBodyBones.RightThumbIntermediate, BoneType.Capsule);
        }

        allBones = new PlayerBone[]
        {
            bone_left_indexFinger,
            bone_right_indexFinger,
            
            bone_hip,
            bone_head,

            bone_left_foot,
            bone_right_foot,

            bone_left_hand,
            bone_right_hand,

            bone_left_littleFinger,
            bone_left_ringFinger,
            bone_left_middleFinger,
            bone_left_thumb,

            bone_right_littleFinger,
            bone_right_ringFinger,
            bone_right_middleFinger,
            bone_right_thumb
        };
    }
    
    private PlayerBone CreateBone(HumanBodyBones bodyBoneType, BoneType boneColliderType)
    {
        var bone = Instantiate(bonePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<PlayerBone>();
        bone.hand = GetHand(bodyBoneType);
        bone.SetType(boneColliderType);
        return bone;
    }
    
    private void DestroyBones()
    {
        foreach (var bone in allBones)
        {
            if (!bone) continue;
            Destroy(bone.gameObject);
        }

        allBones = new PlayerBone[0];
    }

    private void Update()
    {
        if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid()) return;
        
        bool hasHands = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftHand).sqrMagnitude > 0.0001f &&
                        Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightHand).sqrMagnitude > 0.0001f;
        
        if (HasHands && !hasHands)
            Debug.Log("No hands on avatar. Falling back to non-physics interaction");
        else if (!HasHands && hasHands)
            Debug.Log("Hands found on avatar. Re-enabling physics interaction");
        
        HasHands = hasHands;
        
        // Check if avatar has changed
        // Gets an estimated good radius for hand bones based off of arm length
        var armLength = Vector3.Distance(
            Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightLowerArm),
            Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightUpperArm));

        if (Mathf.Abs(_prevArmLength - armLength) > float.Epsilon)
        {
            _recalcSize = true;
            _prevArmLength = armLength;
        }

        if (debugStatus)
        {
            debugStatus.text =   "Skeleton Status\n";
            debugStatus.text += $"User In VR: {Networking.LocalPlayer.IsUserInVR()}\n";
            debugStatus.text += $"Has Hands: {HasHands}\n";
        }
    }

    public override void PostLateUpdate()
    {
        if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid()) return;

        if (forceFallback)
        {
            if (allBones.Length > 0)
                DestroyBones();
        }
        else
        {
            if (allBones.Length == 0)
            {
                SetupBones();
                _recalcSize = true;
            }
            
            UpdateAllBonePositions();
            _recalcSize = false;
        }
    }

    private void UpdateAllBonePositions()
    {
        // Update positions here
        if (includeHead) UpdateHeadPosition(bone_head);
        if (includeHips) UpdateHipPosition(bone_hip);

        // Left Hand
        UpdateFingerPosition(bone_left_indexFinger, HumanBodyBones.LeftHand, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal);
        if (includeFullHands)
        {
            UpdatePalmPosition(bone_left_hand, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftIndexProximal, 1f);
            UpdateFingerPosition(bone_left_littleFinger, HumanBodyBones.LeftHand, HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal);
            UpdateFingerPosition(bone_left_ringFinger, HumanBodyBones.LeftHand, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal);
            UpdateFingerPosition(bone_left_middleFinger, HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal);
            UpdateFingerPosition(bone_left_thumb, HumanBodyBones.LeftHand, HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal);
        }

        // Right Hand
        UpdateFingerPosition(bone_right_indexFinger, HumanBodyBones.RightHand, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal);
        if (includeFullHands)
        {
            UpdatePalmPosition(bone_right_hand, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, HumanBodyBones.RightLittleProximal, HumanBodyBones.RightRingProximal, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightIndexProximal, 1f);
            UpdateFingerPosition(bone_right_littleFinger, HumanBodyBones.RightHand, HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal);
            UpdateFingerPosition(bone_right_ringFinger, HumanBodyBones.RightHand, HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal);
            UpdateFingerPosition(bone_right_middleFinger, HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal);
            UpdateFingerPosition(bone_right_thumb, HumanBodyBones.RightHand, HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal);
        }

        // Feet
        if (includeFeet)
        {
            UpdateFootPosition(bone_left_foot, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes, PlayerBone.SIZE_RATIO_FEET);
            UpdateFootPosition(bone_right_foot, HumanBodyBones.RightFoot, HumanBodyBones.RightToes, PlayerBone.SIZE_RATIO_FEET);
        }
        
        UpdateDebugVisuals();
    }

    #region Public API
    public void Toggle_On()
    {
        showDebug = true;
        UpdateDebugVisuals();
    }

    public void Toggle_Off()
    {
        showDebug = false;
        UpdateDebugVisuals();
    }
    #endregion

    #region Bone API

    public void SetDebug(bool debugOn)
    {
        foreach (var bone in allBones)
        {
            if (bone == null) continue;
            bone.GetDebugVisual().SetActive(debugOn);
        }
    }
    
    private void UpdateDebugVisuals()
    {
        if (allBones.Length == 0) return;
        
        if (allBones[0].GetDebugVisual().activeSelf == showDebug)
            return;
            
        foreach (var playerBone in allBones)
        {
            if (playerBone == null) continue;
            
            // Update debug visuals
            var visuals = playerBone.GetDebugVisual();
            if (visuals.activeSelf != showDebug)
                visuals.SetActive(showDebug);
        }
    }

    private void UpdateHeadPosition(PlayerBone playerBone)
    {
        var boneTransform = playerBone.transform;
        var headPos = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
        var neckPos = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Neck);
        var headUp = (headPos - neckPos).normalized;

        // if (near) zero, bone doesnt exist
        if (headPos.sqrMagnitude < 0.0001f)
        {
            SetValid(boneTransform, false);
            return;
        }

        if (_recalcSize)
        {
            RecalculateBoneSize(playerBone, headPos, headPos, PlayerBone.SIZE_RATIO_HEAD);
        }
        
        // Move the head up a bit to account for the neck
        headPos += headUp * (playerBone.sphere.radius * 0.5f);
        boneTransform.position = headPos;
        SetValid(boneTransform, true);
    }
    
    private void UpdateHipPosition(PlayerBone playerBone)
    {
        var boneTransform = playerBone.transform;
        var bonePos = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Hips);

        // if (near) zero, bone doesnt exist
        if (bonePos.sqrMagnitude < 0.0001f)
        {
            SetValid(boneTransform, false);
            return;
        }

        if (_recalcSize)
        {
            RecalculateBoneSize(playerBone, bonePos, bonePos, PlayerBone.SIZE_RATIO_HIP);
        }
        
        boneTransform.position = bonePos;
        SetValid(boneTransform, true);
    }

    private void UpdatePalmPosition(PlayerBone playerBone, HumanBodyBones forearmBone, HumanBodyBones handBone, HumanBodyBones littleFingerBone, HumanBodyBones ringFingerBone, HumanBodyBones middleFingerBone, HumanBodyBones indexFingerBone, float sizeRatio)
    {
        var boneTransform = playerBone.transform;
        var handBonePos = Networking.LocalPlayer.GetBonePosition(handBone);
        var littleFingerBonePos = Networking.LocalPlayer.GetBonePosition(littleFingerBone);
        var ringFingerBonePos = Networking.LocalPlayer.GetBonePosition(ringFingerBone);
        var middleFingerBonePos = Networking.LocalPlayer.GetBonePosition(middleFingerBone);
        var indexFingerBonePos = Networking.LocalPlayer.GetBonePosition(indexFingerBone);
        
        bool littleFingerExists = littleFingerBonePos != Vector3.zero;
        bool ringFingerExists = ringFingerBonePos != Vector3.zero;
        bool middleFingerExists = middleFingerBonePos != Vector3.zero;
        bool indexFingerExists = indexFingerBonePos != Vector3.zero;

        var outerMostFingerPos = handBonePos;
        if (littleFingerExists)
            outerMostFingerPos = littleFingerBonePos;
        else if (ringFingerExists)
            outerMostFingerPos = ringFingerBonePos;
        else if (middleFingerExists)
            outerMostFingerPos = middleFingerBonePos;
        else if (indexFingerExists)
            outerMostFingerPos = indexFingerBonePos;
        
        var innerMostFingerPos = handBonePos;
        if (indexFingerExists)
            innerMostFingerPos = indexFingerBonePos;
        else if (middleFingerExists)
            innerMostFingerPos = middleFingerBonePos;
        else if (ringFingerExists)
            innerMostFingerPos = ringFingerBonePos;
        else if (littleFingerExists)
            innerMostFingerPos = littleFingerBonePos;

        // if no fingers exist, palm isnt valid
        if (!littleFingerExists &&
            !ringFingerExists && 
            !middleFingerExists &&
            !indexFingerExists)
        {
            // SetValid(boneTransform, false);
            // return;
            
            var forearmBonePos = Networking.LocalPlayer.GetBonePosition(forearmBone);
            var forearmToHand = handBonePos - forearmBonePos;
            var fingerBonePos = handBonePos + forearmToHand.normalized * forearmToHand.magnitude * 0.25f;

            if (_recalcSize)
            {
                RecalculateBoneSize(playerBone, handBonePos, fingerBonePos, sizeRatio, forearmToHand.magnitude * 0.25f);
            }
        
            boneTransform.SetPositionAndRotation(handBonePos, Quaternion.LookRotation(fingerBonePos - handBonePos, Vector3.Cross(forearmBonePos - handBonePos, forearmBonePos - indexFingerBonePos)));
            SetValid(boneTransform, true);
        }
        else
        {
            var fingerBonePos = Vector3.Lerp(
                outerMostFingerPos, 
                innerMostFingerPos,
                0.5f);

            if (_recalcSize)
            {
                RecalculateBoneSize(playerBone, handBonePos, fingerBonePos, sizeRatio, Vector3.Distance(outerMostFingerPos, innerMostFingerPos));
            }
        
            boneTransform.SetPositionAndRotation(handBonePos, Quaternion.LookRotation(fingerBonePos - handBonePos, Vector3.Cross(fingerBonePos - handBonePos, fingerBonePos - indexFingerBonePos)));
            SetValid(boneTransform, true);   
        }
    }
    
    private void UpdateFingerPosition(PlayerBone playerBone, HumanBodyBones hand, HumanBodyBones proximal, HumanBodyBones intermediate, HumanBodyBones distal)
    {
        var boneTransform = playerBone.transform;
        
        // Get Estimated Fingertip
        var proximalPos = Networking.LocalPlayer.GetBonePosition(proximal);
        var intermediatePos = Networking.LocalPlayer.GetBonePosition(intermediate);
        var distalPos = Networking.LocalPlayer.GetBonePosition(distal);
        
        Vector3 startPos;
        Vector3 endPos;
        
        if (distalPos.sqrMagnitude < 0.0001f) // No distal bone, use intermediate
        {
            // startPos = proximalPos;
            // endPos = intermediatePos;
            //
            // var dist = Vector3.Distance(startPos, endPos);
            // playerBone.transform.SetPositionAndRotation(startPos, dist > 0 ? Quaternion.LookRotation(endPos - startPos) : Quaternion.identity);

            distalPos = intermediatePos;
            intermediatePos = proximalPos;
            proximalPos = Networking.LocalPlayer.GetBonePosition(hand);
        }
        
        //else
        {
            startPos = intermediatePos;
            endPos = distalPos;
            
            // if (near) zero, bone doesnt exist
            if (startPos.sqrMagnitude < 0.0001f || endPos.sqrMagnitude < 0.0001f)
            {
                SetValid(boneTransform, false);
                return;
            }

            var dist = Vector3.Distance(startPos, endPos);

            var correction = Quaternion.FromToRotation(proximalPos - intermediatePos, intermediatePos - distalPos);

            endPos = distalPos + correction * (distalPos - intermediatePos);
        
            playerBone.transform.SetPositionAndRotation(startPos, dist > 0 ? Quaternion.LookRotation(endPos - startPos) : Quaternion.identity);
        }

        if (_recalcSize)
        {
            RecalculateBoneSize(playerBone, startPos, endPos, 1f);
        }
        
        SetValid(boneTransform, true);
    }

    private void UpdateFootPosition(PlayerBone playerBone, HumanBodyBones start, HumanBodyBones end, float sizeRatio)
    {
        var boneTransform = playerBone.transform;
        var startPos = Networking.LocalPlayer.GetBonePosition(start);
        var endPos = Networking.LocalPlayer.GetBonePosition(end);

        bool startExists = startPos.sqrMagnitude > 0.0001f;
        bool endExists = endPos.sqrMagnitude > 0.0001f;
        
        // if (near) zero, bone doesnt exist
        if (!startExists)
        {
            SetValid(boneTransform, false);
            return;
        }

        if (_recalcSize)
        {
            RecalculateBoneSize(playerBone, startPos, endExists ? endPos : startPos, sizeRatio);
        }
        
        boneTransform.SetPositionAndRotation(startPos, Quaternion.LookRotation(endPos - startPos));
        SetValid(boneTransform, true);
    }
    
    private void SetValid(Transform boneTransform, bool isValid)
    {
        if (boneTransform.gameObject.activeSelf != isValid)
            boneTransform.gameObject.SetActive(isValid);

        if (!isValid) boneTransform.position = Vector3.zero;
    }

    private void RecalculateBoneSize(PlayerBone bone, Vector3 startPos, Vector3 endPos, float radiusRatio, float boxWidth = 0f)
    {
        // Gets an estimated good radius for hand bones based off of arm length
        var baseRadius = Vector3.Distance(
            Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightLowerArm),
            Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightUpperArm)
        ) * 0.025f;

        var dist = Vector3.Distance(startPos, endPos);

        switch (bone.boneType)
        {
            case BoneType.Capsule:
                var capsule = bone.capsule;
                capsule.height = dist;
                capsule.center = Vector3.forward * (dist * 0.5f);
                capsule.radius = baseRadius * radiusRatio;

                var capsuleTransform = bone.capsule_debugVisual.transform;
                capsuleTransform.localPosition = capsule.center;
                capsuleTransform.localScale = new Vector3(capsule.radius * 2, Mathf.Max(dist * 0.5f, capsule.radius), capsule.radius * 2);
                break;
            
            case BoneType.Box:
                var box = bone.box;
                box.size = new Vector3(boxWidth + baseRadius * radiusRatio * 2, baseRadius * radiusRatio * 4, dist + baseRadius * radiusRatio * 2);
                box.center = Vector3.forward * (dist * 0.5f);
                
                var boxTransform = bone.box_debugVisual.transform;
                boxTransform.localPosition = box.center;
                boxTransform.localScale = box.size;
                break;
            
            case BoneType.Sphere:
                var sphere = bone.sphere;
                sphere.radius = baseRadius * radiusRatio;
                
                var sphereTransform = bone.sphere_debugVisual.transform;
                sphereTransform.localPosition = Vector3.zero;
                sphereTransform.localScale = Vector3.one * (sphere.radius * 2);
                break;
        }
    }
    
    #endregion

    #region Static Helpers

    private static VRC_Pickup.PickupHand GetHand(HumanBodyBones thisBone)
    {
        var hand = VRC_Pickup.PickupHand.None;
        if (thisBone.ToString().Contains("Left"))
        {
            hand = VRC_Pickup.PickupHand.Left;
        }
        else if (thisBone.ToString().Contains("Right"))
        {
            hand = VRC_Pickup.PickupHand.Right;
        }

        return hand;
    }

    #endregion
}
