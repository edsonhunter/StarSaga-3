using System;

namespace StarSaga3.Project.Script.Models
{
    [Serializable]
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Vector2Int other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);

        public static bool operator ==(Vector2Int left, Vector2Int right) => left.Equals(right);
        public static bool operator !=(Vector2Int left, Vector2Int right) => !left.Equals(right);

        public override string ToString() => $"({x}, {y})";
    }
}
