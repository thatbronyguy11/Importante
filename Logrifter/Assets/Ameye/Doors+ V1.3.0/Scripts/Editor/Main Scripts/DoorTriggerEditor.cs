using System.Linq;
using UnityEngine;
using UnityEditor;


namespace DoorsPlus
{
    [CustomEditor(typeof(DoorTrigger)), CanEditMultipleObjects]
    public class DoorTriggerEditor : Editor//, IHierarchyIcon
    {
        private int _toolBarIndex;

        private DoorDetection _doordetection;
        private DoorTrigger _doortrigger;

        private SerializedProperty _colliderTypeProp;

        private SerializedProperty _hasTagProp, _hasNameProp, _isLookingAtProp, _hasPressedProp, _hasScriptProp, _isGameObjectProp;
        private SerializedProperty _tagProp, _nameProp, _charProp;
        private SerializedProperty _lookObjectProp, _objectProp;
        private SerializedProperty _scriptProp;
        
        internal static GUIContent VersionLabel;
        internal static GUIStyle centeredVersionLabel;
        
        bool StylesNotLoaded = true;

        private void OnEnable()
        {
            _colliderTypeProp = serializedObject.FindProperty("colliderType");

            _hasTagProp = serializedObject.FindProperty("hasTag");
            _hasNameProp = serializedObject.FindProperty("hasName");
            _isLookingAtProp = serializedObject.FindProperty("isLookingAt");
            _hasPressedProp = serializedObject.FindProperty("hasPressed");
            _hasScriptProp = serializedObject.FindProperty("hasScript");
            _isGameObjectProp = serializedObject.FindProperty("isGameObject");

            _tagProp = serializedObject.FindProperty("playerTag");
            _nameProp = serializedObject.FindProperty("playerName");
            _charProp = serializedObject.FindProperty("character");
            _lookObjectProp = serializedObject.FindProperty("lookObject");
            _scriptProp = serializedObject.FindProperty("script");
            _objectProp = serializedObject.FindProperty("isObject");

            //lockProp = serializedObject.FindProperty("RequiresPadLockUnlock");
            //keyPadProp = serializedObject.FindProperty("RequiresKeyPadUnlock");
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
        {   /*CaptureGroup.ShowDebug = true;
        CaptureGroup.Begin("Assets/test.png");
        {*/
            
            if (StylesNotLoaded) LoadStyles();
            
            serializedObject.Update();
            _doortrigger = target as DoorTrigger;
            _doordetection = GameObject.FindGameObjectWithTag("Player").GetComponent<DoorDetection>();

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };

            string[] menuOptions = new string[2];
            menuOptions[0] = "Trigger";
            menuOptions[1] = "Gizmo";

            EditorGUILayout.Space();
            EditorGUIUtility.labelWidth = 70;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<b>Trigger Zone</b>", style);
            EditorGUILayout.PropertyField(_colliderTypeProp, new GUIContent("Shape"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<b>Player Requirements</b>", style);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasTagProp,
                new GUIContent("Tag", "Does the object that entered the trigger zone have a certain tag?"));
            if (_doortrigger != null && _doortrigger.hasTag)
                EditorGUILayout.PropertyField(_tagProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasNameProp,
                new GUIContent("Name", "Does the object that entered the trigger zone has a certain name?"));
            if (_doortrigger != null && _doortrigger.hasName)
                EditorGUILayout.PropertyField(_nameProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_isLookingAtProp,
                new GUIContent("Looking At", "Is the player looking at a certain object?"));
            if (_doortrigger != null && _doortrigger.isLookingAt)
                EditorGUILayout.PropertyField(_lookObjectProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasPressedProp,
                new GUIContent("Pressed", "Has the user pressed a certain key?"));
            if (_doortrigger != null && _doortrigger.hasPressed)
                EditorGUILayout.PropertyField(_charProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_hasScriptProp,
                new GUIContent("Script", "Does the player have a certain script component?"));
            if (_doortrigger != null && _doortrigger.hasScript)
                EditorGUILayout.PropertyField(_scriptProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_isGameObjectProp,
                new GUIContent("Is Object", "Is there a certain object in the trigger zone?"));
            if (_doortrigger != null && _doortrigger.isGameObject)
                EditorGUILayout.PropertyField(_objectProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            if (AnyEmptyFields())
                EditorGUILayout.HelpBox("One or more fields have been left empty.", MessageType.Warning);

            if (_doortrigger.isLookingAt && _doordetection.Reach == 0 && _doortrigger.isLookingAt &&
                _doortrigger.lookObject != null)
                EditorGUILayout.HelpBox("The reach of your player is zero.", MessageType.Warning);

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add " + _doortrigger.transform.gameObject.name))
            {
                GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trigger.name = _doortrigger.transform.gameObject.name;

                GameObject rotationParent = _doortrigger.transform.parent.gameObject;

                trigger.transform.position = _doortrigger.transform.position;
                trigger.transform.localScale = _doortrigger.transform.localScale;
                trigger.transform.rotation = _doortrigger.transform.rotation;

                SetParentChild(rotationParent, trigger);
                
                trigger.AddComponent<DoorTrigger>();
                trigger.GetComponent<DoorTrigger>().id = _doortrigger.id;
                trigger.GetComponent<DoorTrigger>().colliderType = _doortrigger.colliderType;

                string material = "";

                switch (trigger.name.Split().First()) // Maybe not use LINQ?
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
                    case "Front":
                        material = "Assets/Ameye/Doors+ V1.3.0/Resources/Front_Trigger_Zone.mat";
                        break;
                    case "Back":
                        material = "Assets/Ameye/Doors+ V1.3.0/Resources/Back_Trigger_Zone.mat";
                        break;
                }

                Material mat = (Material)AssetDatabase.LoadAssetAtPath(material, typeof(Material));
                trigger.GetComponent<Renderer>().material = mat;
            }

            GUI.color = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(VersionLabel, centeredVersionLabel);

            if (Event.current.type == EventType.Repaint)
            {
                if (_doortrigger.colliderType == DoorTrigger.TypeOfCollider.Cubic)
                {
                    if (_doortrigger.gameObject.GetComponent<BoxCollider>() == null)
                        _doortrigger.gameObject.AddComponent<BoxCollider>();
                    _doortrigger.gameObject.GetComponent<BoxCollider>().isTrigger = true;
                }

                else if (_doortrigger.colliderType == DoorTrigger.TypeOfCollider.Spherical)
                {
                    if (_doortrigger.gameObject.GetComponent<SphereCollider>() == null)
                        _doortrigger.gameObject.AddComponent<SphereCollider>();
                    _doortrigger.gameObject.GetComponent<SphereCollider>().isTrigger = true;
                }
            }

            if (_doortrigger.colliderType == DoorTrigger.TypeOfCollider.Cubic)
            {
                _doortrigger.GetComponent<BoxCollider>().enabled = true;
                _doortrigger.GetComponent<SphereCollider>().enabled = false;

                _doortrigger.GetComponent<MeshFilter>().mesh =
                    CreatePrimitiveMesh(PrimitiveType.Cube); //PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube);
            }

            else if (_doortrigger.colliderType == DoorTrigger.TypeOfCollider.Spherical)
            {
                _doortrigger.GetComponent<BoxCollider>().enabled = false;
                _doortrigger.GetComponent<SphereCollider>().enabled = true;
                _doortrigger.GetComponent<MeshFilter>().mesh =
                    CreatePrimitiveMesh(PrimitiveType.Sphere); //PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Sphere);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void SetParentChild(GameObject parent, GameObject trigger)
        {
            parent.transform.parent = Selection.activeGameObject.transform.parent.transform;
            trigger.transform.parent = parent.transform;
        }

        private bool AnyEmptyFields()
        {
            DoorTrigger doortrigger = target as DoorTrigger;

            if (doortrigger != null && (doortrigger.hasName && doortrigger.playerName == ""))
                return true;
            if (doortrigger != null && (doortrigger.isLookingAt && doortrigger.lookObject == null))
                return true;
            if (doortrigger != null && (doortrigger.hasPressed && doortrigger.character == ""))
                return true;
            if (doortrigger != null && (doortrigger.hasScript && doortrigger.script.script.Name == ""))
                return true;
            return doortrigger != null && (doortrigger.isGameObject && doortrigger.isObject == null);
        }

        private static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(gameObject);
            return mesh;
        }
        
        static GUIContent IconContent(string text, string icon, string tooltip)
        {
            Texture2D cached = (Texture2D)Resources.Load("Icons/" + icon);  
            return new GUIContent(text, cached, tooltip);
        }
    }
}