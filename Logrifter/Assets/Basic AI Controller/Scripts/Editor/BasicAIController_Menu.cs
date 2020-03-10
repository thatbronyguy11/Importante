using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ViridaxGameStudios.AI
{
    public class BasicAIController_Menu : MonoBehaviour
    {
        [MenuItem("Window/Viridax Game Studios/AI/Basic AI Controller")]
        public static void CreateAIScript()
        {
            GameObject[] selectedGO = Selection.gameObjects;
            if (selectedGO.Length > 0)
            {
                foreach(GameObject obj in selectedGO)
                {
                    AttachAIControllerScript(obj);
                }
                
            }
            else
            {
                EditorUtility.DisplayDialog("AI Tools", "You need to select at least 1 GameObject", "OK");
            }

        }


        static void AttachAIControllerScript(GameObject obj)
        {
            //Assign AI Script to the GameObject
            BasicAIController AIscript = null;
            if (obj)
            {
                AIscript = obj.AddComponent<BasicAIController>();
                AIscript.enemyTags.Add("Player");
                Selection.activeGameObject = obj;
            }
        }
    }
}

