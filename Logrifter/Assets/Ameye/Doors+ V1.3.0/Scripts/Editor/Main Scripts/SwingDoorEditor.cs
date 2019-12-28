using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Malee.Editor;


namespace DoorsPlus
{
    [CustomEditor(typeof(SwingDoor)), CanEditMultipleObjects]
    public class SwingDoorEditor : Editor
    {
        private ReorderableList _rotationTimeline;

        private int _numberOfTriggers, _numberOfRotationBlocks, _toolBarIndex;

        private SerializedProperty _hingePositionProp, _doorScaleProp, _pivotPositionProp, _rotationWayProp, _angleConventionProp, _resetOnLeaveProp;

        public GameObject FrontTrigger, BackTrigger;
        public GameObject DoorParent, RotationParent;

        readonly Color _orange = new Color32(255, 139, 0, 255);
        
        internal static GUIContent VersionLabel;
        internal static GUIStyle centeredVersionLabel;
        
        bool StylesNotLoaded = true;

        public void OnEnable()
        {
            SwingDoor swingDoor = target as SwingDoor;

            _rotationTimeline = new ReorderableList(serializedObject.FindProperty("RotationTimeline"), true, true, true, ReorderableList.ElementDisplayType.Expandable, "RotationIndex", null);

            _hingePositionProp = serializedObject.FindProperty("HingePosition");
            _doorScaleProp = serializedObject.FindProperty("DoorScale");
            _pivotPositionProp = serializedObject.FindProperty("PivotPosition");
            _rotationWayProp = serializedObject.FindProperty("RotationWay");
            _resetOnLeaveProp = serializedObject.FindProperty("ResetOnLeave");

            if (swingDoor != null && swingDoor.transform.parent == null)
            {
                // Create a parent with the same name as the door itself and reset it
                DoorParent = new GameObject(swingDoor.gameObject.name);
                DoorParent.transform.localRotation = Quaternion.identity;
                DoorParent.transform.localPosition = Vector3.zero;
                DoorParent.transform.localScale = Vector3.one;
                swingDoor.transform.SetParent(DoorParent.transform);
            }

            if (swingDoor != null && (swingDoor.transform.parent != null && swingDoor.transform.parent.transform.name != swingDoor.gameObject.name))
            {
                int siblingIndex = swingDoor.transform.GetSiblingIndex();

                DoorParent = new GameObject(swingDoor.gameObject.name);
                DoorParent.transform.localRotation = Quaternion.identity;
                DoorParent.transform.localPosition = Vector3.zero;
                DoorParent.transform.localScale = Vector3.one;
                DoorParent.transform.SetParent(swingDoor.transform.parent);
                DoorParent.transform.SetSiblingIndex(siblingIndex);
                swingDoor.transform.SetParent(DoorParent.transform);
            }

            // Loop through all the children of the parent object and check for triggers
            _numberOfTriggers = 0;

            if (swingDoor.transform.parent != null)
            {

                _numberOfRotationBlocks = 0;
                if (swingDoor != null && swingDoor.RotationTimeline != null) _numberOfRotationBlocks = swingDoor.RotationTimeline.Count;

                for (int x = 0; x < _numberOfRotationBlocks; x++)
                {
                    if (swingDoor.transform.parent.Find("Front Trigger " + "(Rotation " + (x + 1) + ")") != null ||
                        swingDoor.transform.parent.Find("Back Trigger " + "(Rotation " + (x + 1) + ")") != null) _numberOfTriggers += 1; //! this doesn't count the 'duplicate' trigger zones.

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
            // Debug.Log("#triggers: " + _numberOfTriggers);
            if (StylesNotLoaded) LoadStyles();
            serializedObject.Update();
            SwingDoor swingDoor = target as SwingDoor;

            EditorGUILayout.Space();

            _toolBarIndex = GUILayout.Toolbar(_toolBarIndex, new string[] { "Door", "Rotations" }, new GUIStyle("LargeButton"));

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };

            switch (_toolBarIndex)
            {
                //Door and hinge settings
                case 0:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<b>Door Settings</b>", style);
                    EditorGUILayout.PropertyField(_doorScaleProp, new GUIContent("Scale"));
                    EditorGUILayout.PropertyField(_pivotPositionProp, new GUIContent("Pivot Position"));
                    EditorGUILayout.Space();
                    if (swingDoor != null && (swingDoor.DoorScale == SwingDoor.ScaleOfDoor.Unity3DUnits && swingDoor.PivotPosition == SwingDoor.PositionOfPivot.Centered))
                    {
                        EditorGUILayout.LabelField("<b>Hinge Settings</b>", style);
                        EditorGUILayout.PropertyField(_hingePositionProp, new GUIContent("Position"));
                        EditorGUILayout.Space();
                    }

                    if (swingDoor != null && (swingDoor.DoorScale == SwingDoor.ScaleOfDoor.Other && swingDoor.PivotPosition == SwingDoor.PositionOfPivot.Centered))
                        EditorGUILayout.HelpBox("If your door is not scaled in Unity3D units and the pivot position is not already positioned correctly, the hinge algorithm will not work as expected.", MessageType.Error);

                    else if (Tools.pivotMode == PivotMode.Center)
                        EditorGUILayout.HelpBox("Make sure the tool handle is placed at the active object's pivot point.", MessageType.Warning);
                    serializedObject.ApplyModifiedProperties();
                    /*EditorGUILayout.Space();
                    EditorGUILayout.LabelField(Styles.VersionLabel, Styles.centeredVersionLabel);
                    serializedObject.ApplyModifiedProperties();*/
                    break;

                case 1:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<b>Rotation Settings</b>", style);
                    EditorGUILayout.PropertyField(_rotationWayProp, new GUIContent("Rotation"));
                    if (swingDoor != null)
                    {

                        EditorGUILayout.PropertyField(_resetOnLeaveProp, new GUIContent("Reset On Leave"));

                        _rotationTimeline.DoLayoutList();

                        if (swingDoor.RotationTimeline != null)
                        {
                            /* for (int x = 0; x < swingDoor.RotationTimeline.Count; x++)
                             {
                                 if ((swingDoor.RotationTimeline[x].FinalAngle -
                                      swingDoor.RotationTimeline[x].InitialAngle) >= 360)
                                     EditorGUILayout.HelpBox(
                                         "The difference between FinalAngle and InitialAngle should not exceed 360°. (See rotation " +
                                         (x + 1) + ")", MessageType.Warning);
                             }*/
                        }
                        serializedObject.ApplyModifiedProperties();
                        // EditorGUILayout.Space();
                        //EditorGUILayout.LabelField(Styles.VersionLabel, Styles.centeredVersionLabel);

                    }
                    // serializedObject.ApplyModifiedProperties();
                    break;
                default: break;


            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(VersionLabel, centeredVersionLabel);


            if (swingDoor != null && swingDoor.RotationTimeline != null) _numberOfRotationBlocks = swingDoor.RotationTimeline.Count;

            if (Application.isPlaying) return;

            // Adding rotation blocks
            if (_numberOfTriggers < _numberOfRotationBlocks)
            {
                for (int index = 0; index < _numberOfRotationBlocks; index++)
                {
                    if (swingDoor.RotationTimeline != null)
                    {
                        if (swingDoor.transform.parent.Find("Front Trigger " + "(Rotation " + (index + 1) + ")") == null || swingDoor.transform.parent.Find("Back Trigger " + "(Rotation " + (index + 1) + ")") == null)
                            CreateFrontBackTriggers(true, index);
                    }
                }
            }

            // Changing rotation blocks
            for (int index = 0; index < _numberOfRotationBlocks; index++)
            {
                if (swingDoor == null) continue;
                if (swingDoor.RotationTimeline == null) continue;

                swingDoor.RotationTimeline[index].RotationIndex = "Rotation " + (index + 1).ToString(); // Name of the rotation block
            }

            if (_numberOfTriggers <= _numberOfRotationBlocks) return;

            // Removing Rotation blocks
            while (_numberOfTriggers > _numberOfRotationBlocks)
            {

                if (swingDoor.transform.parent.Find("Front Trigger " + "(Rotation " + _numberOfTriggers + ")") != null || swingDoor.transform.parent.Find("Back Trigger " + "(Rotation " + _numberOfTriggers + ")") != null)
                {

                    if (!Application.isPlaying)
                    {
                        List<GameObject> objectsToRemove = new List<GameObject>();

                        foreach (Transform child in swingDoor.transform.parent)
                        {
                            if (child.name == "Front Trigger " + "(Rotation " + _numberOfTriggers + ")") objectsToRemove.Add(child.gameObject);
                            else if (child.name == "Back Trigger " + "(Rotation " + _numberOfTriggers + ")") objectsToRemove.Add(child.gameObject);

                        }

                        foreach (GameObject child in objectsToRemove)
                        {
                            DestroyImmediate(child.gameObject);
                        }
                        _numberOfTriggers--;

                    }
                }

                else if (swingDoor.transform.parent.Find("Front Trigger " + "(Rotation " + _numberOfTriggers + ")") == null || swingDoor.transform.parent.Find("Back Trigger " + "(Rotation " + _numberOfTriggers + ")") == null) continue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateFrontBackTriggers(bool AddToNumberOfTriggers, int index)
        {
            SwingDoor swingDoor = target as SwingDoor;

            GameObject FrontTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            FrontTrigger.name = "Front Trigger " + "(Rotation " + (index + 1) + ")";


            GameObject BackTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            BackTrigger.name = "Back Trigger " + "(Rotation " + (index + 1) + ")";


            ResetTransform(FrontTrigger, swingDoor);
            SetParentChild(swingDoor.transform.parent.gameObject, FrontTrigger);
            AddTriggerScript(FrontTrigger, index);


            ResetTransform(BackTrigger, swingDoor);
            SetParentChild(swingDoor.transform.parent.gameObject, BackTrigger);
            AddTriggerScript(BackTrigger, index);

            SetColor(FrontTrigger);
            SetColor(BackTrigger);
         //   FrontTrigger.tag = "TriggerZone";
           // BackTrigger.tag = "TriggerZone";

            if (AddToNumberOfTriggers) _numberOfTriggers += 1;
        }

        private static void SetParentChild(GameObject Parent, GameObject Trigger)
        {
            // Parent.transform.parent = Selection.activeGameObject.transform.parent.transform;
            Trigger.transform.parent = Parent.transform;
        }

        private static void ResetTransform(GameObject obj, SwingDoor SwingDoor)
        {
            if (obj.name == "Front Trigger")
            {
                obj.transform.position = SwingDoor.gameObject.transform.position - new Vector3(1, 0, 0) + new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                //obj.transform.localScale = new Vector3(2, SwingDoor.gameObject.transform.localScale.y + 0.25f, 2);
                obj.transform.rotation = SwingDoor.gameObject.transform.rotation;
            }

            if (obj.name == "Back Trigger")
            {
                obj.transform.position = SwingDoor.gameObject.transform.position + new Vector3(1, 0, 0) + new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                //obj.transform.localScale = new Vector3(2, SwingDoor.gameObject.transform.localScale.y + 0.25f, 2);
                obj.transform.rotation = SwingDoor.gameObject.transform.rotation;
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
                case "Front":
                    material = "Assets/Ameye/Doors+ V1.3.0/Resources/Front_Trigger_Zone.mat";
                    break;
                case "Back":
                    material = "Assets/Ameye/Doors+ V1.3.0/Resources/Back_Trigger_Zone.mat";
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
