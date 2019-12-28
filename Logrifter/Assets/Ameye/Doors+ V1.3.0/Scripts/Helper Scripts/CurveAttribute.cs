// CurveAttribute.cs
// Created by Alexander Ameye
// Version 1.2.0

using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class CurveAttribute : PropertyAttribute
{
    public readonly float StartPosX;
    public readonly float StartPosY;
    public readonly float RangeX;
    public readonly float RangeY;

    public readonly string ConditionalSourceField = "";
    public readonly int EnumValue1 = 0;
    public readonly int EnumValue2 = 0;
    public readonly bool HideInInspector = true;

    public CurveAttribute(string conditionalSourceField, int enumValue1, float PosX, float PosY,float RangeX, float RangeY)
    {
        StartPosX = PosX;
        StartPosY = PosY;
        this.RangeX = RangeX;
        this.RangeY = RangeY;

        ConditionalSourceField = conditionalSourceField;
        EnumValue1 = enumValue1;
        EnumValue2 = enumValue1;
    }
}


