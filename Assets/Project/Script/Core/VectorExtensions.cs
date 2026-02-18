using System.Numerics;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace StarSaga3.Project.Script.Core
{
    public static class VectorExtensions
    {
        public static Vector2 ToNumerics(this UnityEngine.Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 ToNumerics(this UnityEngine.Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 ToNumerics(this UnityEngine.Vector2Int v)
        {
            return new Vector2(v.x, v.y);
        }

        public static UnityEngine.Vector2 ToUnity(this Vector2 v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }

        public static UnityEngine.Vector2Int ToUnityInt(this Vector2 v)
        {
            return new UnityEngine.Vector2Int(Mathf.RoundToInt(v.X), Mathf.RoundToInt(v.Y));
        }
    }
}
