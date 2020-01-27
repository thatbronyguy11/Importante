using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Malee.Editor;


namespace DoorsPlus
{
    [CustomEditor(typeof(DefaultDoor)), CanEditMultipleObjects]
    public class DefaultDoorEditor : Editor
    {
        private ReorderableList _rotationTimeline;

        private int _numberOfTriggers, _numberOfRotationBlocks;
        int ToolBarIndex;

        private SerializedProperty _hingePositionProp,
            _doorScaleProp,
            _pivotPositionProp,
            _rotationWayProp,
            _angleConventionProp,
            _resetOnLeaveProp;

        private SerializedProperty _openingClipProp, _openingVolumeProp, _openingPitchProp, _openingOffsetProp;
        private SerializedProperty _openedClipProp, _openedVolumeProp, _openedPitchProp, _openedOffsetProp;
        private SerializedProperty _closingClipProp, _closingVolumeProp, _closingPitchProp, _closingOffsetProp;
        private SerializedProperty _closedClipProp, _closedVolumeProp, _closedPitchProp, _closedOffsetProp;
        private SerializedProperty _lockedCLipProp, _lockedVolumeProp, _lockedPitchProp, _lockedOffsetProp;
        private SerializedProperty p_currentAudioClips;
        private GUIStyle boldLabelStyle;
        public GameObject MoveTrigger, OpenTrigger, CloseTrigger;
        public GameObject DoorParent, RotationParent;
        private string[] menuOptions;
        readonly Color _orange = new Color32(255, 139, 0, 255);

        internal static GUIContent VersionLabel;
        internal static GUIStyle centeredVersionLabel;
        
        bool StylesNotLoaded = true;
        public void OnEnable()
        {
            ToolBarIndex = 1;
            DefaultDoor defaultDoor = target as DefaultDoor;



            _rotationTimeline = new ReorderableList(serializedObject.FindProperty("RotationTimeline"), true, true, true,
                ReorderableList.ElementDisplayType.Expandable, "RotationIndex", null);

            _hingePositionProp = serializedObject.FindProperty("HingePosition");
            _doorScaleProp = serializedObject.FindProperty("DoorScale");
            _pivotPositionProp = serializedObject.FindProperty("PivotPosition");
            _rotationWayProp = serializedObject.FindProperty("RotationWay");
            _resetOnLeaveProp = serializedObject.FindProperty("ResetOnLeave");

            _openingClipProp = serializedObject.FindProperty("OpeningClip");
            _openingVolumeProp = serializedObject.FindProperty("OpeningVolume");
            _openingPitchProp = serializedObject.FindProperty("OpeningPitch");
            _openingOffsetProp = serializedObject.FindProperty("OpeningOffset");
            // mixerProp = serializedObject.FindProperty("mixer");
            _openedClipProp = serializedObject.FindProperty("OpenedClip");
            _openedVolumeProp = serializedObject.FindProperty("OpenedVolume");
            _openedPitchProp = serializedObject.FindProperty("OpenedPitch");
            _openedOffsetProp = serializedObject.FindProperty("OpenedOffset");

            _closingClipProp = serializedObject.FindProperty("ClosingClip");
            _closingVolumeProp = serializedObject.FindProperty("ClosingVolume");
            _closingPitchProp = serializedObject.FindProperty("ClosingPitch");
            _closingOffsetProp = serializedObject.FindProperty("ClosingOffset");

            _closedClipProp = serializedObject.FindProperty("ClosedClip");
            _closedVolumeProp = serializedObject.FindProperty("ClosedVolume");
            _closedPitchProp = serializedObject.FindProperty("ClosedPitch");
            _closedOffsetProp = serializedObject.FindProperty("ClosedOffset");

            _lockedCLipProp = serializedObject.FindProperty("LockedClip");
            _lockedVolumeProp = serializedObject.FindProperty("LockedVolume");
            _lockedPitchProp = serializedObject.FindProperty("LockedPitch");
            _lockedOffsetProp = serializedObject.FindProperty("LockedOffset");

            p_currentAudioClips = serializedObject.FindProperty("CurrentAudioClips");

            if (defaultDoor != null && defaultDoor.transform.parent == null)
            {
                // Create a parent with the same name as the door itself and reset it
                DoorParent = new GameObject(defaultDoor.gameObject.name);
                DoorParent.transform.localRotation = Quaternion.identity;
                DoorParent.transform.localPosition = Vector3.zero;
                DoorParent.transform.localScale = Vector3.one;
                defaultDoor.transform.SetParent(DoorParent.transform);
            }

            if (defaultDoor != null && (defaultDoor.transform.parent != null &&
                                        defaultDoor.transform.parent.transform.name != defaultDoor.gameObject.name))
            {
                int siblingIndex = defaultDoor.transform.GetSiblingIndex();

                DoorParent = new GameObject(defaultDoor.gameObject.name);
                DoorParent.transform.localRotation = Quaternion.identity;
                DoorParent.transform.localPosition = Vector3.zero;
                DoorParent.transform.localScale = Vector3.one;
                DoorParent.transform.SetParent(defaultDoor.transform.parent);
                DoorParent.transform.SetSiblingIndex(siblingIndex);
                defaultDoor.transform.SetParent(DoorParent.transform);
            }

            // Loop through all the children of the parent object and check for triggers
            _numberOfTriggers = 0;

            if (defaultDoor.transform.parent != null)
            {
                _numberOfRotationBlocks = 0;
                if (defaultDoor != null && defaultDoor.RotationTimeline != null)
                    _numberOfRotationBlocks = defaultDoor.RotationTimeline.Count;

                for (int x = 0; x < _numberOfRotationBlocks; x++)
                {
                    if (defaultDoor.transform.parent.Find("Move Trigger " + "(Rotation " + (x + 1) + ")") != null ||
                        defaultDoor.transform.parent.Find("Open Trigger " + "(Rotation " + (x + 1) + ")") != null ||
                        defaultDoor.transform.parent.Find("Close Trigger " + "(Rotation " + (x + 1) + ")") != null)
                        _numberOfTriggers += 1; //! this doesn't count the 'duplicate' trigger zones.
                }
            }


            menuOptions = new string[3];
            menuOptions[0] = "Door";
            menuOptions[1] = "Rotations";
            menuOptions[2] = "Sound";
            
            
        }

        void LoadStyles()
        {

            boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };
            
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
            DefaultDoor defaultDoor = target as DefaultDoor;

            EditorGUILayout.Space();


            ToolBarIndex = GUILayout.Toolbar(ToolBarIndex, menuOptions, new GUIStyle("LargeButton"));


            switch (ToolBarIndex)
            {
                //Door and hinge settings
                case 0:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<b>Door Settings</b>", boldLabelStyle);
                    EditorGUILayout.PropertyField(_doorScaleProp, new GUIContent("Scale"));
                    EditorGUILayout.PropertyField(_pivotPositionProp, new GUIContent("Pivot Position"));
                    EditorGUILayout.Space();
                    if (defaultDoor != null && (defaultDoor.DoorScale == DefaultDoor.ScaleOfDoor.Unity3DUnits &&
                                                defaultDoor.PivotPosition == DefaultDoor.PositionOfPivot.Centered))
                    {
                        EditorGUILayout.LabelField("<b>Hinge Settings</b>", boldLabelStyle);
                        EditorGUILayout.PropertyField(_hingePositionProp, new GUIContent("Position"));
                        EditorGUILayout.Space();
                    }

                    if (defaultDoor != null && (defaultDoor.DoorScale == DefaultDoor.ScaleOfDoor.Other &&
                                                defaultDoor.PivotPosition == DefaultDoor.PositionOfPivot.Centered))
                        EditorGUILayout.HelpBox(
                            "If your door is not scaled in Unity3D units and the pivot position is not already positioned correctly, the hinge algorithm will not work as expected.",
                            MessageType.Error);

                    else if (Tools.pivotMode == PivotMode.Center)
                        EditorGUILayout.HelpBox("Make sure the tool handle is placed at the active object's pivot point.",
                            MessageType.Warning);
                    serializedObject.ApplyModifiedProperties();
                    /*EditorGUILayout.Space();
                    EditorGUILayout.LabelField(Styles.VersionLabel, Styles.centeredVersionLabel);
                    serializedObject.ApplyModifiedProperties();*/
                    break;

                case 1:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<b>Rotation Settings</b>", boldLabelStyle);
                    EditorGUILayout.PropertyField(_rotationWayProp, new GUIContent("Rotation"));
                    if (defaultDoor != null)
                    {
                        EditorGUILayout.PropertyField(_resetOnLeaveProp, new GUIContent("Reset On Leave"));

                        _rotationTimeline.DoLayoutList();

                        if (defaultDoor.RotationTimeline != null)
                        {
                            for (int x = 0; x < defaultDoor.RotationTimeline.Count; x++)
                            {
                                if ((defaultDoor.RotationTimeline[x].FinalAngle -
                                     defaultDoor.RotationTimeline[x].InitialAngle) >= 360)
                                    EditorGUILayout.HelpBox(
                                        "The difference between FinalAngle and InitialAngle should not exceed 360°. (See rotation " +
                                        (x + 1) + ")", MessageType.Warning);
                            }
                        }


                        // EditorGUILayout.Space();
                        //EditorGUILayout.LabelField(Styles.VersionLabel, Styles.centeredVersionLabel);
                    }
                    serializedObject.ApplyModifiedProperties();

                    // 
                    break;

                case 2:

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(p_currentAudioClips, new GUIContent("Rotation State"));
                    if (defaultDoor.CurrentAudioClips == DefaultDoor.AudioClips.Opening)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b>Opening</b>", boldLabelStyle);
                        EditorGUILayout.PropertyField(_openingClipProp, new GUIContent("Clip"));
                        EditorGUILayout.PropertyField(_openingVolumeProp, new GUIContent("Volume"));
                        EditorGUILayout.PropertyField(_openingPitchProp, new GUIContent("Playback Speed"));
                        EditorGUILayout.PropertyField(_openingOffsetProp, new GUIContent("Delay"));
                        // EditorGUILayout.PropertyField(mixerProp, new GUIContent("Mixer Group"));


                        EditorGUILayout.LabelField("<b>Opened</b>", boldLabelStyle);
                        EditorGUILayout.PropertyField(_openedClipProp, new GUIContent("Clip"));
                        EditorGUILayout.PropertyField(_openedVolumeProp, new GUIContent("Volume"));
                        EditorGUILayout.PropertyField(_openedPitchProp, new GUIContent("Playback Speed"));
                        EditorGUILayout.PropertyField(_openedOffsetProp, new GUIContent("Delay"));

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Preview Audio"))
                            if (defaultDoor != null)
                                defaultDoor.Preview("Open");
                    }

                    else if (defaultDoor.CurrentAudioClips == DefaultDoor.AudioClips.Closing)
                    {

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b>Closing</b>", boldLabelStyle);
                        EditorGUILayout.PropertyField(_closingClipProp, new GUIContent("Clip"));
                        EditorGUILayout.PropertyField(_closingVolumeProp, new GUIContent("Volume"));
                        EditorGUILayout.PropertyField(_closingPitchProp, new GUIContent("Playback Speed"));
                        EditorGUILayout.PropertyField(_closingOffsetProp, new GUIContent("Delay"));


                        EditorGUILayout.LabelField("<b>Closed</b>", boldLabelStyle);
                        EditorGUILayout.PropertyField(_closedClipProp, new GUIContent("Clip"));
                        EditorGUILayout.PropertyField(_closedVolumeProp, new GUIContent("Volume"));
                        EditorGUILayout.PropertyField(_closedPitchProp, new GUIContent("Playback Speed"));
                        EditorGUILayout.PropertyField(_closedOffsetProp, new GUIContent("Delay"));

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Preview Audio")) defaultDoor.Preview("Close");
                    }

                    else if (defaultDoor.CurrentAudioClips == DefaultDoor.AudioClips.Locked)
                    {

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b>Locked</b>", boldLabelStyle);
                        EditorGUILayout.PropertyField(_lockedCLipProp, new GUIContent("Clip"));
                        EditorGUILayout.PropertyField(_lockedVolumeProp, new GUIContent("Volume"));
                        EditorGUILayout.PropertyField(_lockedPitchProp, new GUIContent("Playback Speed"));
                        EditorGUILayout.PropertyField(_lockedOffsetProp, new GUIContent("Delay"));



                        if (GUILayout.Button("Preview Audio")) defaultDoor.Preview("Lock");


                    }
                    serializedObject.ApplyModifiedProperties();

                    break;
                default: break;

            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(VersionLabel, centeredVersionLabel);


            if (defaultDoor != null && defaultDoor.RotationTimeline != null)
                _numberOfRotationBlocks = defaultDoor.RotationTimeline.Count;

            if (Application.isPlaying) return;

            // Adding rotation blocks
            if (_numberOfTriggers < _numberOfRotationBlocks)
            {
                for (int index = 0; index < _numberOfRotationBlocks; index++)
                {
                    if (defaultDoor.RotationTimeline != null &&
                        (defaultDoor.transform.parent.Find("Move Trigger " + "(Rotation " + (index + 1) + ")") == null &&
                         defaultDoor.RotationTimeline[index].RotationType ==
                         DefaultDoor.RotationTimelineData.TypeOfRotation.SingleRotation))
                        CreateMoveTrigger(true, index);

                    if (defaultDoor.RotationTimeline != null && defaultDoor.RotationTimeline[index].RotationType ==
                        DefaultDoor.RotationTimelineData.TypeOfRotation.LoopedRotation)
                    {
                        if (defaultDoor.transform.parent.Find("Open Trigger " + "(Rotation " + (index + 1) + ")") == null ||
                            defaultDoor.transform.parent.Find("Close Trigger " + "(Rotation " + (index + 1) + ")") == null)
                            CreateOpenCloseTriggers(true, index);
                    }
                }
            }

            // Changing rotation blocks
            for (int index = 0; index < _numberOfRotationBlocks; index++)
            {
                if (defaultDoor == null) continue;
                if (defaultDoor.RotationTimeline == null) continue;

                defaultDoor.RotationTimeline[index].RotationIndex =
                    "Rotation " + (index + 1).ToString(); // Name of the rotation block

                if (defaultDoor.RotationTimeline[index].RotationType ==
                    DefaultDoor.RotationTimelineData.TypeOfRotation.LoopedRotation)
                {
                    if (defaultDoor.transform.parent.Find("Move Trigger " + "(Rotation " + (index + 1) + ")"))
                    {
                        if (!Application.isPlaying)
                        {
                            List<GameObject> objectsToRemove = new List<GameObject>();

                            foreach (Transform child in defaultDoor.transform.parent)
                            {
                                if (child.name == "Move Trigger " + "(Rotation " + (index + 1) + ")")
                                    objectsToRemove.Add(child.gameObject);
                            }

                            foreach (GameObject child in objectsToRemove)
                            {
                                DestroyImmediate(child.gameObject);
                            }

                            CreateOpenCloseTriggers(false, index);
                        }
                    }
                }

                if (defaultDoor.RotationTimeline[index].RotationType ==
                    DefaultDoor.RotationTimelineData.TypeOfRotation.SingleRotation)
                {
                    if (defaultDoor.transform.parent.Find("Open Trigger " + "(Rotation " + (index + 1) + ")") ||
                        defaultDoor.transform.parent.Find("Close Trigger " + "(Rotation " + (index + 1) + ")"))
                    {
                        if (!Application.isPlaying)
                        {
                            List<GameObject> objectsToRemove = new List<GameObject>();

                            foreach (Transform child in defaultDoor.transform.parent)
                            {
                                if (child.name == "Open Trigger " + "(Rotation " + (index + 1) + ")")
                                    objectsToRemove.Add(child.gameObject);
                                else if (child.name == "Close Trigger " + "(Rotation " + (index + 1) + ")")
                                    objectsToRemove.Add(child.gameObject);
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
                if (defaultDoor != null &&
                    defaultDoor.transform.parent.Find("Move Trigger " + "(Rotation " + _numberOfTriggers + ")") != null)
                {
                    foreach (Transform child in defaultDoor.transform.parent.transform)
                    {
                        if (child.name == "Move Trigger " + "(Rotation " + _numberOfTriggers + ")")
                            DestroyImmediate(child.gameObject);
                    }

                    _numberOfTriggers--;
                }

                else if (
                    defaultDoor.transform.parent.Find("Open Trigger " + "(Rotation " + _numberOfTriggers + ")") != null ||
                    defaultDoor.transform.parent.Find("Close Trigger " + "(Rotation " + _numberOfTriggers + ")") != null)
                {
                    if (!Application.isPlaying)
                    {
                        List<GameObject> objectsToRemove = new List<GameObject>();

                        foreach (Transform child in defaultDoor.transform.parent)
                        {
                            if (child.name == "Open Trigger " + "(Rotation " + _numberOfTriggers + ")")
                                objectsToRemove.Add(child.gameObject);
                            else if (child.name == "Close Trigger " + "(Rotation " + _numberOfTriggers + ")")
                                objectsToRemove.Add(child.gameObject);
                        }

                        foreach (GameObject child in objectsToRemove)
                        {
                            DestroyImmediate(child.gameObject);
                        }

                        _numberOfTriggers--;
                    }
                }

                else if (
                    defaultDoor.transform.parent.Find("Move Trigger " + "(Rotation " + _numberOfTriggers + ")") == null ||
                    defaultDoor.transform.parent.Find("Open Trigger " + "(Rotation " + _numberOfTriggers + ")") == null ||
                    defaultDoor.transform.parent.Find("Close Trigger " + "(Rotation " + _numberOfTriggers + ")") ==
                    null) continue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateMoveTrigger(bool addToNumberOfTriggers, int index)
        {
            DefaultDoor defaultDoor = target as DefaultDoor;
            if (defaultDoor == null) throw new ArgumentNullException("DefaultDoor");

            GameObject MoveTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MoveTrigger.name = "Move Trigger " + "(Rotation " + (index + 1) + ")";

            ResetTransform(MoveTrigger, defaultDoor);
            SetParentChild(defaultDoor.transform.parent.gameObject, MoveTrigger);
            AddTriggerScript(MoveTrigger, index);
            SetColor(MoveTrigger);
            // MoveTrigger.tag = "TriggerZone";

            if (addToNumberOfTriggers) _numberOfTriggers += 1;
        }

        private void CreateOpenCloseTriggers(bool AddToNumberOfTriggers, int index)
        {
            DefaultDoor defaultDoor = target as DefaultDoor;

            GameObject OpenTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            OpenTrigger.name = "Open Trigger " + "(Rotation " + (index + 1) + ")";

            GameObject CloseTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            CloseTrigger.name = "Close Trigger " + "(Rotation " + (index + 1) + ")";

            ResetTransform(OpenTrigger, defaultDoor);
            SetParentChild(defaultDoor.transform.parent.gameObject, OpenTrigger);
            AddTriggerScript(OpenTrigger, index);

            ResetTransform(CloseTrigger, defaultDoor);
            SetParentChild(defaultDoor.transform.parent.gameObject, CloseTrigger);
            AddTriggerScript(CloseTrigger, index);
            SetColor(OpenTrigger);
            SetColor(CloseTrigger);

            //  OpenTrigger.tag = "TriggerZone";
            // CloseTrigger.tag = "TriggerZone";

            if (AddToNumberOfTriggers) _numberOfTriggers += 1;
        }

        private static void SetParentChild(GameObject Parent, GameObject Trigger)
        {
            // Parent.transform.parent = Selection.activeGameObject.transform.parent.transform;
            Trigger.transform.parent = Parent.transform;
        }

        private static void ResetTransform(GameObject obj, DefaultDoor DefaultDoor)
        {
            if (obj.name == "Move Trigger")
            {
                obj.transform.position = DefaultDoor.gameObject.transform.position + new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                //obj.transform.localScale = new Vector3(2, DefaultDoor.gameObject.transform.localScale.y + 0.25f, 2);
                obj.transform.rotation = DefaultDoor.gameObject.transform.rotation;
            }

            if (obj.name == "Open Trigger")
            {
                obj.transform.position = DefaultDoor.gameObject.transform.position - new Vector3(1, 0, 0) +
                                         new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                //obj.transform.localScale = new Vector3(2, DefaultDoor.gameObject.transform.localScale.y + 0.25f, 2);
                obj.transform.rotation = DefaultDoor.gameObject.transform.rotation;
            }

            if (obj.name == "Close Trigger")
            {
                obj.transform.position = DefaultDoor.gameObject.transform.position + new Vector3(1, 0, 0) +
                                         new Vector3(0, 0.125f, 0);
                obj.transform.localScale = Vector3.one;
                //obj.transform.localScale = new Vector3(2, DefaultDoor.gameObject.transform.localScale.y + 0.25f, 2);
                obj.transform.rotation = DefaultDoor.gameObject.transform.rotation;
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