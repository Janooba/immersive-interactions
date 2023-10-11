using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JanoobaAssets
{
    public static class Shared_EditorUtility
    {
        public const string VERSION = "v0.2.6-beta.5";
        
        private static GUIStyle _boldHeader = null;
        public static GUIStyle BoldHeader
        {
            get
            {
                if (_boldHeader == null)
                {
                    _boldHeader = new GUIStyle(EditorStyles.boldLabel);
                    _boldHeader.fontSize = 16;
                }

                return _boldHeader;
            }
        }

        public static void DrawMainHeader(Texture header, UnityEngine.Object target)
        {
            if (UdonSharpEditor.UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();

            DrawLogo(header);
            
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = new Color(1, 0.6f, 0, 1f);
            style.alignment = TextAnchor.MiddleRight;
            EditorGUILayout.LabelField($"{VERSION}", style);
        }
        
        public static void DrawLogo(Texture logo)
        {
            Vector2 contentOffset = new Vector2(0f, -2f);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 150;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(300f, 140f, style);
            GUI.Box(rect, logo, style);
        }
    }
}