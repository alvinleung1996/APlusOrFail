using UnityEngine;

namespace APlusOrFail
{
    public static class MathUtil
    {
        /// <summary>
        ///  
        /// </summary>
        /// <param name="angle">the angle to normalize</param>
        /// <returns>[0, 360)</returns>
        public static float NormalizeRandianAngle(float angle)
        {
            const float twoPI = 2 * Mathf.PI;
            while (angle < 0)
            {
                angle += twoPI;
            }
            angle %= twoPI;
            return angle;
        }

        public static bool Approximately(float a, float b, float tolerate)
        {
            return Mathf.Abs(a - b) <= tolerate;
        }

        public static Vector2 Rotate90DegCW(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        public static Vector2Int Rotate90DegCW(this Vector2Int vector)
        {
            return new Vector2Int(vector.y, -vector.x);
        }

        public static Vector2 Rotate90DegCCW(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        public static Vector2Int Rotate90DegCCW(this Vector2Int vector)
        {
            return new Vector2Int(-vector.y, vector.x);
        }
    }
}
