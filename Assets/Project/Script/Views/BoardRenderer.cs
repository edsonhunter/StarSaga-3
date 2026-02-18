using System;
using System.Collections.Generic;
using DG.Tweening;
using StarSaga3.Project.Script.Core;
using UnityEngine;

namespace StarSaga3.Project.Script.Views
{
    public class BoardRenderer : MonoBehaviour, IBoardView
    {
        public event Constants.TileClickDelegate TileClicked;

        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Color[] _tileColors;
        [SerializeField] private float _padding = 0.1f;

        private VisualTile[,] _grid;
        private List<VisualTile> _visualTiles = new List<VisualTile>(); // Flat list for rendering
        private int _width;
        private int _height;
        
        // Mesh Data Arrays
        private Vector3[] _vertices;
        private Color[] _colors;
        private int[] _triangles;
        private Mesh _mesh;
        private bool _isDirty;

        private void Awake()
        {
            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _meshFilter.mesh = _mesh;
        }

        private void Update()
        {
            if (_isDirty)
            {
                UpdateMesh();
                _isDirty = false;
            }

            HandleInput();
        }

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new VisualTile[width, height];
            
            // Camera setup to center board
            Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
            Camera.main.orthographicSize = Math.Max(width, height) / 2f + 1f;
        }

        public void SetTile(int x, int y, int type)
        {
            if (type == -1)
            {
                ClearTile(x, y);
                return;
            }

            Color color = (type >= 0 && type < _tileColors.Length) ? _tileColors[type] : Color.white;
            var tile = new VisualTile(new Vector2(x, y), type, color);
            
            _grid[x, y] = tile;
            if (!_visualTiles.Contains(tile)) _visualTiles.Add(tile);
            
            _isDirty = true;
        }

        public void ClearTile(int x, int y)
        {
            var tile = _grid[x, y];
            if (tile != null)
            {
                _visualTiles.Remove(tile);
                _grid[x, y] = null;
                _isDirty = true;
            }
        }

        public void AnimateSwap(int x1, int y1, int x2, int y2, float duration, Action onComplete)
        {
            var t1 = _grid[x1, y1];
            var t2 = _grid[x2, y2];

            // Swap in grid logic immediately (Visual state swap)
            _grid[x1, y1] = t2;
            _grid[x2, y2] = t1;

            Sequence seq = DOTween.Sequence();

            if (t1 != null)
            {
                seq.Join(DOTween.To(() => t1.Position, v => { t1.Position = v; _isDirty = true; }, new Vector2(x2, y2), duration));
            }
            if (t2 != null)
            {
                seq.Join(DOTween.To(() => t2.Position, v => { t2.Position = v; _isDirty = true; }, new Vector2(x1, y1), duration));
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateMove(List<(System.Numerics.Vector2 from, System.Numerics.Vector2 to)> moves, float duration, Action onComplete)
        {
            Sequence seq = DOTween.Sequence();
            
            // Create a temporary backup of the grid layout for the animation logic if needed,
            // but for simplicity we rely on the fact that GameService already moved logic tiles.
            // We just need to animate visual tiles from A to B.

            // Problem: GameService logic already updated the board state.
            // We need to find the VisualTile that WAS at 'from' and move it to 'to'.
            // But we might have overlapping moves.
            
            // Standard approach:
            // 1. Identify all tiles moving.
            // 2. Clear their grid spots.
            // 3. Animate them.
            // 4. Place them in their new grid spots.

            foreach (var move in moves)
            {
                int fx = (int)move.from.X;
                int fy = (int)move.from.Y;
                int tx = (int)move.to.X;
                int ty = (int)move.to.Y;

                // Find the tile currently at 'from'
                // Note: Since logic happens before animation, we assume the visual representation hasn't been updated yet via SetTile calls
                // Actually, SetTile is likely not called for moves, only spawns.
                // We track our own state in _grid.
                
                var tile = _grid[fx, fy];
                if (tile != null)
                {
                    _grid[fx, fy] = null; // Remove from old spot
                    _grid[tx, ty] = tile; // Place in new spot (logically)
                    
                    seq.Join(DOTween.To(() => tile.Position, v => { tile.Position = v; _isDirty = true; }, new Vector2(tx, ty), duration));
                }
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateExplosion(List<System.Numerics.Vector2> positions, float duration, Action onComplete)
        {
            Sequence seq = DOTween.Sequence();

            foreach (var pos in positions)
            {
                int x = (int)pos.X;
                int y = (int)pos.Y;
                var tile = _grid[x, y];
                if (tile != null)
                {
                    seq.Join(DOTween.To(() => tile.Scale, v => { tile.Scale = v; _isDirty = true; }, 0f, duration));
                }
            }

            seq.OnComplete(() =>
            {
                foreach (var pos in positions) ClearTile((int)pos.X, (int)pos.Y);
                onComplete?.Invoke();
            });
        }

        public void AnimateSpawn(List<(System.Numerics.Vector2 pos, int type)> spawns, float duration, Action onComplete)
        {
            Sequence seq = DOTween.Sequence();

            foreach (var spawn in spawns)
            {
                int x = (int)spawn.pos.X;
                int y = (int)spawn.pos.Y;
                
                Color color = (spawn.type >= 0 && spawn.type < _tileColors.Length) ? _tileColors[spawn.type] : Color.white;
                var tile = new VisualTile(new Vector2(x, y), spawn.type, color);
                tile.Scale = 0f;

                _grid[x, y] = tile;
                _visualTiles.Add(tile);

                seq.Join(DOTween.To(() => tile.Scale, v => { tile.Scale = v; _isDirty = true; }, 1f, duration));
            }
            
            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void SetHighlight(int x, int y, bool active)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                var tile = _grid[x, y];
                if (tile != null)
                {
                    tile.IsHighlighted = active;
                    _isDirty = true;
                }
            }
        }

        public void HighlightHint(List<System.Numerics.Vector2> positions)
        {
            foreach (var p in positions)
            {
                SetHighlight((int)p.X, (int)p.Y, true);
            }

            // Auto turn off after 2 seconds
            DOVirtual.DelayedCall(2f, () =>
            {
                foreach (var p in positions)
                {
                    SetHighlight((int)p.X, (int)p.Y, false);
                }
            });
        }

        public void ClearHighlights()
        {
            foreach (var tile in _visualTiles)
            {
                if (tile.IsHighlighted)
                {
                    tile.IsHighlighted = false;
                    _isDirty = true;
                }
            }
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                int x = Mathf.RoundToInt(worldPos.x);
                int y = Mathf.RoundToInt(worldPos.y);

                if (x >= 0 && x < _width && y >= 0 && y < _height)
                {
                    TileClicked?.Invoke(x, y);
                }
            }
        }

        private void UpdateMesh()
        {
            int tileCount = _visualTiles.Count;
            if (_vertices == null || _vertices.Length != tileCount * 4)
            {
                _vertices = new Vector3[tileCount * 4];
                _colors = new Color[tileCount * 4];
                _triangles = new int[tileCount * 6];
            }

            for (int i = 0; i < tileCount; i++)
            {
                var tile = _visualTiles[i];
                int vIndex = i * 4;
                int tIndex = i * 6;

                Vector2 center = tile.Position;
                float halfSize = 0.5f * tile.Scale - _padding;

                // Adjust color for highlight
                Color c = tile.IsHighlighted ? Color.Lerp(tile.Color, Color.white, 0.5f) : tile.Color;

                _vertices[vIndex + 0] = new Vector3(center.x - halfSize, center.y - halfSize, 0);
                _vertices[vIndex + 1] = new Vector3(center.x - halfSize, center.y + halfSize, 0);
                _vertices[vIndex + 2] = new Vector3(center.x + halfSize, center.y + halfSize, 0);
                _vertices[vIndex + 3] = new Vector3(center.x + halfSize, center.y - halfSize, 0);

                _colors[vIndex + 0] = c;
                _colors[vIndex + 1] = c;
                _colors[vIndex + 2] = c;
                _colors[vIndex + 3] = c;

                _triangles[tIndex + 0] = vIndex + 0;
                _triangles[tIndex + 1] = vIndex + 1;
                _triangles[tIndex + 2] = vIndex + 2;
                _triangles[tIndex + 3] = vIndex + 0;
                _triangles[tIndex + 4] = vIndex + 2;
                _triangles[tIndex + 5] = vIndex + 3;
            }

            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;
        }
    }
}
