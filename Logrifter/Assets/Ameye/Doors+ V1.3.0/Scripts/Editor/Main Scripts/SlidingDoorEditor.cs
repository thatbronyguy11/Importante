using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Malee.Editor;


namespace DoorsPlus
{
    [CustomEditor(typeof(SlidingDoor)), CanEditMultipleObjects]
    public class SlidingDoorEditor : Editor
    {
        static readonly string[] menuOptions = { "Rotations", "Sound" };
        
        private ReorderableList _slidingTimeline;

        private int _numberOfTriggers, _numberOfRotationBlocks, _toolBarIndex;

        private SerializedProperty _hingePositionProp, _doorScaleProp, _pivotPositionProp, _rotationWayProp, _angleConventionProp, _resetOnLeaveProp;

        public GameObject MoveTrigger, OpenTrigger, CloseTrigger;
        public GameObject DoorParent, RotationParent;

        readonly Color _orange = new Color32(255, 139, 0, 255);

        internal static GUIContent VersionLabel;
        internal static GUIStyle centeredVersionLabel;
        
        bool StylesNotLoaded = true;
        public void OnEnable()
        {
            SlidingDoor slidingDoor = target as SlidingDoor;

            _slidingTimeline = new ReorderableList(serializedObject.FindProperty("SlidingTimeline"), true, true, true, ReorderableList.ElementDisplayType.Expandable, "RotationIndex", null);

            _hingePositionProp = serializedObject.FindProperty("HingePosition");
            _doorScaleProp = serializedObject.FindProperty("DoorScale");
            _pivotPositionProp = serializedObject.FindProperty("PivotPosition");
            _rotationWayProp = serializedObject.FindProperty("RotationWay");
            _resetOnLeaveProp = serializedObject.FindProperty("ResetOnLeave");

            if (slidingDoor != null && slidingDoor.transform.parent == null)
            {
                // Create a parent with the same name as the door itself and reset it
                DoorParent = new GameObject(slidingDoor.gameObject.name);
                DoorParent.transform.localRotation = Quaternion.identity;
                DoorParent.transform.localPosition = Vector3.zero;
                DoorParent.transform.localScale = Vector3.one;
                slidingDoor.transform.SetParent(DoorParent.transform);
            }

            if (slidingDoor != null && (slidingDoor.transform.parent != null && slidingDoor.transform.parent.transform.name != slidingDoor.gameObject.name))
            {
                int siblingIndex = slidingDoor.transform.GetSiblingIndex();

                DoorParent = new GameObject(slidingDoor.gameObject.name);
                DoorParent.transform.localRotation = Quaternion.identity;
                DoorParent.transform.localPosition = Vector3.zero;
                DoorParent.transform.localScale = Vector3.one;
                DoorParent.transform.SetParent(slidingDoor.transform.parent);
                DoorParent.transform.SetSiblingIndex(siblingIndex);
                slidingDoor.transform.SetParent(DoorParent.transform);
            }

            // Loop through all the children of the parent object and check for triggers
            _numberOfTriggers = 0;

            if (slidingDoor.transform.parent != null)
            {

                _numberOfRotationBlocks = 0;
                if (slidingDoor != null && slidingDoor.SlidingTimeline != null) _numberOfRotationBlocks = slidingDoor.SlidingTimeline.Count;

                for (int x = 0; x < _numberOfRotationBlocks; x++)
                {
                    if (slidingDoor.transform.parent.Find("Move Trigger " + "(Slide " + (x + 1) + ")") != null ||
                        slidingDoor.transform.parent.Find("Open Trigger " + "(Slide " + (x + 1) + ")") != null ||
                        slidingDoor.transform.parent.Find("Close Trigger " + "(Slide " + (x + 1) + ")") != null) _numberOfTriggers += 1; //! this doesn't count the 'duplicate' trigger zones.
                }
            }


        }
        
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
            
            
            serializedObject.Update();
            SlidingDoor slidingDoor = target as SlidingDoor;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            _toolBarIndex = GUILayout.Toolbar(_toolBarIndex, menuOptions, new GUIStyle("LargeButton"));

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };

            switch (_toolBarIndex)
            {
                case 0:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<b>Sliding Settings</b>", style);
                    if (slidingDoor != null)
                    {
                        EditorGUILayout.PropertyField(_resetOnLeaveProp, new GUIContent("Reset On Leave"));
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        _slidingTimeline.DoLayoutList();
                        serializedObject.ApplyModifiedProperties();
                    }
                    break;

                case 1:
                    break;
                default: break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(VersionLabel, centeredVersionLabel);
            if (slidingDoor != null && slidingDoor.SlidingTimeline != null) _numberOfRotationBlocks = slidingDoor.SlidingTimeline.Count;

            if (Application.isPlaying) return;

            // Adding rotation blocks
            if (_numberOfTriggers < _numberOfRotationBlocks)
            {
                for (int index = 0; index < _numberOfRotationBlocks; index++)
                {
                    if (slidingDoor.SlidingTimeline != null && (slidingDoor.transform.parent.Find("Move Trigger " + "(Slide " + (index + 1) + ")") == null && slidingDoor.SlidingTimeline[index].Type == SlidingDoor.SlidingTimelineData.TypeOfSlide.SingleSlide))
                        CreateMoveTrigger(true, index);

                    if (slidingDoor.SlidingTimeline != null && slidingDoor.SlidingTimeline[index].Type == SlidingDoor.SlidingTimelineData.TypeOfSlide.LoopedSlide)
                    {
                        if (slidingDoor.transform.parent.Find("Open Trigger " + "(Slide " + (index + 1) + ")") == null || slidingDoor.transform.parent.Find("Close Trigger " + "(Slide " + (index + 1) + ")") == null)
                            CreateOpenCloseTriggers(true, index);
                    }
                }
            }

            // Changing rotation blocks
            for (int index = 0; index < _numberOfRotationBlocks; index++)
            {
                if (slidingDoor == null) continue;
                if (slidingDoor.SlidingTimeline == null) continue;

                slidingDoor.SlidingTimeline[index].RotationIndex = "Slide " + (index + 1).ToString(); // Name of the rotation block

                if (slidingDoor.SlidingTimeline[index].Type == SlidingDoor.SlidingTimelineData.TypeOfSlide.LoopedSlide)
                {
                    if (slidingDoor.transform.parent.Find("Move Trigger " + "(Slide " + (index + 1) + ")"))
                    {
                        if (!Application.isPlaying)
                        {
                            List<GameObject> objectsToRemove = new List<GameObject>();

                            foreach (Transform child in slidingDoor.transform.parent)
                            {

                                if (child.name == "Move Trigger " + "(Slide " + (index + 1) + ")") objectsToRemove.Add(child.gameObject);
                            }

                            foreach (GameObject child in objectsToRemove)
                            {
                                DestroyImmediate(child.gameObject);
                            }

                            CreateOpenCloseTriggers(false, index);
                        }
                    }
                }

                if (slidingDoor.SlidingTimeline[index].Type == SlidingDoor.SlidingTimelineData.TypeOfSlide.SingleSlide)
                {
                    if (slidingDoor.transform.parent.Find("Open Trigger " + "(Slide " + (index + 1) + ")") || slidingDoor.transform.parent.Find("Close Trigger " + "(Slide " + (index + 1) + ")"))
                    {
                        if (!Application.isPlaying)
                        {
                            List<GameObject> objectsToRemove = new List<GameObject>();

                            foreach (Transform child in slidingDoor.transform.parent)
                            {
                                if (child.name == "Open Trigger " + "(Slide " + (index + 1) + ")") objectsToRemove.Add(child.gameObject);
                                else if (child.name == "Close Trigger " + "(Slide " + (index + 1) + ")") objectsToRemove.Add(child.gameObject);
                            }

                            foreach (GameObject child in objectsToRemove)
                            {
                                DestroyImmediate(child.gameObject);
                            }

                            CreateMoveTrigger(false, index);
                        }
                    }
                }
            }

            if (_numberOfTriggers <= _numberOfRotationBlocks) return;

            // Removing Rotation blocks
            while (_numberOfTriggers > _numberOfRotationBlocks)
            {
                if (slidingDoor != null && slidingDoor.transform.parent.Find("Move Trigger " + "(Slide " + _numberOfTriggers + ")") != null)
                {
                    foreach (Transform child in slidingDoor.transform.parent.transform)
                    {
                        if (child.name == "Move Trigger " + "(Slide " + _numberOfTriggers + ")") DestroyImmediate(child.gameObject);
                    }
                    _numberOfTriggers--;
                }

                else if (slidingDoor.transform.parent.Find("Open Trigger " + "(Slide " + _numberOfTriggers + ")") != null || slidingDoor.transform.parent.Find("Close Trigger " + "(Slide " + _numberOfTriggers + ")") != null)
                {

                    if (!Application.isPlaying)
                    {
                        List<GameObject> objectsToRemove = new List<GameObject>();

                        foreach (Transform child in slidingDoor.transform.parent)
                        {
                            if (child.name == "Open Trigger " + "(Slide " + _numberOfTriggers + ")") objectsToRemove.Add(child.gameObject);
                            else if (child.name == "Close Trigger " + "(Slide " + _numberOfTriggers + ")") objectsToRemove.Add(child.gameObject);

                        }

                        foreach (GameObject child in objectsToRemove)
                        {
                            DestroyImmediate(child.gameObject);
                        }
                        _numberOfTriggers--;

                    }
                }

                else if (slidingDoor.transform.parent.Find("Move Trigger " + "(Slide " + _numberOfTriggers + ")") == null || slidingDoor.transform.parent.Find("Open Trigger " + "(Slide " + _numberOfTriggers + ")") == null || slidingDoor.transform.parent.Find("Close Trigger " + "(Slide " + _numberOfTriggers + ")") == null) continue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateMoveTrigger(bool addToNumberOfTriggers, int index)
        {
            SlidingDoor slidingDoor = target as SlidingDoor;
            if (slidingDoor == null) throw new ArgumentNullException("SlidingDoor");

            GameObject MoveTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MoveTrigger.name = "Move Trigger " + "(Slide " + (index + 1) + ")";

            ResetTransform(MoveTrigger, slidingDoor);
            SetParentChild(slidingDoor.transform.parent.gameObject, MoveTrigger);
            AddTriggerScript(MoveTrigger, index);
            SetColor(MoveTrigger);

            if (addToNumberOfTriggers) _numberOfTriggers += 1;
        }

        private void CreateOpenCloseTriggers(bool AddToNumberOfTriggers, int index)
        {
            SlidingDoor slidingDoor = target as SlidingDoor;

            GameObject OpenTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            OpenTrigger.name = "Open Trigger " + "(Slide " + (index + 1) + ")";

            GameObject CloseTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            CloseTrigger.name = "Close Trigger " + "(Slide " + (index + 1) + ")";

            ResetTransform(OpenTrigger, slidingDoor);
            SetParentChild(slidingDoor.transform.parent.gameObject, OpenTrigger);
            AddTriggerScript(OpenTrigger, index);

            ResetTransform(CloseTrigger, slidingDoor);
            SetParentChild(slidingDoor.transform.parent.gameObject, CloseTrigger);
            AddTriggerScript(CloseTrigger, index);
            SetColor(OpenTrigger);
            SetColor(CloseTrigger);

            if (AddToNumberOfTriggers) _numberOfTriggers += 1;
        }

        private static void SetParentChild(GameObject Parent, GameObject Trigger)
        {
            Trigger.transform.parent = Parent.transform;
        }

        private static void ResetTransform(GameObject obj, SlidingDoor SlidingDoor)
        {
            if (obj.name == "Move Trigger")
            {
                obj.transform.position = SlidingDoor.gameObject.transform.position + new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                obj.transform.rotation = SlidingDoor.gameObject.transform.rotation;
            }

            if (obj.name == "Open Trigger")
            {
                obj.transform.position = SlidingDoor.gameObject.transform.position - new Vector3(1, 0, 0) + new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                obj.transform.rotation = SlidingDoor.gameObject.transform.rotation;
            }

            if (obj.name == "Close Trigger")
            {
                obj.transform.position = SlidingDoor.gameObject.transform.position + new Vector3(1, 0, 0) + new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                obj.transform.rotation = SlidingDoor.gameObject.transform.rotation;
            }
        }

        private static void AddTriggerScript(GameObject Trigger, int index)
        {
            Trigger.AddComponent<DoorTrigger>();
            Trigger.GetComponent<DoorTrigger>().id = index;
        }

        private static void SetColor(GameObject Trigger)
        {
            string material = "";

            switch (Trigger.name.Split().First())
            {
                case "Move":
                    material = "Assets/Ameye/Doors+ V1.3.0/Resources/Move_Trigger_Zone.mat";
                    break;
                case "Open":
                    material = "Assets/Ameye/Doors+ V1.3.0/Resources/Open_Trigger_Zone.mat";
                    break;
                case "Close":
                    material = "Assets/Ameye/Doors+ V1.3.0/Resources/Close_Trigger_Zone.mat";
                    break;
            }

            Material mat = (Material)AssetDatabase.LoadAssetAtPath(material, typeof(Material));
            Trigger.GetComponent<Renderer>().material = mat;
        }

        static GUIContent IconContent(string text, string icon, string tooltip)
        {
            Texture2D cached = (Texture2D)Resources.Load("Icons/" + icon);  
            return new GUIContent(text, cached, tooltip);
        }
    }
}