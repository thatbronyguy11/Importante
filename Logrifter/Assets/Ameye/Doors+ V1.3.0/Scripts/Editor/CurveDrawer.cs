// CurveDrawer.cs
// Created by Alexander Ameye
// Version 1.2.0

using UnityEngine;
using UnityEditor;

namespace DoorsPlus
{
    [CustomPropertyDrawer(typeof(CurveAttribute))]
    public class CurveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color curveColor = Color.green;
            CurveAttribute curve = attribute as CurveAttribute;

            int enumValue = GetConditionalHideAttributeResult(curve, property);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = ((curve.EnumValue1 == enumValue) || (curve.EnumValue2 == enumValue));
            if (!curve.HideInInspector || (curve.EnumValue1 == enumValue) || (curve.EnumValue2 == enumValue))
            {
                if (property.propertyType == SerializedPropertyType.AnimationCurve)
                {
                    EditorGUI.CurveField(position, property, curveColor, new Rect(curve.StartPosX, curve.StartPosY, curve.RangeX, curve.RangeY));
                }
            }
            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CurveAttribute curve = (CurveAttribute)attribute;
            int enumValue = GetConditionalHideAttributeResult(curve, property);

            if (!curve.HideInInspector || (curve.EnumValue1 == enumValue) || (curve.EnumValue2 == enumValue)) return EditorGUI.GetPropertyHeight(property, label) + 10;
            else return -EditorGUIUtility.standardVerticalSpacing;
        }

        private int GetConditionalHideAttributeResult(CurveAttribute curve, SerializedProperty property)
        {
            int enumValue = 0;

            SerializedProperty sourcePropertyValue = null;

            if (!property.isArray)
            {
                string propertyPath = property.propertyPath;
                string conditionPath = propertyPath.Replace(property.name, curve.ConditionalSourceField);
                sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

                if (sourcePropertyValue == null) sourcePropertyValue = property.serializedObject.FindProperty(curve.ConditionalSourceField);
            }

            else sourcePropertyValue = property.serializedObject.FindProperty(curve.ConditionalSourceField);

            if (sourcePropertyValue != null) enumValue = sourcePropertyValue.enumValueIndex;

            return enumValue;
        }
    }
}
