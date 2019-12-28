using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace DoorsPlus
{
    public class SlidingDoor : Door, IHierarchyIcon
    {
        public string EditorIconPath { get { return "SlidingDoor"; } }
        public List<SlidingTimelineData> SlidingTimeline;

        [Serializable]
        public class SlidingTimelineData
        {
            [HideInInspector]
            public string RotationIndex;

            public enum TypeOfSlide { SingleSlide, LoopedSlide }
            public TypeOfSlide Type = TypeOfSlide.SingleSlide;

            public enum SlidingAxis { X, Y, Z }
            public SlidingAxis Axis;

            public float Distance;

            [StayPositive]
            public float Speed;

            public enum RotationCurvePreset { Custom, Linear, EaseIn, EaseOut, Smoothstep, Smootherstep, Exponential, Hermite }
            public RotationCurvePreset SpeedCurve;

            [Curve("SpeedCurve", 0, 0, 0, 1f, 1f)]
            public AnimationCurve Curve;

            [ConditionalEnumHide("Type", 1)]
            public int TimesMoveable;
        }

        // Positions
        public Vector3 InitialPosition;
        public Vector3 StartPosition, EndPosition;
        public int PositionState;

        // Sliding Progress
        public int CurrentSlidingBlockIndex;

        public override void Start()
        {
            InitialPosition = transform.localPosition;
        }

        public IEnumerator Slide()
        {
            MovementPending = true;

            Transform t = transform;
            SlidingTimelineData CurrentSlidingBlock = SlidingTimeline[CurrentSlidingBlockIndex];
            float TimeProgression = 0f;

            if (CurrentSlidingBlock.Type == SlidingTimelineData.TypeOfSlide.SingleSlide)
                CurrentSlidingBlock.TimesMoveable = 1;

            StartPosition = InitialPosition;

            if (CurrentSlidingBlock.Axis == SlidingTimelineData.SlidingAxis.X) EndPosition = StartPosition + t.TransformDirection(new Vector3(CurrentSlidingBlock.Distance, 0, 0));
            else if (CurrentSlidingBlock.Axis == SlidingTimelineData.SlidingAxis.Y) EndPosition = StartPosition + t.TransformDirection(new Vector3(0, CurrentSlidingBlock.Distance, 0));
            else if (CurrentSlidingBlock.Axis == SlidingTimelineData.SlidingAxis.Z) EndPosition = StartPosition + t.TransformDirection(new Vector3(0, 0, CurrentSlidingBlock.Distance));
           
            if (TimesMoved == 0) t.localPosition = StartPosition;

            if (TimesMoved < CurrentSlidingBlock.TimesMoveable || CurrentSlidingBlock.TimesMoveable == 0)
            {
                if (t.localPosition == (PositionState == 0 ? EndPosition : StartPosition)) PositionState ^= 1;

                while (TimeProgression <= (1 / CurrentSlidingBlock.Speed))
                {
                    TimeProgression += Time.deltaTime;
                    float SlideProgression = Mathf.Clamp01(TimeProgression / (1 / CurrentSlidingBlock.Speed));
                    float SpeedCurveValue;

                    switch (CurrentSlidingBlock.SpeedCurve)
                    {
                        case SlidingTimelineData.RotationCurvePreset.Linear:
                             SpeedCurveValue = SlideProgression;
                            break;
                        case SlidingTimelineData.RotationCurvePreset.EaseIn:
                             SpeedCurveValue = 1f - Mathf.Cos(SlideProgression * Mathf.PI * 0.5f);
                            break;
                        case SlidingTimelineData.RotationCurvePreset.EaseOut:
                            SpeedCurveValue = Mathf.Sin(SlideProgression * Mathf.PI * 0.5f);
                            break;
                        case SlidingTimelineData.RotationCurvePreset.Smoothstep:
                            SpeedCurveValue = SlideProgression * SlideProgression * (3f - 2f * SlideProgression);
                            break;
                        case SlidingTimelineData.RotationCurvePreset.Smootherstep:
                            SpeedCurveValue = SlideProgression * SlideProgression * SlideProgression * (SlideProgression * (6f * SlideProgression - 15f) + 10f);
                            break;
                        case SlidingTimelineData.RotationCurvePreset.Exponential:
                            SpeedCurveValue = SlideProgression * SlideProgression;
                            break;
                        case SlidingTimelineData.RotationCurvePreset.Hermite:
                            SpeedCurveValue = SlideProgression * SlideProgression * (3.0f - 2.0f * SlideProgression);
                            break;
                        default:
                            SpeedCurveValue = CurrentSlidingBlock.Curve.Evaluate(SlideProgression);
                            break;
                    }

                    if (PositionState == 0) // Door is closed
                        t.localPosition = Vector3.Lerp(StartPosition, EndPosition, SpeedCurveValue);
    

                    if (PositionState == 1) // Door is opened
                        t.localPosition = Vector3.Lerp(EndPosition, StartPosition, SpeedCurveValue);
         
                    yield return null;
                }

                TimesMoved++;

                if (TimesMoved == CurrentSlidingBlock.TimesMoveable && CurrentSlidingBlock.TimesMoveable != 0)
                {
                    TimesMoved = 0;
                    CurrentSlidingBlockIndex++;
                }
            }
            MovementPending = false;
        }
    }
}
