using System.Collections.Generic;
using StarSaga3.Project.Script.Models;
using StarSaga3.Project.Script.Core.Services;

namespace StarSaga3.Project.Script.Core
{
    public class GameService : IGameService
    {
        private Tile[,] _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;
        private PowerUp.PowerUp _powerUp;

        private List<List<bool>> _matchBuffer;

        private readonly IBoardFactory _boardFactory;
        private readonly IMatchDetector _matchDetector;
        private readonly IGravityHandler _gravityHandler;
        private readonly IHintProvider _hintProvider;

        public GameService()
        {
            _boardFactory = new BoardFactory();
            _matchDetector = new MatchDetector();
            _gravityHandler = new GravityHandler();
            _hintProvider = new HintProvider();
        }

        public bool IsValidMovement(int x1, int y1, int x2, int y2)
        {
            return _matchDetector.IsValidMovement(_boardTiles, x1, y1, x2, y2);
        }

        public Tile[,] StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = _boardFactory.CreateBoard(boardWidth, boardHeight, _tilesTypes, ref _tileCount);
            
            _matchBuffer = new List<List<bool>>(boardHeight);
            for (int y = 0; y < boardHeight; y++)
            {
                var row = new List<bool>(boardWidth);
                for(int x = 0; x < boardWidth; x++) row.Add(false);
                _matchBuffer.Add(row);
            }

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int x1, int y1, int x2, int y2)
        {
            Swap(_boardTiles, x1, y1, x2, y2);
            return Cascade(_boardTiles);
        }
        
        private void Swap(Tile[,] board, int fromX, int fromY, int toX, int toY)
        {
            (board[toY, toX], board[fromY, fromX]) = (board[fromY, fromX], board[toY, toX]);
        }
        
        private List<BoardSequence> Cascade(Tile[,] board)
        {
            var boardSequences = new List<BoardSequence>();

            while (true)
            {
                var matches = _matchDetector.FindMatches(board, _matchBuffer);
                
                if (!_matchDetector.HasMatch(matches))
                    break;
                
                List<Vector2Int> matchedPositions = AddMatchedPositions(board, matches, out int scoreToAdd);
                List<MovedTileInfo> movedTiles = _gravityHandler.MoveTiles(board, matchedPositions);
                List<AddedTileInfo> addedTiles = _gravityHandler.FillEmptyTiles(board, _tilesTypes, ref _tileCount);

                var seq = new BoardSequence
                {
                    MatchedPosition = matchedPositions,
                    MovedTiles = movedTiles,
                    AddedTiles = addedTiles,
                    ScoreToAdd = scoreToAdd
                };

                boardSequences.Add(seq);
            }

            return boardSequences;
        }
        
        private List<Vector2Int> AddMatchedPositions(Tile[,] board, List<List<bool>> matchedTiles, out int totalScore)
        {
            List<Vector2Int> matchedPositions = new();
            totalScore = 0;
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (matchedTiles[y][x])
                    {
                        matchedPositions.Add(new Vector2Int(x, y));
                        totalScore += board[y, x].Score;
                        board[y, x] = new Tile { Id = -1, Type = -1, Score = -1 };
                        matchedTiles[y][x] = false;
                    }
                }
            }

            return matchedPositions;
        }
        
        private List<Vector2Int> AddMatchesFromPositions(Tile[,] board, List<Vector2Int> positions, out int scoreGained)
        {
            var unique = new HashSet<(int x, int y)>();
            var result = new List<Vector2Int>();
            scoreGained = 0;
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            foreach (var p in positions)
            {
                int px = p.x;
                int py = p.y;

                if (py < 0 || py >= height) continue;
                if (px < 0 || px >= width) continue;

                if (!unique.Add((px, py))) continue;

                if (board[py, px].Type != -1)
                {
                    result.Add(new Vector2Int(px, py));
                    scoreGained += board[py, px].Score;
                    board[py, px] = new Tile { Id = -1, Type = -1, Score = 0 };
                }
            }

            return result;
        }
        
        public List<Vector2Int> LookForHint()
        {
            return _hintProvider.LookForHint(_boardTiles, _matchDetector);
        }

        #region PowerUp
        
        public void ActivatePowerUp(PowerUp.PowerUp powerUp)
        {
            _powerUp = powerUp;
        }

        public List<BoardSequence> UsePowerUp(int x, int y)
        {
            var sequences = new List<BoardSequence>();

            if (_powerUp == null)
                return sequences;

            var info = new PowerUpInfo { Board = _boardTiles, FromX =  x, FromY = y };

            List<Vector2Int> affected = new List<Vector2Int>();
            var rawAffected = _powerUp.Activate(info);
            if (rawAffected != null)
            {
                affected.AddRange(rawAffected);
            }

            _powerUp = null;

            if (affected.Count == 0)
                return sequences;
            
            List<Vector2Int> matchedPositions = AddMatchesFromPositions(_boardTiles, affected, out int scoreToAdd);
            List<MovedTileInfo> moved = _gravityHandler.MoveTiles(_boardTiles, matchedPositions);
            List<AddedTileInfo> added = _gravityHandler.FillEmptyTiles(_boardTiles, _tilesTypes, ref _tileCount);

            sequences.Add(new BoardSequence
            {
                MatchedPosition = matchedPositions,
                MovedTiles = moved,
                AddedTiles = added,
                ScoreToAdd = scoreToAdd
            });

            sequences.AddRange(Cascade(_boardTiles));

            return sequences;
        }
        
        #endregion
    }
}
