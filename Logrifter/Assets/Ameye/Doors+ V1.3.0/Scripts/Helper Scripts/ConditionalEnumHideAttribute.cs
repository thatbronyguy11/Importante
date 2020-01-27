// ConditionalEnumHideAttribute.cs
// Original version created by Brecht Lecluyse (www.brechtos.com)
// Modified by Alexander Ameye

using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalEnumHideAttribute : PropertyAttribute
{
    public string ConditionalSourceField = "";
    public int EnumValue1 = 0, EnumValue2 = 0;
    public bool HideInInspector = true;
    public bool Inverse = false;

    public ConditionalEnumHideAttribute(string conditionalSourceField, int enumValue1)
    {
        ConditionalSourceField = conditionalSourceField;
        EnumValue1 = enumValue1;
        EnumValue2 = enumValue1;
    }

    public ConditionalEnumHideAttribute(string conditionalSourceField, int enumValue1, int enumValue2)
    {
        ConditionalSourceField = conditionalSourceField;
        EnumValue1 = enumValue1;
        EnumValue2 = enumValue2;
    }
}