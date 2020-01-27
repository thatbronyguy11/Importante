// TagHelper.cs
// Created by Alexander Ameye
// Version 1.2.0

using UnityEditor;
using UnityEngine;

namespace Tagger
{
    public static class TagHelper
    {
        /*[MenuItem("Tools/Alex's Door System/Create Door Tag")]
        public static void AddDoorTag()
        {
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == "Door")
                    {
                        Debug.Log("Tag 'Door' already exists.");
                        return;
                    }
                }

                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Door";
                so.ApplyModifiedProperties();
                so.Update();

                Debug.Log("Tag 'Door' was created.");
            }
        }

        public static bool DoesDoorTagNotExist()
        {
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == "Door")
                        return false;
                }
            }

            return true;
        }*/

        public static bool DoesLayerNotExist()
        {
            var newLayer = LayerMask.NameToLayer("Trigger Zones");
            return newLayer <= -1;
        }

        [MenuItem("Tools/Doors+/Create Trigger Zone Layer", false, 2)]
        public static void CreateLayer()
        {
            if (string.IsNullOrEmpty("Trigger Zones")) throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var propCount = layerProps.arraySize;

            SerializedProperty firstEmptyProp = null;

            for (var i = 0; i < propCount; i++)
            {
                var layerProp = layerProps.GetArrayElementAtIndex(i);

                var stringValue = layerProp.stringValue;

                if (stringValue == "Trigger Zones") return;

                if (i < 8 || stringValue != string.Empty) continue;

                if (firstEmptyProp == null)
                    firstEmptyProp = layerProp;
            }

            if (firstEmptyProp == null)
            {
                UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + "Trigger Zones" + "\" not created.");
                return;
            }

            firstEmptyProp.stringValue = "Trigger Zones";
            tagManager.ApplyModifiedProperties();
        }
    }
}

