using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace JanoobaAssets.ImmersiveInteractions
{
    [CustomEditor(typeof(Pressable_Button))]
    [CanEditMultipleObjects]
    public class Pressable_Button_Editor : Editor
    {
        public static Texture logo;

        #region Serialized Property Declarations
        private SerializedProperty isLocked;
        
        private SerializedProperty detectPlayers;
        private SerializedProperty detectRigidbodies;
        private SerializedProperty detectStatic;
        private SerializedProperty ignoredColliders;
        
        private SerializedProperty buttonAxis;
        private SerializedProperty buttonThickness;
        private SerializedProperty triggerZone;

        private SerializedProperty returnRate;
        private SerializedProperty cooldown;

        private SerializedProperty isToggleButton;
        private SerializedProperty startToggledOn;
        private SerializedProperty toggleInPosition;

        private SerializedProperty fallbackCollider;
        private SerializedProperty fallbackPressTime;
        
        private SerializedProperty networkSync;
        private SerializedProperty masterOnly;
        private SerializedProperty transferReceiverOwnership;
        private SerializedProperty disableWhenNetworkClogged;
        
        private SerializedProperty playAudio;
        private SerializedProperty buttonSource;
        private SerializedProperty pressClip;
        private SerializedProperty releaseClip;
        
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
            isLocked = serializedObject.FindProperty(nameof(Pressable_Button.isLocked));
            
            detectPlayers = serializedObject.FindProperty(nameof(Pressable_Button.detectPlayers));
            detectRigidbodies = serializedObject.FindProperty(nameof(Pressable_Button.detectRigidbodies));
            detectStatic = serializedObject.FindProperty(nameof(Pressable_Button.detectStatic));
            ignoredColliders = serializedObject.FindProperty(nameof(Pressable_Button.ignoredColliders));
                
            buttonAxis = serializedObject.FindProperty(nameof(Pressable_Button.buttonAxis));
            buttonThickness = serializedObject.FindProperty(nameof(Pressable_Button.buttonThickness));
            triggerZone = serializedObject.FindProperty(nameof(Pressable_Button.triggerZone));

            returnRate = serializedObject.FindProperty(nameof(Pressable_Button.returnRate));
            cooldown = serializedObject.FindProperty(nameof(Pressable_Button.cooldown));

            isToggleButton = serializedObject.FindProperty(nameof(Pressable_Button.isToggleButton));
            startToggledOn = serializedObject.FindProperty(nameof(Pressable_Button.startToggledOn));
            toggleInPosition = serializedObject.FindProperty(nameof(Pressable_Button.toggleInPosition));
            
            fallbackCollider = serializedObject.FindProperty(nameof(Pressable_Button.fallbackCollider));
            fallbackPressTime = serializedObject.FindProperty(nameof(Pressable_Button.fallbackPressTime));
            
            networkSync = serializedObject.FindProperty(nameof(Pressable_Button.networkSync));
            masterOnly = serializedObject.FindProperty(nameof(Pressable_Button.masterOnly));
            transferReceiverOwnership = serializedObject.FindProperty(nameof(Pressable_Button.transferReceiverOwnership));
            disableWhenNetworkClogged = serializedObject.FindProperty(nameof(Pressable_Button.disableWhenNetworkClogged));
            
            playAudio = serializedObject.FindProperty(nameof(Pressable_Button.playAudio));
            buttonSource = serializedObject.FindProperty(nameof(Pressable_Button.buttonSource));
            pressClip = serializedObject.FindProperty(nameof(Pressable_Button.pressClip));
            releaseClip = serializedObject.FindProperty(nameof(Pressable_Button.releaseClip));
            
            applyTint = serializedObject.FindProperty(nameof(Pressable_Button.applyTint));
            tintRenderer = serializedObject.FindProperty(nameof(Pressable_Button.tintRenderer));
            tintShaderKeyword = serializedObject.FindProperty(nameof(Pressable_Button.tintShaderKeyword));
            offTint = serializedObject.FindProperty(nameof(Pressable_Button.offTint));
            inTint = serializedObject.FindProperty(nameof(Pressable_Button.inTint));
            onTint = serializedObject.FindProperty(nameof(Pressable_Button.onTint));
            lockedTint = serializedObject.FindProperty(nameof(Pressable_Button.lockedTint));
            
            applyTexture = serializedObject.FindProperty(nameof(Pressable_Button.applyTexture));
            textureRenderer = serializedObject.FindProperty(nameof(Pressable_Button.textureRenderer));
            textureShaderKeyword = serializedObject.FindProperty(nameof(Pressable_Button.textureShaderKeyword));
            offTexture = serializedObject.FindProperty(nameof(Pressable_Button.offTexture));
            inTexture = serializedObject.FindProperty(nameof(Pressable_Button.inTexture));
            onTexture = serializedObject.FindProperty(nameof(Pressable_Button.onTexture));
            lockedTexture = serializedObject.FindProperty(nameof(Pressable_Button.lockedTexture));
            
            enableAnimation = serializedObject.FindProperty(nameof(Pressable_Button.enableAnimation));  
            animator = serializedObject.FindProperty(nameof(Pressable_Button.animator));         
            progressParameter = serializedObject.FindProperty(nameof(Pressable_Button.progressParameter));
            
            enableHaptics = serializedObject.FindProperty(nameof(Pressable_Button.enableHaptics));
            hapticsDuration = serializedObject.FindProperty(nameof(Pressable_Button.hapticsDuration));
            hapticsAmplitude = serializedObject.FindProperty(nameof(Pressable_Button.hapticsAmplitude));
            hapticsFrequency = serializedObject.FindProperty(nameof(Pressable_Button.hapticsFrequency));

            udonReceivers = serializedObject.FindProperty(nameof(Pressable_Button.udonReceivers));
            #endregion
        }
        
        public override void OnInspectorGUI()
        {
            Pressable_Button button = (Pressable_Button)target;
            Shared_EditorUtility.DrawMainHeader(logo, target);
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            if (button.MissingSkeletonInfo)
                EditorGUILayout.HelpBox("No Player Skeleton Info found. This is a required component for player interactions!", MessageType.Error);
            
            GUILayout.Label("// RUNTIME & DETECTION", Shared_EditorUtility.BoldHeader);
            EditorGUILayout.PropertyField(isLocked);
            
            EditorGUILayout.Space();
            GUILayout.Label("What types of objects should this button detect?");
            EditorGUILayout.PropertyField(detectPlayers);
            EditorGUILayout.PropertyField(detectRigidbodies);
            EditorGUILayout.PropertyField(detectStatic);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Find Nearby Colliders To Ignore"))
                {
                    var found = Physics.OverlapSphere(button.transform.position, 0.25f);
                    var self = button.GetComponentsInChildren<Collider>();
                    for (int i = 0; i < found.Length; i++)
                    {
                        if (self.Contains(found[i])) continue;
                        TryAddToArray(ref button.ignoredColliders, found[i]);
                    }
                    button.ignoredColliders = button.ignoredColliders.Where(x => x != null).ToArray();
                    EditorUtility.SetDirty(button);
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Clean Up Nulls"))
                {
                    button.ignoredColliders = button.ignoredColliders.Where(x => x != null).ToArray();
                    EditorUtility.SetDirty(button);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(ignoredColliders);

            EditorGUILayout.Space(12);
            GUILayout.Label("// GENERAL SETTINGS", Shared_EditorUtility.BoldHeader);

            EditorGUILayout.PropertyField(buttonAxis);
            EditorGUILayout.PropertyField(buttonThickness);
            EditorGUILayout.PropertyField(triggerZone);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(cooldown);
            EditorGUILayout.PropertyField(returnRate);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(isToggleButton);

            if (button.isToggleButton)
            {
                EditorGUILayout.PropertyField(startToggledOn);
                EditorGUILayout.PropertyField(toggleInPosition);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(fallbackCollider);
            EditorGUILayout.PropertyField(fallbackPressTime);
            
            EditorGUILayout.Space(12);
            GUILayout.Label("// EVENTS", Shared_EditorUtility.BoldHeader);

            EditorGUILayout.BeginHorizontal();
            {
                button.sendPressedEvent = EditorGUILayout.ToggleLeft("Send Pressed Event", button.sendPressedEvent, EditorStyles.boldLabel, GUILayout.Width(150));
                if (button.sendPressedEvent)
                    button.pressedEventName = EditorGUILayout.TextField(button.pressedEventName);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                button.sendReleasedEvent = EditorGUILayout.ToggleLeft("Send Released Event", button.sendReleasedEvent, EditorStyles.boldLabel, GUILayout.Width(150));
                if (button.sendReleasedEvent)
                    button.releasedEventName = EditorGUILayout.TextField(button.releasedEventName);
            }
            EditorGUILayout.EndHorizontal();
            
            button.forceSendStateless = EditorGUILayout.ToggleLeft(new GUIContent("Force Send Stateless", "Send stateless events even if this is a toggle button (Without \"_On\" and \"_Off\")"), button.forceSendStateless);

            if (button.sendPressedEvent || button.sendReleasedEvent)
            {
                string eventsToSend = "Events that will be sent to receivers:\n";

                if ((!button.isToggleButton && button.sendPressedEvent) || 
                    (button.isToggleButton && button.forceSendStateless && button.sendPressedEvent)) 
                    eventsToSend += $"- {button.pressedEventName}\n";
            
                if ((!button.isToggleButton && button.sendReleasedEvent) || 
                    (button.isToggleButton && button.forceSendStateless && button.sendReleasedEvent)) 
                    eventsToSend += $"- {button.releasedEventName}\n";
            
                if (button.isToggleButton && button.sendPressedEvent)
                {
                    eventsToSend += $"- {button.pressedEventName}_On\n";
                    eventsToSend += $"- {button.pressedEventName}_Off\n";
                }

                if (button.isToggleButton && button.sendReleasedEvent)
                {
                    eventsToSend += $"- {button.releasedEventName}_On\n";
                    eventsToSend += $"- {button.releasedEventName}_Off\n";
                }
            
                EditorGUILayout.HelpBox(eventsToSend, MessageType.Info, true);
                EditorGUILayout.PropertyField(udonReceivers);
            }

            EditorGUILayout.Space(12);
            GUILayout.Label("// MODULES", Shared_EditorUtility.BoldHeader);

            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            button.networkSync = EditorGUILayout.ToggleLeft("Network Sync", button.networkSync, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Sync button state and events with other players.");
            EditorGUILayout.EndHorizontal();
            if (button.networkSync)
            {
                EditorGUILayout.PropertyField(masterOnly);
                EditorGUILayout.PropertyField(transferReceiverOwnership);
                EditorGUILayout.PropertyField(disableWhenNetworkClogged);   
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            button.playAudio = EditorGUILayout.ToggleLeft("Audio", button.playAudio, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Add audio to button events.");
            EditorGUILayout.EndHorizontal();
            if (button.playAudio)
            {
                EditorGUILayout.PropertyField(buttonSource);
                EditorGUILayout.PropertyField(pressClip);
                EditorGUILayout.PropertyField(releaseClip);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            button.applyTint = EditorGUILayout.ToggleLeft("Tint", button.applyTint, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Change a renderer's tint depending on button state.");
            EditorGUILayout.EndHorizontal();
            if (button.applyTint)
            {
                EditorGUILayout.PropertyField(tintRenderer);
                EditorGUILayout.PropertyField(tintShaderKeyword);
                EditorGUILayout.PropertyField(offTint);
                EditorGUILayout.PropertyField(inTint);
                if (button.isToggleButton)
                    EditorGUILayout.PropertyField(onTint);
                EditorGUILayout.PropertyField(lockedTint);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            button.applyTexture = EditorGUILayout.ToggleLeft("Texture", button.applyTexture, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Change a renderer's texture depending on button state.");
            EditorGUILayout.EndHorizontal();
            if (button.applyTexture)
            {
                EditorGUILayout.PropertyField(textureRenderer);
                EditorGUILayout.PropertyField(textureShaderKeyword);
                EditorGUILayout.PropertyField(offTexture);
                EditorGUILayout.PropertyField(inTexture);
                if (button.isToggleButton)
                    EditorGUILayout.PropertyField(onTexture);
                EditorGUILayout.PropertyField(lockedTexture);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            button.enableAnimation = EditorGUILayout.ToggleLeft("Animation", button.enableAnimation, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Control an animator's parameter with press progress.");
            EditorGUILayout.EndHorizontal();
            if (button.enableAnimation)
            {
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(progressParameter);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            button.enableHaptics = EditorGUILayout.ToggleLeft("Haptics", button.enableHaptics, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Trigger haptic feedback for users.");
            EditorGUILayout.EndHorizontal();
            if (button.enableHaptics)
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
                button.Wake();
            }
            if (GUILayout.Button("Force Sleep"))
            {
                button.Sleep();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            
            string text = "";
            text += $"Is Locked: {button.isLocked}\n";
            text += $"Is Toggled: {button.IsToggled}\n";
            text += $"Progress: {button.CurrentUnitProgress}\n";
            text += $"Sleeping: {button.IsSleeping}\n";
            EditorGUILayout.HelpBox(text, MessageType.None);
            
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(button);
            
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