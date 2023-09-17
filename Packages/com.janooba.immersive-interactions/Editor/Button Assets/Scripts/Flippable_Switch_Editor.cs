using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace JanoobaAssets.ImmersiveInteractions
{
    [CustomEditor(typeof(Flippable_Switch))]
    [CanEditMultipleObjects]
    public class Flippable_Switch_Editor : Editor
    {
        public static Texture logo;

        #region Serialized Property Declarations
        private SerializedProperty isLocked;
        
        private SerializedProperty detectPlayers;
        private SerializedProperty detectRigidbodies;
        private SerializedProperty detectStatic;
        private SerializedProperty ignoredColliders;
        
        private SerializedProperty maxRotation;
        private SerializedProperty triggerZone;

        private SerializedProperty returnRate;
        private SerializedProperty cooldown;

        private SerializedProperty isToggleSwitch;
        private SerializedProperty startToggledOn;
        
        private SerializedProperty fallbackCollider;
        private SerializedProperty fallbackPressTime;
        
        private SerializedProperty networkSync;
        private SerializedProperty masterOnly;
        private SerializedProperty transferReceiverOwnership;
        private SerializedProperty disableWhenNetworkClogged;
        
        private SerializedProperty playAudio;
        private SerializedProperty buttonSource;
        private SerializedProperty pressClip;
        private SerializedProperty depressClip;
        
        private SerializedProperty applyTint;
        private SerializedProperty tintRenderer;
        private SerializedProperty tintShaderKeyword;
        private SerializedProperty offTint;
        private SerializedProperty inTint;
        private SerializedProperty onTint;
        private SerializedProperty lockedTint;
        
        private SerializedProperty applyTexture;
        private SerializedProperty textureRenderer;
        private SerializedProperty textureShaderKeyword;
        private SerializedProperty offTexture;
        private SerializedProperty inTexture;
        private SerializedProperty onTexture;
        private SerializedProperty lockedTexture;

        private SerializedProperty enableAnimation;
        private SerializedProperty animator;
        private SerializedProperty progressParameter;

        private SerializedProperty enableHaptics;
        private SerializedProperty hapticsDuration;
        private SerializedProperty hapticsAmplitude;
        private SerializedProperty hapticsFrequency;

        private SerializedProperty udonReceivers;
        #endregion
        
        public void OnEnable()
        {
            logo = Resources.Load("ImmersiveInteraction_Header") as Texture;

            #region Serialized Property Assignments
            isLocked = serializedObject.FindProperty(nameof(Flippable_Switch.isLocked));
            
            detectPlayers = serializedObject.FindProperty(nameof(Flippable_Switch.detectPlayers));
            detectRigidbodies = serializedObject.FindProperty(nameof(Flippable_Switch.detectRigidbodies));
            detectStatic = serializedObject.FindProperty(nameof(Flippable_Switch.detectStatic));
            ignoredColliders = serializedObject.FindProperty(nameof(Flippable_Switch.ignoredColliders));
            
            maxRotation = serializedObject.FindProperty(nameof(Flippable_Switch.maxRotation));
            triggerZone = serializedObject.FindProperty(nameof(Flippable_Switch.triggerZone));

            returnRate = serializedObject.FindProperty(nameof(Flippable_Switch.returnRate));
            cooldown = serializedObject.FindProperty(nameof(Flippable_Switch.cooldown));

            isToggleSwitch = serializedObject.FindProperty(nameof(Flippable_Switch.isToggleSwitch));
            startToggledOn = serializedObject.FindProperty(nameof(Flippable_Switch.startToggledOn));

            fallbackCollider = serializedObject.FindProperty(nameof(Pressable_Button.fallbackCollider));
            fallbackPressTime = serializedObject.FindProperty(nameof(Pressable_Button.fallbackPressTime));
            
            networkSync = serializedObject.FindProperty(nameof(Flippable_Switch.networkSync));
            masterOnly = serializedObject.FindProperty(nameof(Flippable_Switch.masterOnly));
            transferReceiverOwnership = serializedObject.FindProperty(nameof(Flippable_Switch.transferReceiverOwnership));
            disableWhenNetworkClogged = serializedObject.FindProperty(nameof(Flippable_Switch.disableWhenNetworkClogged));
            
            playAudio = serializedObject.FindProperty(nameof(Flippable_Switch.playAudio));
            buttonSource = serializedObject.FindProperty(nameof(Flippable_Switch.buttonSource));
            pressClip = serializedObject.FindProperty(nameof(Flippable_Switch.pressClip));
            depressClip = serializedObject.FindProperty(nameof(Flippable_Switch.depressClip));
            
            applyTint = serializedObject.FindProperty(nameof(Flippable_Switch.applyTint));
            tintRenderer = serializedObject.FindProperty(nameof(Flippable_Switch.tintRenderer));
            tintShaderKeyword = serializedObject.FindProperty(nameof(Flippable_Switch.tintShaderKeyword));
            offTint = serializedObject.FindProperty(nameof(Flippable_Switch.offTint));
            inTint = serializedObject.FindProperty(nameof(Flippable_Switch.inTint));
            onTint = serializedObject.FindProperty(nameof(Flippable_Switch.onTint));
            lockedTint = serializedObject.FindProperty(nameof(Flippable_Switch.lockedTint));
            
            applyTexture = serializedObject.FindProperty(nameof(Flippable_Switch.applyTexture));
            textureRenderer = serializedObject.FindProperty(nameof(Flippable_Switch.textureRenderer));
            textureShaderKeyword = serializedObject.FindProperty(nameof(Flippable_Switch.textureShaderKeyword));
            offTexture = serializedObject.FindProperty(nameof(Flippable_Switch.offTexture));
            inTexture = serializedObject.FindProperty(nameof(Flippable_Switch.inTexture));
            onTexture = serializedObject.FindProperty(nameof(Flippable_Switch.onTexture));
            lockedTexture = serializedObject.FindProperty(nameof(Flippable_Switch.lockedTexture));
            
            enableAnimation = serializedObject.FindProperty(nameof(Flippable_Switch.enableAnimation));  
            animator = serializedObject.FindProperty(nameof(Flippable_Switch.animator));         
            progressParameter = serializedObject.FindProperty(nameof(Flippable_Switch.progressParameter));
            
            enableHaptics = serializedObject.FindProperty(nameof(Flippable_Switch.enableHaptics));
            hapticsDuration = serializedObject.FindProperty(nameof(Flippable_Switch.hapticsDuration));
            hapticsAmplitude = serializedObject.FindProperty(nameof(Flippable_Switch.hapticsAmplitude));
            hapticsFrequency = serializedObject.FindProperty(nameof(Flippable_Switch.hapticsFrequency));

            udonReceivers = serializedObject.FindProperty(nameof(Flippable_Switch.udonReceivers));
            #endregion
        }
        
        public override void OnInspectorGUI()
        {
            Flippable_Switch flippableSwitch = (Flippable_Switch)target;
            Shared_EditorUtility.DrawMainHeader(logo, target);
            serializedObject.Update();

            if (flippableSwitch.MissingSkeletonInfo)
                EditorGUILayout.HelpBox("No Player Skeleton Info found. This is a required component for player interactions!", MessageType.Error);
            
            GUILayout.Label("// RUNTIME & DETECTION", Shared_EditorUtility.BoldHeader);
            EditorGUILayout.PropertyField(isLocked);
            
            EditorGUILayout.Space();
            GUILayout.Label("What types of objects should this switch detect?");
            EditorGUILayout.PropertyField(detectPlayers);
            EditorGUILayout.PropertyField(detectRigidbodies);
            EditorGUILayout.PropertyField(detectStatic);
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Find Nearby Colliders To Ignore"))
                {
                    var found = Physics.OverlapSphere(flippableSwitch.transform.position, 0.25f);
                    var self = flippableSwitch.GetComponentsInChildren<Collider>();
                    for (int i = 0; i < found.Length; i++)
                    {
                        if (self.Contains(found[i])) continue;
                        TryAddToArray(ref flippableSwitch.ignoredColliders, found[i]);
                    }
                    flippableSwitch.ignoredColliders = flippableSwitch.ignoredColliders.Where(x => x != null).ToArray();
                    EditorUtility.SetDirty(flippableSwitch);
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Clean Up Nulls"))
                {
                    flippableSwitch.ignoredColliders = flippableSwitch.ignoredColliders.Where(x => x != null).ToArray();
                    EditorUtility.SetDirty(flippableSwitch);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(ignoredColliders);

            EditorGUILayout.Space(12);
            GUILayout.Label("// GENERAL SETTINGS", Shared_EditorUtility.BoldHeader);
            
            EditorGUILayout.PropertyField(maxRotation);
            EditorGUILayout.PropertyField(triggerZone);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(cooldown);
            EditorGUILayout.PropertyField(returnRate);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(isToggleSwitch);

            if (flippableSwitch.isToggleSwitch)
            {
                EditorGUILayout.PropertyField(startToggledOn);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(fallbackCollider);
            EditorGUILayout.PropertyField(fallbackPressTime);
            
            EditorGUILayout.Space(12);
            GUILayout.Label("// EVENTS", Shared_EditorUtility.BoldHeader);

            EditorGUILayout.BeginHorizontal();
            {
                flippableSwitch.sendPressedEvent = EditorGUILayout.ToggleLeft("Send Pressed Event", flippableSwitch.sendPressedEvent, EditorStyles.boldLabel, GUILayout.Width(150));
                if (flippableSwitch.sendPressedEvent)
                    flippableSwitch.pressedEventName = EditorGUILayout.TextField(flippableSwitch.pressedEventName);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                flippableSwitch.sendDepressedEvent = EditorGUILayout.ToggleLeft("Send Depressed Event", flippableSwitch.sendDepressedEvent, EditorStyles.boldLabel, GUILayout.Width(150));
                if (flippableSwitch.sendDepressedEvent)
                    flippableSwitch.depressedEventName = EditorGUILayout.TextField(flippableSwitch.depressedEventName);
            }
            EditorGUILayout.EndHorizontal();
            
            flippableSwitch.forceSendStateless = EditorGUILayout.ToggleLeft(new GUIContent("Force Send Stateless", "Send stateless events even if this is a toggle button (Without \"_On\" and \"_Off\")"), flippableSwitch.forceSendStateless);

            if (flippableSwitch.sendPressedEvent || flippableSwitch.sendDepressedEvent)
            {
                string eventsToSend = "Events that will be sent to receivers:\n";

                if ((!flippableSwitch.isToggleSwitch && flippableSwitch.sendPressedEvent) || 
                    (flippableSwitch.isToggleSwitch && flippableSwitch.forceSendStateless && flippableSwitch.sendPressedEvent)) 
                    eventsToSend += $"- {flippableSwitch.pressedEventName}\n";
            
                if ((!flippableSwitch.isToggleSwitch && flippableSwitch.sendDepressedEvent) || 
                    (flippableSwitch.isToggleSwitch && flippableSwitch.forceSendStateless && flippableSwitch.sendDepressedEvent)) 
                    eventsToSend += $"- {flippableSwitch.depressedEventName}\n";
            
                if (flippableSwitch.isToggleSwitch && flippableSwitch.sendPressedEvent)
                {
                    eventsToSend += $"- {flippableSwitch.pressedEventName}_On\n";
                    eventsToSend += $"- {flippableSwitch.pressedEventName}_Off\n";
                }

                if (flippableSwitch.isToggleSwitch && flippableSwitch.sendDepressedEvent)
                {
                    eventsToSend += $"- {flippableSwitch.depressedEventName}_On\n";
                    eventsToSend += $"- {flippableSwitch.depressedEventName}_Off\n";
                }
            
                EditorGUILayout.HelpBox(eventsToSend, MessageType.Info, true);
                EditorGUILayout.PropertyField(udonReceivers);
            }

            EditorGUILayout.Space(12);
            GUILayout.Label("// MODULES", Shared_EditorUtility.BoldHeader);

            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            flippableSwitch.networkSync = EditorGUILayout.ToggleLeft("Network Sync", flippableSwitch.networkSync, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Sync switch state and events with other players.");
            EditorGUILayout.EndHorizontal();
            if (flippableSwitch.networkSync)
            {
                EditorGUILayout.PropertyField(masterOnly);
                EditorGUILayout.PropertyField(transferReceiverOwnership);
                EditorGUILayout.PropertyField(disableWhenNetworkClogged);   
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            flippableSwitch.playAudio = EditorGUILayout.ToggleLeft("Audio", flippableSwitch.playAudio, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Add audio to switch events.");
            EditorGUILayout.EndHorizontal();
            if (flippableSwitch.playAudio)
            {
                EditorGUILayout.PropertyField(buttonSource);
                EditorGUILayout.PropertyField(pressClip);
                EditorGUILayout.PropertyField(depressClip);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            flippableSwitch.applyTint = EditorGUILayout.ToggleLeft("Tint", flippableSwitch.applyTint, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Change a renderer's tint depending on switch state.");
            EditorGUILayout.EndHorizontal();
            if (flippableSwitch.applyTint)
            {
                EditorGUILayout.PropertyField(tintRenderer);
                EditorGUILayout.PropertyField(tintShaderKeyword);
                EditorGUILayout.PropertyField(offTint);
                EditorGUILayout.PropertyField(inTint);
                if (flippableSwitch.isToggleSwitch)
                    EditorGUILayout.PropertyField(onTint);
                EditorGUILayout.PropertyField(lockedTint);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            flippableSwitch.applyTexture = EditorGUILayout.ToggleLeft("Texture", flippableSwitch.applyTexture, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Change a renderer's texture depending on switch state.");
            EditorGUILayout.EndHorizontal();
            if (flippableSwitch.applyTexture)
            {
                EditorGUILayout.PropertyField(textureRenderer);
                EditorGUILayout.PropertyField(textureShaderKeyword);
                EditorGUILayout.PropertyField(offTexture);
                EditorGUILayout.PropertyField(inTexture);
                if (flippableSwitch.isToggleSwitch)
                    EditorGUILayout.PropertyField(onTexture);
                EditorGUILayout.PropertyField(lockedTexture);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            flippableSwitch.enableAnimation = EditorGUILayout.ToggleLeft("Animation", flippableSwitch.enableAnimation, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Control an animator's parameter with press progress.");
            EditorGUILayout.EndHorizontal();
            if (flippableSwitch.enableAnimation)
            {
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(progressParameter);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            flippableSwitch.enableHaptics = EditorGUILayout.ToggleLeft("Haptics", flippableSwitch.enableHaptics, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Trigger haptic feedback for users.");
            EditorGUILayout.EndHorizontal();
            if (flippableSwitch.enableHaptics)
            {
                EditorGUILayout.PropertyField(hapticsDuration);
                EditorGUILayout.PropertyField(hapticsAmplitude);
                EditorGUILayout.PropertyField(hapticsFrequency);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(12);
            GUILayout.Label("// DEBUG", Shared_EditorUtility.BoldHeader);

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Wake"))
            {
                flippableSwitch.Wake();
            }
            if (GUILayout.Button("Force Sleep"))
            {
                flippableSwitch.Sleep();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            
            string text = "";
            text += $"Is Locked: {flippableSwitch.isLocked}\n";
            text += $"Is Toggled: {flippableSwitch.IsToggled}\n";
            text += $"Progress: {flippableSwitch.CurrentUnitProgress}\n";
            text += $"Sleeping: {flippableSwitch.IsSleeping}\n";
            EditorGUILayout.HelpBox(text, MessageType.None);
            
            serializedObject.ApplyModifiedProperties();

        }
        
        public void TryAddToArray<T>(ref T[] array, T toAdd)
        {
            int existingIndex = Array.IndexOf(array, toAdd);
            if (existingIndex < 0) // doesnt exist
            {
                int emptyIndex = Array.IndexOf(array, null);
                if (emptyIndex >= 0) // empty spot found
                {
                    array[emptyIndex] = toAdd;
                }
                else // Array full 
                {
                    Array.Resize(ref array, array.Length + 1);
                    array[array.Length - 1] = toAdd;
                }
            }
        }
    }
}