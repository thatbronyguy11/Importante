using System.Collections;
using UnityEngine;

namespace DoorsPlus
{
    public class RotationTools : MonoBehaviour
    {
        public static IEnumerator Rotate(GameObject door, float initialAngle, float finalAngle, float speed, float rotationOffset, bool shortestWay)
        {
            Quaternion startRotation, endRotation, RotationOffset;

            RotationOffset = Quaternion.Euler(0, rotationOffset, 0);

            AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            float timeProgression = 0f;

            startRotation = Quaternion.Euler(0, initialAngle, 0);
            endRotation = Quaternion.Euler(0, finalAngle, 0);

            while (timeProgression <= (1 / speed))
            {
                timeProgression += Time.deltaTime;
                float rotationProgression = Mathf.Clamp01(timeProgression / (1 / speed));
                float rotationCurveValue = curve.Evaluate(rotationProgression);

                door.transform.rotation = Lerp(startRotation * RotationOffset, endRotation * RotationOffset, rotationCurveValue, shortestWay);

                yield return null;
            }
        }

        public static IEnumerator Slide(GameObject door, Vector3 initialPosition, Vector3 finalPosition, float speed)
        {
            Vector3 startPosition, endPosition;

            AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            float timeProgression = 0f;

            startPosition = initialPosition;
            endPosition = finalPosition;

            while (timeProgression <= (1 / speed))
            {
                timeProgression += Time.deltaTime;
                float slideProgression = Mathf.Clamp01(timeProgression / (1 / speed));
                float speedCurveValue = curve.Evaluate(slideProgression);

                door.transform.position = Vector3.Lerp(startPosition, endPosition, speedCurveValue);

                yield return null;
            }
        }

        public static Quaternion Lerp(Quaternion p, Quaternion q, float t, bool shortWay)
        {
            if (shortWay)
            {
                float dot = Quaternion.Dot(p, q);
                if (dot < 0.0f)
                    return Lerp(ScalarMultiply(p, -1.0f), q, t, true);
            }

            Quaternion r = Quaternion.identity;
            r.x = p.x * (1f - t) + q.x * (t);
            r.y = p.y * (1f - t) + q.y * (t);
            r.z = p.z * (1f - t) + q.z * (t);
            r.w = p.w * (1f - t) + q.w * (t);
            return r;
        }

        public static Quaternion Slerp(Quaternion p, Quaternion q, float t, bool shortWay)
        {
            float dot = Quaternion.Dot(p, q);
            if (shortWay)
            {
                if (dot < 0.0f)
                    return Slerp(ScalarMultiply(p, -1.0f), q, t, true);
            }

            float angle = Mathf.Acos(dot);
            Quaternion first = ScalarMultiply(p, Mathf.Sin((1f - t) * angle));
            Quaternion second = ScalarMultiply(q, Mathf.Sin((t) * angle));
            float division = 1f / Mathf.Sin(angle);
            return ScalarMultiply(Add(first, second), division);
        }

        public static Quaternion ScalarMultiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }

        public static Quaternion Add(Quaternion p, Quaternion q)
        {
            return new Quaternion(p.x + q.x, p.y + q.y, p.z + q.z, p.w + q.w);
        }
    }
}