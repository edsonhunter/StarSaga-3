using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public class TileSpotView : MonoBehaviour
    {
        public event Action<int, int> Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private GameObject _highlight;
        
        private int _x;
        private int _y;
        private bool _isHighlighted;
        
        #region Unity
        private void Awake()
        {
            _button.onClick.AddListener(OnTileClick);
            _highlight.SetActive(false);
        }
        #endregion

        public Tween AnimatedSetTile(GameObject tile)
        {
            SetHighlight(false);
            tile.transform.SetParent(transform);
            tile.transform.DOKill();

            return tile.transform.DOMove(transform.position, 0.3f);
        }

        public void SetPosition(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public void SetTile(GameObject tile)
        {
            tile.transform.SetParent(transform, false);
            tile.transform.position = transform.position;
        }
        
        public void ToggleHighlight()
        {
            _isHighlighted = !_isHighlighted;
            _highlight.SetActive(_isHighlighted);
        }
        
        public void SetHighlight(bool value)
        {
            _isHighlighted = value;
            _highlight.SetActive(value);
        }

        private void OnTileClick()
        {
            Clicked?.Invoke(_x, _y);
        }
    }
}
