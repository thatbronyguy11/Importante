using UnityEngine;
using UnityEditor;


namespace DoorsPlus
{
    [CustomEditor(typeof(DoorDetection))]
    public class DoorDetectionEditor : Editor
    {
        internal static GUIContent VersionLabel;
        internal static GUIStyle centeredVersionLabel;
        bool StylesNotLoaded = true;
        void LoadStyles()
        {
            VersionLabel = IconContent("v1.3.0", "", "");
            
            centeredVersionLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)

            };
            
            StylesNotLoaded = false;
        }
        
        public override void OnInspectorGUI()
        {
            if (StylesNotLoaded) LoadStyles();
            
            DoorDetection doorDetection = target as DoorDetection;

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<b>UI Settings</b>", style);
            if (doorDetection != null)
            {
                doorDetection.LookingAtPrefab = (GameObject)EditorGUILayout.ObjectField("Looking at", doorDetection.LookingAtPrefab, typeof(GameObject), true);
                doorDetection.InTriggerZoneLookingAtPrefab = (GameObject)EditorGUILayout.ObjectField("In zone", doorDetection.InTriggerZoneLookingAtPrefab, typeof(GameObject), true);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<b>Raycast Settings</b>", style);
                doorDetection.cam = EditorGUILayout.ObjectField("Camera", doorDetection.cam, typeof(Camera), true) as Camera;
                doorDetection.Reach = EditorGUILayout.FloatField("Reach", doorDetection.Reach);
                doorDetection.DebugRay = EditorGUILayout.Toggle("Debug Ray", doorDetection.DebugRay);
                if (doorDetection.DebugRay)
                {
                    doorDetection.DebugRayColor = EditorGUILayout.ColorField("Color", doorDetection.DebugRayColor);
                    doorDetection.DebugRayColorAlpha =
                        EditorGUILayout.Slider("Opacity", doorDetection.DebugRayColorAlpha, 0, 1);
                    doorDetection.DebugRayColor.a = doorDetection.DebugRayColorAlpha;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(VersionLabel, centeredVersionLabel);
        }
        
        static GUIContent IconContent(string text, string icon, string tooltip)
        {
            Texture2D cached = (Texture2D)Resources.Load("Icons/" + icon);  
            return new GUIContent(text, cached, tooltip);
        }
    }
}