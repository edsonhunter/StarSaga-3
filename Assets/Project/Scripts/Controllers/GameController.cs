using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using StarSaga3.Project.Script.Core;
using StarSaga3.Project.Script.Core.PowerUp;
using StarSaga3.Project.Script.Core.Pooling;
using StarSaga3.Project.Script.Models;
using StarSaga3.Project.Script.Views;
using UnityEngine;
using Vector2Int = StarSaga3.Project.Script.Models.Vector2Int;

namespace StarSaga3.Project.Script.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private ScoreController scoreController;
        [SerializeField] private BoardRenderer _boardView;
        [SerializeField] private PowerUpButtonController _stripLinePowerUp;
        [SerializeField] private PowerUpButtonController _explodePowerUp;
        [SerializeField] private PowerUpButtonController _colorPowerUp;
        [SerializeField] private ParticlePool _particlePool;
        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;

        private IGameService _gameEngine;
        private bool _isAnimating;
        private bool _powerUpActivated;
        private PowerUp _activePowerUp;
        private int _selectedX = -1;
        private int _selectedY = -1;

        private CancellationTokenSource _cancellationTokenSource;
        
        #region Unity
        private void Awake()
        {
            _gameEngine = new GameService();
            _boardView.TileClicked += OnTileClick;
            _stripLinePowerUp.OnPressed += ActivatePowerUp;
            _explodePowerUp.OnPressed += ActivatePowerUp;
            _colorPowerUp.OnPressed += ActivatePowerUp;
        }
        
        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
            _stripLinePowerUp.OnPressed -= ActivatePowerUp;
            _explodePowerUp.OnPressed -= ActivatePowerUp;
            _colorPowerUp.OnPressed -= ActivatePowerUp;
            _cancellationTokenSource?.Cancel();
        }

        private void Start()
        {
            var board = _gameEngine.StartGame(_boardWidth, _boardHeight);
            
            // Initialize View
            _boardView.Initialize(_boardWidth, _boardHeight);
            for(int y=0; y<_boardHeight; y++)
            {
                for(int x=0; x<_boardWidth; x++)
                {
                    _boardView.SetTile(x, y, board[y, x].Type);
                }
            }

            _stripLinePowerUp.Initialize(new StripedPowerUp(true));
            _explodePowerUp.Initialize(new ExplodePowerUp(2));
            _colorPowerUp.Initialize(new ColorPowerUp());

            StartHintLoop();
        }
        #endregion

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            ResetHints();
            
            if (index >= boardSequences.Count)
            {
                ResetHints();
                onComplete();
                return;
            }

            BoardSequence boardSequence = boardSequences[index];

            float duration = 0.2f;

            // Trigger particles for exploded tiles right as animation begins
            if (_particlePool != null)
            {
                foreach (var pos in boardSequence.MatchedPosition)
                {
                    _particlePool.PlayEffect(new Vector3(pos.x, pos.y, -0.5f)); 
                }
            }

            // Sequential Animation logic via Callbacks
            // 1. Destroy
            _boardView.AnimateExplosion(boardSequence.MatchedPosition, duration, () =>
            {
                scoreController.AddScore(boardSequence.ScoreToAdd);
                
                // 2. Move
                var moves = new List<(Vector2Int from, Vector2Int to)>();
                foreach(var m in boardSequence.MovedTiles) moves.Add((m.From, m.To));
                
                _boardView.AnimateMove(moves, duration, () =>
                {
                    // 3. Spawn
                    var spawns = new List<(Vector2Int pos, int type)>();
                    foreach(var a in boardSequence.AddedTiles) spawns.Add((a.Position, a.Type));

                    _boardView.AnimateSpawn(spawns, duration, () =>
                    {
                        AnimateBoard(boardSequences, index + 1, onComplete);
                    });
                });
            });
        }

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_powerUpActivated)
            {
                _isAnimating = true;

                List<BoardSequence> powerUpResult = _gameEngine.UsePowerUp(x, y);

                int shockwaveType = 1; // Default to radial explode
                if (_activePowerUp is StripedPowerUp) shockwaveType = 2; // Line Expand
                if (_activePowerUp is ColorPowerUp) shockwaveType = 3;   // Scatter points

                var destroyedTiles = powerUpResult.Count > 0 ? powerUpResult[0].MatchedPosition : new List<Vector2Int>();
                _boardView.TriggerShockwave(shockwaveType, x, y, destroyedTiles);

                AnimateBoard(powerUpResult, 0, () =>
                {
                    _isAnimating = false;
                    _powerUpActivated = false;
                    _activePowerUp = null;
                });
                return;
            }
            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    // Visual Swap 1
                    _boardView.AnimateSwap(_selectedX, _selectedY, x, y, 0.2f, () =>
                    {
                        bool isValid = _gameEngine.IsValidMovement(_selectedX, _selectedY, x, y);
                        if (isValid)
                        {
                            List<BoardSequence> swapResult = _gameEngine.SwapTile(_selectedX, _selectedY, x, y);
                            AnimateBoard(swapResult, 0, () => _isAnimating = false);
                        }
                        else
                        {
                            // Revert Swap
                            _boardView.AnimateSwap(x, y, _selectedX, _selectedY, 0.2f, () => _isAnimating = false);
                        }
                        _selectedX = -1;
                        _selectedY = -1;
                    });
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
        }
        
        private void ActivatePowerUp(PowerUp powerUp)
        {
            if (_powerUpActivated) return;
            _powerUpActivated = true;
            _activePowerUp = powerUp;
            _gameEngine.ActivatePowerUp(powerUp);
        }

        private async void StartHintLoop()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);

                    var hint = _gameEngine.LookForHint();
                    if (hint.Count > 0 && !_isAnimating)
                    {
                        _boardView.HighlightHint(hint);
                    }
                }
            }
            catch (TaskCanceledException) { }
        }
        
        private void ResetHints()
        {
            _cancellationTokenSource?.Cancel();
            _boardView.ClearHighlights();
            StartHintLoop();
        }
    }
}
