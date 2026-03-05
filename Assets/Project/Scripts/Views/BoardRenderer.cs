using System;
using System.Collections.Generic;
using DG.Tweening;
using StarSaga3.Project.Script.Core;
using UnityEngine;
using StarSaga3.Project.Script.Models;
using Vector2Int = StarSaga3.Project.Script.Models.Vector2Int;

namespace StarSaga3.Project.Script.Views
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BoardRenderer : MonoBehaviour, IBoardView
    {
        public event Constants.TileClickDelegate TileClicked;

        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _boardMaterial;
        [SerializeField] private Color[] _tileColors;
        [SerializeField] private float _padding = 0.1f;

        private VisualTile[,] _grid;
        private List<VisualTile> _visualTiles = new List<VisualTile>(); // Flat list for rendering
        private int _width;
        private int _height;
        
        private List<Vector3> _vertices = new List<Vector3>();
        private List<Color> _colors = new List<Color>();
        private List<Vector2> _uvs = new List<Vector2>();
        private List<Vector2> _uv2s = new List<Vector2>();
        private List<int> _triangles = new List<int>();
        private Mesh _mesh;
        private bool _isDirty;

        private void Awake()
        {
            _mesh = new Mesh();
            _mesh.name = "BoardMesh";
            _mesh.MarkDynamic();
            _meshFilter.mesh = _mesh;
            if (_boardMaterial != null) _meshRenderer.material = _boardMaterial;
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
            
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = Math.Max(width, height) / 2f + 1f;
            }
        }

        public void SetTile(int x, int y, int type)
        {
            if (type == -1)
            {
                ClearTile(x, y);
                return;
            }

            Color color = (type >= 0 && type < _tileColors.Length) ? _tileColors[type] : Color.white;
            var tile = new VisualTile(new UnityEngine.Vector2(x, y), type, color);
            
            _grid[x, y] = tile;
            if (!_visualTiles.Contains(tile)) _visualTiles.Add(tile);
            
            _isDirty = true;
        }

        public void ClearTile(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return;
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

            _grid[x1, y1] = t2;
            _grid[x2, y2] = t1;

            Sequence seq = DOTween.Sequence();

            if (t1 != null)
            {
                seq.Join(DOTween.To(() => t1.Position, v => { t1.Position = v; _isDirty = true; }, new UnityEngine.Vector2(x2, y2), duration));
            }
            if (t2 != null)
            {
                seq.Join(DOTween.To(() => t2.Position, v => { t2.Position = v; _isDirty = true; }, new UnityEngine.Vector2(x1, y1), duration));
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateMove(List<(Vector2Int from, Vector2Int to)> moves, float duration, Action onComplete)
        {
            Sequence seq = DOTween.Sequence();
            
            foreach (var move in moves)
            {
                int fx = move.from.x;
                int fy = move.from.y;
                int tx = move.to.x;
                int ty = move.to.y;

                var tile = _grid[fx, fy];
                if (tile != null)
                {
                    _grid[fx, fy] = null; 
                    _grid[tx, ty] = tile; 
                    
                    seq.Join(DOTween.To(() => tile.Position, v => { tile.Position = v; _isDirty = true; }, new UnityEngine.Vector2(tx, ty), duration));
                }
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateExplosion(List<Vector2Int> positions, float duration, Action onComplete)
        {
            Sequence seq = DOTween.Sequence();

            foreach (var pos in positions)
            {
                int x = pos.x;
                int y = pos.y;
                var tile = _grid[x, y];
                if (tile != null)
                {
                    seq.Join(DOTween.To(() => tile.Scale, v => { tile.Scale = v; _isDirty = true; }, 0f, duration));
                }
            }

            seq.OnComplete(() =>
            {
                foreach (var pos in positions) ClearTile(pos.x, pos.y);
                onComplete?.Invoke();
            });
        }

        public void AnimateSpawn(List<(Vector2Int pos, int type)> spawns, float duration, Action onComplete)
        {
            Sequence seq = DOTween.Sequence();

            foreach (var spawn in spawns)
            {
                int x = spawn.pos.x;
                int y = spawn.pos.y;
                
                Color color = (spawn.type >= 0 && spawn.type < _tileColors.Length) ? _tileColors[spawn.type] : Color.white;
                var tile = new VisualTile(new UnityEngine.Vector2(x, y), spawn.type, color);
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

        public void HighlightHint(List<Vector2Int> positions)
        {
            foreach (var p in positions)
            {
                SetHighlight(p.x, p.y, true);
            }

            DOVirtual.DelayedCall(2f, () =>
            {
                foreach (var p in positions)
                {
                    SetHighlight(p.x, p.y, false);
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

        public void TriggerShockwave(int x, int y)
        {
            if (_boardMaterial == null) return;
            
            _boardMaterial.SetVector("_ExplosionCenter", new Vector4(x, y, 0, 0));
            _boardMaterial.SetFloat("_ExplosionTime", 0f);
            
            DOTween.To(() => _boardMaterial.GetFloat("_ExplosionTime"), 
                       v => _boardMaterial.SetFloat("_ExplosionTime", v), 
                       1f, 0.5f).SetEase(Ease.OutSine);
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
            if (tileCount == 0)
            {
                _mesh.Clear(true);
                return;
            }

            _vertices.Clear();
            _colors.Clear();
            _uvs.Clear();
            _uv2s.Clear();
            _triangles.Clear();

            for (int i = 0; i < tileCount; i++)
            {
                var tile = _visualTiles[i];
                int vIndex = i * 4;

                UnityEngine.Vector2 center = tile.Position;
                float halfSize = 0.5f * tile.Scale - _padding;

                Color c = tile.Color;

                _vertices.Add(new Vector3(center.x - halfSize, center.y - halfSize, 0));
                _vertices.Add(new Vector3(center.x - halfSize, center.y + halfSize, 0));
                _vertices.Add(new Vector3(center.x + halfSize, center.y + halfSize, 0));
                _vertices.Add(new Vector3(center.x + halfSize, center.y - halfSize, 0));

                _colors.Add(c);
                _colors.Add(c);
                _colors.Add(c);
                _colors.Add(c);

                _uvs.Add(new UnityEngine.Vector2(0, 0));
                _uvs.Add(new UnityEngine.Vector2(0, 1));
                _uvs.Add(new UnityEngine.Vector2(1, 1));
                _uvs.Add(new UnityEngine.Vector2(1, 0));

                float highlightVal = tile.IsHighlighted ? 1f : 0f;
                _uv2s.Add(new UnityEngine.Vector2(highlightVal, highlightVal));
                _uv2s.Add(new UnityEngine.Vector2(highlightVal, highlightVal));
                _uv2s.Add(new UnityEngine.Vector2(highlightVal, highlightVal));
                _uv2s.Add(new UnityEngine.Vector2(highlightVal, highlightVal));

                _triangles.Add(vIndex + 0);
                _triangles.Add(vIndex + 1);
                _triangles.Add(vIndex + 2);
                _triangles.Add(vIndex + 0);
                _triangles.Add(vIndex + 2);
                _triangles.Add(vIndex + 3);
            }

            _mesh.Clear(true);
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetUVs(1, _uv2s);
            _mesh.SetTriangles(_triangles, 0);
            
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
        }
    }
}
