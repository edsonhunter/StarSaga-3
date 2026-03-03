using UnityEngine;

namespace StarSaga3.Project.Script.Views
{
    public class VisualTile
    {
        public Vector2 Position;
        public float Scale;
        public Color Color;
        public int Type;
        public bool IsHighlighted;

        // Constructor for convenience
        public VisualTile(Vector2 position, int type, Color color)
        {
            Position = position;
            Type = type;
            Color = color;
            Scale = 1.0f;
        }
    }
}
