// ConditionalEnumHidePropertyDrawer.cs
// Original version created by Brecht Lecluyse (www.brechtos.com)
// Modified by Alexander Ameye

using UnityEngine;
using UnityEditor;

namespace DoorsPlus
{
    [CustomPropertyDrawer(typeof(ConditionalEnumHideAttribute))]
    public class ConditionalEnumHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalEnumHideAttribute condHAtt = (ConditionalEnumHideAttribute)attribute;
            int enumValue = GetConditionalHideAttributeResult(condHAtt, property);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = ((condHAtt.EnumValue1 == enumValue) || (condHAtt.EnumValue2 == enumValue));
            if (!condHAtt.HideInInspector || (condHAtt.EnumValue1 == enumValue) || (condHAtt.EnumValue2 == enumValue))
                EditorGUI.PropertyField(position, property, label, true);

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalEnumHideAttribute condHAtt = (ConditionalEnumHideAttribute)attribute;
            int enumValue = GetConditionalHideAttributeResult(condHAtt, property);

            if (!condHAtt.HideInInspector || (condHAtt.EnumValue1 == enumValue) || (condHAtt.EnumValue2 == enumValue)) return EditorGUI.GetPropertyHeight(property, label);
            else return -EditorGUIUtility.standardVerticalSpacing;
        }

        private int GetConditionalHideAttributeResult(ConditionalEnumHideAttribute condHAtt, SerializedProperty property)
        {
            int enumValue = 0;

            SerializedProperty sourcePropertyValue = null;

            if (!property.isArray)
            {
                string propertyPath = property.propertyPath;
                string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField);
                sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

                if (sourcePropertyValue == null) sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);
            }

            else sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);

            if (sourcePropertyValue != null) enumValue = sourcePropertyValue.enumValueIndex;

            return enumValue;
        }
    }
}
