using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{
    public class LineEquation
    {
        public LineEquation(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;

            IsVertical = Mathf.Abs(End.x - start.x) < 0.00001f;
            M = (End.y - Start.y) / (End.x - Start.x);
            A = -M;
            B = 1;
            C = Start.y - M * Start.x;
        }

        public bool IsVertical { get; private set; }

        public float M { get; private set; }

        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }

        public bool IntersectsWithLine(LineEquation otherLine, out Vector2 intersectionVector2)
        {
            intersectionVector2 = new Vector2(0, 0);
            if (IsVertical && otherLine.IsVertical)
                return false;
            if (IsVertical || otherLine.IsVertical)
            {
                intersectionVector2 = GetIntersectionVector2IfOneIsVertical(otherLine, this);
                return true;
            }
            float delta = A * otherLine.B - otherLine.A * B;
            bool hasIntersection = Mathf.Abs(delta - 0) > 0.0001f;
            if (hasIntersection)
            {
                float x = (otherLine.B * C - B * otherLine.C) / delta;
                float y = (A * otherLine.C - otherLine.A * C) / delta;
                intersectionVector2 = new Vector2(x, y);
            }
            return hasIntersection;
        }

        private static Vector2 GetIntersectionVector2IfOneIsVertical(LineEquation line1, LineEquation line2)
        {
            LineEquation verticalLine = line2.IsVertical ? line2 : line1;
            LineEquation nonVerticalLine = line2.IsVertical ? line1 : line2;

            float y = (verticalLine.Start.x - nonVerticalLine.Start.x) *
                       (nonVerticalLine.End.y - nonVerticalLine.Start.y) /
                       ((nonVerticalLine.End.x - nonVerticalLine.Start.x)) +
                       nonVerticalLine.Start.y;
            float x = line1.IsVertical ? line1.Start.x : line2.Start.x;
            return new Vector2(x, y);
        }

        public Vector2 IntersectWithSegementOfLine(LineEquation otherLine)
        {
            Vector2 intersectionVector2 = new Vector2();
            bool hasIntersection = IntersectsWithLine(otherLine, out intersectionVector2);
            if (hasIntersection)
            {
                return intersectionVector2;
            }
            else
            {
                return Vector2.zero;
            }            
        }

        public static Vector2 FindIntersection(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2)
        {
            if (AreSegmentsIntersecting(s1,e1, s2, e2))
            {
                float a1 = e1.y - s1.y;
                float b1 = s1.x - e1.x;
                float c1 = a1 * s1.x + b1 * s1.y;

                float a2 = e2.y - s2.y;
                float b2 = s2.x - e2.x;
                float c2 = a2 * s2.x + b2 * s2.y;

                float delta = a1 * b2 - a2 * b1;
                //If lines are parallel, the result will be (NaN, NaN).
                return delta == 0 ? Vector2.zero
                    : new Vector2((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta);
            }
            else
            {
                return Vector2.zero;
            }
        }

        public static bool AreSegmentsIntersecting(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2)
        {
            // Get the segments' parameters.
            float dx12 = e1.x - s1.x;
            float dy12 = e1.y - s1.y;
            float dx34 = e2.x - s2.x;
            float dy34 = e2.y - s2.y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);
            float t1 = ((s1.x - s2.x) * dy34 + (s2.y - s1.y) * dx34) / denominator;
            if (float.IsInfinity(t1))
            {
                return false;
            }
            
            float t2 = ((s2.x - s1.x) * dy12 + (s1.y - s2.y) * dx12) / -denominator;

            // The segments intersect if t1 and t2 are between 0 and 1.
            return ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));
        }

    }
}