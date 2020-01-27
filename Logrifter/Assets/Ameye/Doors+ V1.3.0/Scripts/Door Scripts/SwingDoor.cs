using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoorsPlus
{
    public class SwingDoor : Door, IHierarchyIcon
    {
        public enum PositionOfHinge
        {
            Left,
            Right
        }

        public enum PositionOfPivot
        {
            Centered,
            CorrectlyPositioned
        }

        // 3rd party compatibility
        public enum ScaleOfDoor
        {
            Unity3DUnits,
            Other
        }

        public enum WayOfRotation
        {
            Default,
            Shortest
        }

        // Rotation Progress
        public int CurrentRotationBlockIndex;
        public ScaleOfDoor DoorScale;

        private GameObject hinge;
        public PositionOfHinge HingePosition;
        public float InitialYRotation;
        public PositionOfPivot PivotPosition;
        public Quaternion RotationOffset;
        public int RotationState; // 0 = closed, 1 = opened

        public List<RotationTimelineData> RotationTimeline;
        public WayOfRotation RotationWay;

        // Settings
        public bool ShortestWay = true;

        // Rotations
        public Quaternion StartRotation, EndRotation;
        public Quaternion SwingStart, SwingMid, SwingEnd;
        public int SwingState; // 1 = Front, 2 = Middle, 3 = Back
        public string EditorIconPath => "SwingDoor";

        public override void Start()
        {
            if (RotationTimeline[0].InitialState == RotationTimelineData.InitialSwingState.Front) SwingState = 1;
            if (RotationTimeline[0].InitialState == RotationTimelineData.InitialSwingState.Middle) SwingState = 2;
            if (RotationTimeline[0].InitialState == RotationTimelineData.InitialSwingState.Back) SwingState = 3;

            InitialYRotation = transform.localEulerAngles.y;
            RotationOffset = Quaternion.Euler(0, transform.localEulerAngles.y, 0);

            if (PivotPosition == PositionOfPivot.CorrectlyPositioned)
            {
                if (SwingState == 1)
                    transform.rotation = Quaternion.Euler(0, RotationTimeline[0].FrontAngle, 0) * RotationOffset;
                if (SwingState == 2)
                    transform.rotation = Quaternion.Euler(0, RotationTimeline[0].MiddleAngle, 0) * RotationOffset;
                if (SwingState == 3)
                    transform.rotation = Quaternion.Euler(0, RotationTimeline[0].BackAngle, 0) * RotationOffset;
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
            var cosDeg = Mathf.Cos(transform.eulerAngles.y * Mathf.PI / 180);
            var sinDeg = Mathf.Sin(transform.eulerAngles.y * Mathf.PI / 180);

            var posDoorX = transform.position.x;
            var posDoorY = transform.position.y;
            var posDoorZ = transform.position.z;
            var scaleDoorX = transform.localScale.x;
            var scaleDoorZ = transform.localScale.z;
            var hingePosCopy = hinge.transform.position;

            if (HingePosition == PositionOfHinge.Left && RotationTimeline.Count != 0)
            {
                hingePosCopy.x = transform.localScale.x > transform.localScale.z
                    ? posDoorX - scaleDoorX / 2 * cosDeg
                    : posDoorX + scaleDoorZ / 2 * sinDeg;
                hingePosCopy.z = transform.localScale.x > transform.localScale.z
                    ? posDoorZ + scaleDoorX / 2 * sinDeg
                    : posDoorZ + scaleDoorZ / 2 * cosDeg;
            }

            if (HingePosition == PositionOfHinge.Right && RotationTimeline.Count != 0)
            {
                hingePosCopy.x = transform.localScale.x > transform.localScale.z
                    ? posDoorX + scaleDoorX / 2 * cosDeg
                    : posDoorX - scaleDoorZ / 2 * sinDeg;
                hingePosCopy.z = transform.localScale.x > transform.localScale.z
                    ? posDoorZ - scaleDoorX / 2 * sinDeg
                    : posDoorZ - scaleDoorZ / 2 * cosDeg;
            }

            hingePosCopy.y = posDoorY;

            return hingePosCopy;
        }

        public Vector3 CalculateHingeRotation()
        {
            var rotDoorX = transform.localEulerAngles.x;
            var rotDoorZ = transform.localEulerAngles.z;

            var hingeRotCopy = hinge.transform.localEulerAngles;

            hingeRotCopy.x = rotDoorX;
            if (SwingState == 1) hingeRotCopy.y = RotationTimeline[0].FrontAngle;
            if (SwingState == 2) hingeRotCopy.y = RotationTimeline[0].MiddleAngle;
            if (SwingState == 3) hingeRotCopy.y = RotationTimeline[0].BackAngle;
            hingeRotCopy.z = rotDoorZ;

            return hingeRotCopy;
        }

        public IEnumerator Swing(bool front)
        {
            MovementPending = true;

            var t = transform;
            var CurrentRotationBlock = RotationTimeline[CurrentRotationBlockIndex];
            var TimeProgression = 0f;

            SwingStart = Quaternion.Euler(0, CurrentRotationBlock.FrontAngle, 0);
            SwingMid = Quaternion.Euler(0, CurrentRotationBlock.MiddleAngle, 0);
            SwingEnd = Quaternion.Euler(0, CurrentRotationBlock.BackAngle, 0);

            if (DoorScale == ScaleOfDoor.Unity3DUnits && PivotPosition == PositionOfPivot.Centered)
            {
                t = hinge.transform;

                if (TimesMoved == 0 && SwingState == 1)
                    t.rotation = SwingStart;

                if (TimesMoved == 0 && SwingState == 2)
                    t.rotation = SwingMid;

                if (TimesMoved == 0 && SwingState == 3)
                    t.rotation = SwingEnd;
            }

            else if (PivotPosition == PositionOfPivot.CorrectlyPositioned)
            {
                if (TimesMoved == 0) t.rotation = StartRotation;
            }

            if (TimesMoved < CurrentRotationBlock.TimesMoveable || CurrentRotationBlock.TimesMoveable == 0)
            {
                while (TimeProgression <= 1 / CurrentRotationBlock.Speed)
                {
                    TimeProgression += Time.deltaTime;
                    var RotationProgression = Mathf.Clamp01(TimeProgression / (1 / CurrentRotationBlock.Speed));
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
                            RotationCurveValue = RotationProgression * RotationProgression *
                                                 (3f - 2f * RotationProgression);
                            break;
                        case RotationTimelineData.RotationCurvePreset.Smootherstep:
                            RotationCurveValue = RotationProgression * RotationProgression * RotationProgression *
                                                 (RotationProgression * (6f * RotationProgression - 15f) + 10f);
                            break;
                        case RotationTimelineData.RotationCurvePreset.Exponential:
                            RotationCurveValue = RotationProgression * RotationProgression;
                            break;
                        case RotationTimelineData.RotationCurvePreset.Hermite:
                            RotationCurveValue = RotationProgression * RotationProgression *
                                                 (3.0f - 2.0f * RotationProgression);
                            break;
                        default:
                            RotationCurveValue = CurrentRotationBlock.CustomCurve.Evaluate(RotationProgression);
                            break;
                    }

                    if (front)
                    {
                        if (SwingState == 1)
                            t.rotation = RotationTools.Lerp(SwingStart, SwingMid, RotationCurveValue, ShortestWay);
                        if (SwingState == 2)
                            t.rotation = RotationTools.Lerp(SwingMid, SwingEnd, RotationCurveValue, ShortestWay);
                        if (SwingState == 3)
                            t.rotation = RotationTools.Lerp(SwingEnd, SwingMid, RotationCurveValue, ShortestWay);
                    }

                    if (!front)
                    {
                        if (SwingState == 3)
                            t.rotation = RotationTools.Lerp(SwingEnd, SwingMid, RotationCurveValue, ShortestWay);
                        if (SwingState == 2)
                            t.rotation = RotationTools.Lerp(SwingMid, SwingStart, RotationCurveValue, ShortestWay);
                        if (SwingState == 1)
                            t.rotation = RotationTools.Lerp(SwingStart, SwingMid, RotationCurveValue, ShortestWay);
                    }

                    yield return null;
                }

                if (front && SwingState == 1) SwingState++;
                else if (front && SwingState == 2) SwingState++;
                else if (front && SwingState == 3) SwingState--;

                else if (!front && SwingState == 1) SwingState++;
                else if (!front && SwingState == 2) SwingState--;
                else if (!front && SwingState == 3) SwingState--;

                TimesMoved++;

                if (TimesMoved == CurrentRotationBlock.TimesMoveable && CurrentRotationBlock.TimesMoveable != 0)
                {
                    TimesMoved = 0;
                    CurrentRotationBlockIndex++;
                }
            }

            MovementPending = false;
        }

        [Serializable]
        public class RotationTimelineData
        {
            public InitialSwingState InitialState;
            
            public enum InitialSwingState
            {
                Front,
                Middle,
                Back
            }

           
            public float BackAngle;
            public float MiddleAngle;
            public float FrontAngle;
            
            [StayPositive] public float Speed;
            
            public RotationCurvePreset RotationCurve;
            [Curve("RotationCurve", 0, 0, 0, 1f, 1f)]
            public AnimationCurve CustomCurve;

            [HideInInspector] public string RotationIndex;
            
            public int TimesMoveable;
            
            public enum RotationCurvePreset
            {
                Custom,
                Linear,
                EaseIn,
                EaseOut,
                Smoothstep,
                Smootherstep,
                Exponential,
                Hermite
            }

        }
    }
}