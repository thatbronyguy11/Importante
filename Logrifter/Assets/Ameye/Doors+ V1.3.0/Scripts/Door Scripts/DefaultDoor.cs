using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace DoorsPlus
{
    public class DefaultDoor : Door, IHierarchyIcon
    {
        public string EditorIconPath { get { return "DefaultDoor"; } }
        public List<RotationTimelineData> RotationTimeline;

        [Serializable]
        public class RotationTimelineData
        {
            [HideInInspector]
            public string RotationIndex;

            public enum TypeOfRotation { SingleRotation, LoopedRotation }
            public TypeOfRotation RotationType = TypeOfRotation.SingleRotation; // default rotation type

            public float InitialAngle;
            public float FinalAngle = 90f;

            [StayPositive]
            public float Speed;

            public enum RotationCurvePreset { Custom, Linear, EaseIn, EaseOut, Smoothstep, Smootherstep, Exponential, Hermite }
            public RotationCurvePreset RotationCurve;

            [Curve("RotationCurve", 0, 0, 0, 1f, 1f)]
            public AnimationCurve CustomCurve;

            [ConditionalEnumHide("RotationType", 1)]
            public int TimesMoveable;
        }

        private GameObject hinge;

        public enum PositionOfHinge { Left, Right }
        public PositionOfHinge HingePosition;

        // 3rd party compatibility
        public enum ScaleOfDoor { Unity3DUnits, Other }
        public ScaleOfDoor DoorScale;
        public enum PositionOfPivot { Centered, CorrectlyPositioned }
        public PositionOfPivot PivotPosition;

        // Rotations
        public Quaternion StartRotation, EndRotation;
        public int RotationState; // 0 = closed, 1 = opened
        public Quaternion RotationOffset;
        public float InitialYRotation;

        // Rotation Progress
        public int CurrentRotationBlockIndex;
        
        // Settings
        public bool ShortestWay = true;
        public enum WayOfRotation { Default, Shortest }
        public WayOfRotation RotationWay;

        public override void Start()
        {
            base.Start();

            var localEulerAngles = transform.localEulerAngles;
            
            InitialYRotation = localEulerAngles.y;
            RotationOffset = Quaternion.Euler(0, localEulerAngles.y, 0);

            if (PivotPosition == PositionOfPivot.CorrectlyPositioned)
            {
                if (RotationTimeline[0].RotationType == RotationTimelineData.TypeOfRotation.LoopedRotation || RotationTimeline[0].RotationType == RotationTimelineData.TypeOfRotation.SingleRotation)
                    transform.rotation = Quaternion.Euler(0, RotationTimeline[0].InitialAngle, 0) * RotationOffset;
            }

            ShortestWay = RotationWay == WayOfRotation.Shortest;

            #region Hinge Algorithm
            if (DoorScale != ScaleOfDoor.Unity3DUnits || PivotPosition != PositionOfPivot.Centered) return;

            hinge = new GameObject("Hinge");
            hinge.transform.parent = transform.parent;
            hinge.transform.position = CalculateHingePosition();
            transform.SetParent(hinge.transform);
            hinge.transform.localEulerAngles = CalculateHingeRotation();
            #endregion
        }

        public Vector3 CalculateHingePosition()
        {
            var t = transform;
            var eulerAngles = t.eulerAngles;
            var position = t.position;
            var localScale = t.localScale;
            
            var cosDeg = Mathf.Cos((eulerAngles.y * Mathf.PI) / 180);
            var sinDeg = Mathf.Sin((eulerAngles.y * Mathf.PI) / 180);
            
            var posDoorX = position.x;
            var posDoorY = position.y;
            var posDoorZ = position.z;
            
            var scaleDoorX = localScale.x;
            var scaleDoorZ = localScale.z;
            var hingePosCopy = hinge.transform.position;

            if (HingePosition == PositionOfHinge.Left && RotationTimeline.Count != 0)
            {
                var scale = transform.localScale;
                hingePosCopy.x = scale.x > scale.z ? posDoorX - (scaleDoorX / 2 * cosDeg) : posDoorX + (scaleDoorZ / 2 * sinDeg);
                hingePosCopy.z = scale.x > scale.z ? posDoorZ + (scaleDoorX / 2 * sinDeg) : posDoorZ + (scaleDoorZ / 2 * cosDeg);
            }

            if (HingePosition == PositionOfHinge.Right && RotationTimeline.Count != 0)
            {
                var scale = transform.localScale;
                hingePosCopy.x = scale.x > scale.z ? posDoorX + (scaleDoorX / 2 * cosDeg) : posDoorX - (scaleDoorZ / 2 * sinDeg);
                hingePosCopy.z = scale.x > scale.z ? posDoorZ - (scaleDoorX / 2 * sinDeg) : posDoorZ - (scaleDoorZ / 2 * cosDeg);
            }

            hingePosCopy.y = posDoorY;

            return hingePosCopy;
        }

        public Vector3 CalculateHingeRotation()
        {
            var localEulerAngles = transform.localEulerAngles;
            var rotDoorX = localEulerAngles.x;
            var rotDoorZ = localEulerAngles.z;

            var hingeRotCopy = hinge.transform.localEulerAngles;

            hingeRotCopy.x = rotDoorX;
            hingeRotCopy.y = RotationTimeline[0].InitialAngle;
            hingeRotCopy.z = rotDoorZ;

            return hingeRotCopy;
        }

        public IEnumerator Rotate()
        {
            MovementPending = true;

            Transform t = transform;
            RotationTimelineData CurrentRotationBlock = RotationTimeline[CurrentRotationBlockIndex];
            float TimeProgression = 0f;

            if (CurrentRotationBlock.RotationType == RotationTimelineData.TypeOfRotation.SingleRotation)
                CurrentRotationBlock.TimesMoveable = 1;

            StartRotation = Quaternion.Euler(0, CurrentRotationBlock.InitialAngle, 0);
            EndRotation = Quaternion.Euler(0, CurrentRotationBlock.FinalAngle, 0);

            if (DoorScale == ScaleOfDoor.Unity3DUnits && PivotPosition == PositionOfPivot.Centered)
            {
                t = hinge.transform;
                RotationOffset = Quaternion.identity;
            }

            if (TimesMoved == 0) t.rotation = StartRotation * RotationOffset;

            if (TimesMoved < CurrentRotationBlock.TimesMoveable || CurrentRotationBlock.TimesMoveable == 0)
            {
                if (t.rotation == (RotationState == 0 ? EndRotation * RotationOffset : StartRotation * RotationOffset)) RotationState ^= 1;

                while (TimeProgression <= (1 / CurrentRotationBlock.Speed))
                {
                    TimeProgression += Time.deltaTime;
                    float RotationProgression = Mathf.Clamp01(TimeProgression / (1 / CurrentRotationBlock.Speed));
                    float RotationCurveValue;

                    switch (CurrentRotationBlock.RotationCurve)
                    {
                        case RotationTimelineData.RotationCurvePreset.Linear:
                            RotationCurveValue = RotationProgression;
                            break;
                        case RotationTimelineData.RotationCurvePreset.EaseIn:
                            RotationCurveValue = 1f - Mathf.Cos(RotationProgression * Mathf.PI * 0.5f);
                            break;
                        case RotationTimelineData.RotationCurvePreset.EaseOut:
                            RotationCurveValue = Mathf.Sin(RotationProgression * Mathf.PI * 0.5f);
                            break;
                        case RotationTimelineData.RotationCurvePreset.Smoothstep:
                            RotationCurveValue = RotationProgression * RotationProgression * (3f - 2f * RotationProgression);
                            break;
                        case RotationTimelineData.RotationCurvePreset.Smootherstep:
                            RotationCurveValue = RotationProgression * RotationProgression * RotationProgression * (RotationProgression * (6f * RotationProgression - 15f) + 10f);
                            break;
                        case RotationTimelineData.RotationCurvePreset.Exponential:
                            RotationCurveValue = RotationProgression * RotationProgression;
                            break;
                        case RotationTimelineData.RotationCurvePreset.Hermite:
                            RotationCurveValue = RotationProgression * RotationProgression * (3.0f - 2.0f * RotationProgression);
                            break;
                        default:
                            RotationCurveValue = CurrentRotationBlock.CustomCurve.Evaluate(RotationProgression);
                            break;
                    }

                    if (RotationState == 0) // Door is closed
                        t.rotation = RotationTools.Lerp(StartRotation * RotationOffset, EndRotation * RotationOffset, RotationCurveValue, ShortestWay);

                    if (RotationState == 1) // Door is opened
                        t.rotation = RotationTools.Lerp(EndRotation * RotationOffset, StartRotation * RotationOffset, RotationCurveValue, ShortestWay);

                    yield return null;
                }

                TimesMoved++;

                if (TimesMoved == CurrentRotationBlock.TimesMoveable && CurrentRotationBlock.TimesMoveable != 0)
                {
                    TimesMoved = 0;
                    CurrentRotationBlockIndex++;
                }
            }
            MovementPending = false;
        }
    }
}