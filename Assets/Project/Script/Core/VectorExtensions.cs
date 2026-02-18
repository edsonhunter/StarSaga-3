using UnityEngine;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core
{
    public static class VectorExtensions
    {
        // To Unity Types
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }

        public static UnityEngine.Vector2Int ToUnity(this StarSaga3.Project.Script.Models.Vector2Int v)
        {
            return new UnityEngine.Vector2Int(v.x, v.y);
        }

        public static UnityEngine.Vector2 ToUnityFloat(this StarSaga3.Project.Script.Models.Vector2Int v)
        {
            return new UnityEngine.Vector2(v.x, v.y);
        }

        // To Decoupled Types
        public static StarSaga3.Project.Script.Models.Vector2Int ToDecoupled(this UnityEngine.Vector2Int v)
        {
            return new StarSaga3.Project.Script.Models.Vector2Int(v.x, v.y);
        }

        public static StarSaga3.Project.Script.Models.Vector2Int ToDecoupled(this System.Numerics.Vector2 v)
        {
            return new StarSaga3.Project.Script.Models.Vector2Int((int)v.X, (int)v.Y);
        }

        // To Numerics (for internal logic if needed)
        public static System.Numerics.Vector2 ToNumerics(this StarSaga3.Project.Script.Models.Vector2Int v)
        {
            return new System.Numerics.Vector2(v.x, v.y);
        }
    }
}
