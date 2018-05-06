using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Components
{
    public static class MapGridRectExtensions
    {
        public enum Rotation : byte
        {
            None = 0,
            One = 1,
            Two = 2,
            Three = 3
        }


        public static IEnumerable<RectInt> GetLocalRects(this IEnumerable<MapGridRect> gridRects) => from gr in gridRects select gr.gridLocalRect;


        public static IEnumerable<RectInt> Rotate(this IEnumerable<RectInt> rects, Rotation rotation) => from r in rects select Rotate(r, rotation);

        public static RectInt Rotate(this RectInt rect, Rotation rotation)
        {
            Vector2Int p1 = rect.min.Rotate(rotation);
            Vector2Int p2 = rect.max.Rotate(rotation);
            Vector2Int min = new Vector2Int(Math.Min(p1.x, p2.x), Math.Min(p1.y, p2.y));
            Vector2Int max = new Vector2Int(Math.Max(p1.x, p2.x), Math.Max(p1.y, p2.y));
            Vector2Int size = new Vector2Int(Math.Abs(min.x - max.x), Math.Abs(min.y - max.y));
            return new RectInt(min, size);
        }


        public static IEnumerable<RectInt> Move(this IEnumerable<RectInt> rects, Vector2Int offset) => from r in rects select Move(r, offset);

        public static RectInt Move(this RectInt rect, Vector2Int offset) => new RectInt(rect.position + offset, rect.size);


        public static RectInt GetInnerBound(this IEnumerable<RectInt> rects) => rects.DefaultIfEmpty().Aggregate((bound, rect) =>
        {
            RectInt newBound = new RectInt
            {
                xMin = Math.Min(bound.xMin, rect.xMin),
                xMax = Math.Max(bound.xMax, rect.xMax),
                yMin = Math.Min(bound.yMin, rect.yMin),
                yMax = Math.Max(bound.yMax, rect.yMax)
            };
            return newBound;
        });

        public static RectInt GetOuterBound(this IEnumerable<RectInt> rects)
        {
            RectInt bound = rects.GetInnerBound();
            bound.xMin = Mathf.Min(bound.xMin, 0);
            bound.yMin = Mathf.Min(bound.yMin, 0);
            bound.xMax = Mathf.Max(bound.xMax, 0);
            bound.yMax = Mathf.Max(bound.yMax, 0);
            return bound;
        }


        public static Vector2Int Rotate(this Vector2Int vector, Rotation rotation)
        {
            switch ((byte)rotation % 4)
            {
                case 0: return vector;
                case 1: return vector.Rotate90DegCCW();
                case 2: return vector * -1;
                case 3: return vector.Rotate90DegCW();
                default: throw new InvalidOperationException();
            }
        }

        public static Vector2 Rotate(this Vector2 vector, Rotation rotation)
        {
            switch ((byte)rotation % 4)
            {
                case 0: return vector;
                case 1: return vector.Rotate90DegCCW();
                case 2: return vector * -1;
                case 3: return vector.Rotate90DegCW();
                default: throw new InvalidOperationException();
            }
        }
    }
}
