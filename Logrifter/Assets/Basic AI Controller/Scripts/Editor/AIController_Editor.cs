using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ViridaxGameStudios.AI
{
    [CustomEditor(typeof(BasicAIController))]
    public class AIController_Editor : Editor
    {
        #region variables
        private BasicAIController character;
        string[] arrTabs = { "AI Settings", "Movement", "Combat" };
        private int tabIndex;
        private int enemyTagCount;
        private int patrolPointCount;
        private int allyTagCount;
        GUIStyle guiStyle = new GUIStyle();
        bool showPatrolPoints = false;
        bool showAllyTags = false;
        bool showEnemyTags = false;
        
        #endregion

        #region Main Methods
        void OnEnable()
        {
            //Store a reference to the AI Controller script
            character = (BasicAIController)target;
            guiStyle.fontSize = 14;
            guiStyle.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            Texture2D image = (Texture2D)Resources.Load("LogoScaled");
            GUIContent label = new GUIContent(image);

            
            GUILayout.Label(label);
            EditorGUI.BeginChangeCheck();
            tabIndex = GUILayout.Toolbar(tabIndex, arrTabs);
            switch (tabIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
            }
            EditorGUI.BeginChangeCheck();
            switch (tabIndex)
            {
                case 0:
                    DrawAISettingGUI();
                    break;
                case 1:
                    DrawMovementGUI();
                    break;
                case 2:
                    DrawCombatGUI();
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {

            }
            

        }
        void DrawAISettingGUI()
        {
            EditorGUILayout.TextField("Hit Points:", character.HitPoints.ToString());
            GUIContent label = new GUIContent("Base Stats", "The base stats are used once the game runs to create the character object and do initial calculations, such as damages. They cannot be changed after runtime, except by calling LevelUp(). Only stats multiplier can be changed during runtime.");
            GUILayout.Label(label, guiStyle);
            label = new GUIContent("Stats Multiplier", "This is used to calculate the attack damage based on your Strength, Intelligence and Faith. This variable can be changed during runtime.");
            character.m_StatsMultiplier = EditorGUILayout.Slider(label, character.m_StatsMultiplier, 1f, 10);
            label = new GUIContent("Strength", "This directly affects the HitPoints and physical damage.");
            character.m_Strength = (int)EditorGUILayout.IntField(label, character.m_Strength);
            label = new GUIContent("Intelligence", "This directly affects the HitPoints and mental damage.");
            character.m_Intelligence = (int) EditorGUILayout.IntField(label, character.m_Intelligence);
            label = new GUIContent("Faith", "This directly affects the HitPoints and spiritual damage.");
            character.m_Faith = (int)EditorGUILayout.IntField(label, character.m_Faith);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            label = new GUIContent("Damage Stats", "The characters damages, based on the base stats and stats multiplier. NOTE: These values cannot be changed and are completely dependent on the base stats and stat multiplier.");
            GUILayout.Label(label, guiStyle);
            EditorGUILayout.FloatField("Attack Damage", character.attackDamage);
            EditorGUILayout.FloatField("Physical Damage", character.physicalDamage);
            EditorGUILayout.FloatField("Mental Damage", character.mentalDamage);
            EditorGUILayout.FloatField("Spiritual Damage", character.spiritualDamage);
            EditorGUILayout.Space();
            
            //To be Implemented in a future update.
            /*
            label = new GUIContent("Ally Tags:", "All the Tags that the character will consider as an ally. NOTE: The default reaction is to follow.");
            EditorGUILayout.LabelField(label, guiStyle);
            showAllyTags = EditorGUILayout.Foldout(showAllyTags, label);
            if(showAllyTags)
            {
                allyTagCount = character.allyTags.Count;
                allyTagCount = EditorGUILayout.IntField("Size: ", allyTagCount);

                if (allyTagCount != character.allyTags.Count)
                {
                    while (allyTagCount > character.allyTags.Count)
                    {
                        character.allyTags.Add("");
                    }
                    while (allyTagCount < character.allyTags.Count)
                    {
                        character.allyTags.RemoveAt(character.allyTags.Count - 1);
                    }
                }

                for (int i = 0; i < character.allyTags.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Element " + i);
                    string tag = "";
                    tag = EditorGUILayout.TagField(character.allyTags[i]);
                    if (character.enemyTags.Contains(tag))
                    {
                        EditorUtility.DisplayDialog("Basic AI Controller", "Tag '" + tag + "' already added to enemy tags", "OK");
                    }
                    else
                    {
                        character.allyTags[i] = tag;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }
            */
        }
        void DrawMovementGUI()
        {
            GUIContent label = new GUIContent("Detection Radius", "The radius which the character can detect other objects.");
            //Detection and Head Look Settings
            GUILayout.Label("Detection Settings", guiStyle);
            character.m_DetectionRadius = EditorGUILayout.FloatField(label, character.m_DetectionRadius);
            label = new GUIContent("Enable Head Look:", "Allow the character to dynbamically look at objects.");
            character.enableHeadLook = EditorGUILayout.Toggle(label, character.enableHeadLook);
            label = new GUIContent("Head Look Target: ");
            character.headLookTarget = (GameObject)EditorGUILayout.ObjectField(label, character.headLookTarget, typeof(GameObject), true);
            label = new GUIContent("Head Look Intensity:", "How quickly the character will turn their head to look at objects.");
            character.headLookIntensity = EditorGUILayout.Slider(label, character.headLookIntensity, 0f, 1f);

            //Patrol Settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //GUIContent label = new GUIContent("Patrol Settings");

            //EditorGUILayout.LabelField();
            EditorGUILayout.LabelField("Patrol Settings", guiStyle);
            
            character.isPatrolling = EditorGUILayout.Toggle("Is Patrolling", character.isPatrolling);
            label = new GUIContent("Patrol In Order:", "Whether or not the character should patrol each point in order of the list. False will allow the character to patrol randomly.");
            character.patrolInOrder = EditorGUILayout.Toggle(label, character.patrolInOrder);
            label = new GUIContent("Patrol Points:", "The points in the gameworld where you want the character to patrol. They can be anything, even empty gameObjects. Note: Ensure each patrol point is tagged as 'PatrolPoint'");
            patrolPointCount = character.patrolPoints.Count;
            showPatrolPoints = EditorGUILayout.Foldout(showPatrolPoints, label);
            if(showPatrolPoints)
            {
                label = new GUIContent("Size:");
                patrolPointCount = EditorGUILayout.IntField(label, patrolPointCount);

                if (patrolPointCount != character.patrolPoints.Count)
                {
                    while (patrolPointCount > character.patrolPoints.Count)
                    {
                        character.patrolPoints.Add(null);
                    }
                    while (patrolPointCount < character.patrolPoints.Count)
                    {
                        character.patrolPoints.RemoveAt(character.patrolPoints.Count - 1);
                    }
                }
                //EditorGUILayout.Space();
                for (int i = 0; i < character.patrolPoints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Element " + i);
                    character.patrolPoints[i] = (GameObject)EditorGUILayout.ObjectField(character.patrolPoints[i], typeof(GameObject), true);
                    EditorGUILayout.EndHorizontal();
                    //EditorGUILayout.Space();
                }
            }
        }
        void DrawCombatGUI()
        {
            GUILayout.Label("Attack Settings", guiStyle);
            character.m_AttackRange = EditorGUILayout.FloatField("Attack Range:", character.m_AttackRange);
            character.m_DamageAngle = EditorGUILayout.Slider("Damage Angle:", character.m_DamageAngle, 0, 360f);
            character.canAttack = EditorGUILayout.Toggle("Can Attack:", character.canAttack);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIContent label = new GUIContent("Enemy Tags", "All the Tags that the character will consider as an enemy. NOTE: The default reaction is to attack.");
            EditorGUILayout.LabelField(label, guiStyle);
            showEnemyTags = EditorGUILayout.Foldout(showEnemyTags, label);
            if(showEnemyTags)
            {
                enemyTagCount = character.enemyTags.Count;
                enemyTagCount = EditorGUILayout.IntField("Size", enemyTagCount);

                if (enemyTagCount != character.enemyTags.Count)
                {
                    while (enemyTagCount > character.enemyTags.Count)
                    {
                        character.enemyTags.Add("");
                    }
                    while (enemyTagCount < character.enemyTags.Count)
                    {
                        character.enemyTags.RemoveAt(character.enemyTags.Count - 1);
                    }
                }

                for (int i = 0; i < character.enemyTags.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Element " + i);
                    string tag = "";
                    tag = EditorGUILayout.TagField(character.enemyTags[i]);
                    if (character.allyTags.Contains(tag))
                    {
                        EditorUtility.DisplayDialog("Basic AI Controller", "Tag '" + tag + "' already added to ally tags", "OK");
                    }
                    else
                    {
                        character.enemyTags[i] = tag;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }
            


            }
        void OnSceneGUI()
        {
            if(character != null)
            {
                //Call the necessary methods to draw the discs and handles on the editor
                Color color = new Color(1f, 0f, 0f, 0.15f);
                DrawDiscs(color, character.transform.position, Vector3.up, -character.transform.forward, ref character.m_DetectionRadius, "Detection Radius", float.MaxValue);
                color = new Color(0f, 0f, 1f, 0.15f);
                DrawDiscs(color, character.transform.position, Vector3.up, character.transform.right, ref character.m_AttackRange, "Attack Range", character.m_DetectionRadius);
                color = new Color(1f, 0f, 0f, 0.35f);
                DrawArcs(color, character.transform.position, Vector3.up, character.transform.forward, ref character.m_DamageAngle, ref character.m_AttackRange, "Damage Angle");
            }
            
        }

        protected void DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius, float maxValue)
        {
            //
            //Method Name : void DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius)
            //Purpose     : This method draws the necessary discs and slider handles in the editor to adjust the attack range and detection radius.
            //Re-use      : none
            //Input       : Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius
            //Output      : none
            //
            //Draw the disc that will represent the detection radius
            Handles.color = color;
            Handles.DrawSolidDisc(center, normal, radius);
            Handles.color = new Color(1f, 1f, 0f, 0.75f);
            Handles.DrawWireDisc(center, normal, radius);

            //Create Slider handles to adjust detection radius properties
            color.a = 0.5f;
            Handles.color = color;
            radius = Handles.ScaleSlider(radius, character.transform.position, direction, Quaternion.identity, radius, 1f);
            radius = Mathf.Clamp(radius, 1f, maxValue);

            

        }
        
        protected void DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius, string label, float maxValue)
        {
            //
            //Method Name : void DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius, string label)
            //Purpose     : Overloaded method of DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius)
            //              that adds the necessary labels. 
            //Re-use      : DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius)
            //Input       : Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius, string label
            //Output      : none
            //

            DrawDiscs(color, center, normal, direction, ref radius, maxValue);
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 12;
            color.a = 0.8f;
            labelStyle.normal.textColor = color;
            labelStyle.alignment = TextAnchor.UpperCenter;
            Handles.Label(character.transform.position + (direction * radius), label, labelStyle);
        }

        protected void DrawArcs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float angle, ref float radius, string label)
        {
            //
            //Method Name : void DrawDiscs(Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius)
            //Purpose     : This method draws the necessary discs and slider handles in the editor to adjust the attack range and detection radius.
            //Re-use      : none
            //Input       : Color color, Vector3 center, Vector3 normal, Vector3 direction, ref float radius
            //Output      : none
            //
            //Draw the disc that will represent the detection radius
            
            Handles.color = color;
            Vector3 newDirection = character.transform.forward - (character.transform.right);
            Handles.DrawSolidArc(center, normal, direction, angle/2, radius);
            Handles.DrawSolidArc(center, normal, direction, -angle/2, radius);
            Handles.color = new Color(1f, 1f, 0f, 0.75f);
            Handles.DrawWireArc(center, normal, newDirection, angle, radius);

            //Create Slider handles to adjust detection radius properties
            color.a = 0.5f;
            Handles.color = color;
            angle = Handles.ScaleSlider(angle, character.transform.position, direction, Quaternion.identity, radius, 1f);
            angle = Mathf.Clamp(angle, 1f, 360);

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 12;
            color.a = 0.8f;
            labelStyle.normal.textColor = color;
            labelStyle.alignment = TextAnchor.UpperCenter;
            Handles.Label(character.transform.position + (direction * radius), label, labelStyle);
        }
        #endregion
    }//end class
}//end namespace

