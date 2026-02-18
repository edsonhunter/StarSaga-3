using System.Collections.Generic;
using StarSaga3.Project.Script.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSaga3.Project.Script.Core
{
    public class GameService
    {
        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;
        private PowerUp.PowerUp _powerUp;

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> board = _boardTiles;

            if (fromX < 0 || fromX >= board[0].Count || fromY < 0 || fromY >= board.Count) return false;
            if (toX < 0 || toX >= board[0].Count || toY < 0 || toY >= board.Count) return false;

            // Swap
            (board[toY][toX], board[fromY][fromX]) = (board[fromY][fromX], board[toY][toX]);

            bool hasMatch = false;

            // Checks
            for (int y = 0; y < board.Count; y++)
            {
                for (int x = 0; x < board[y].Count; x++)
                {
                    if (x > 1 &&
                        board[y][x].Type == board[y][x - 1].Type &&
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        hasMatch = true;
                        break;
                    }

                    if (y > 1 &&
                        board[y][x].Type == board[y - 1][x].Type &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
                    {
                        hasMatch = true;
                        break;
                    }
                }
                if (hasMatch) break;
            }

            // Swap back
            (board[toY][toX], board[fromY][fromX]) = (board[fromY][fromX], board[toY][toX]);

            return hasMatch;
        }

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            Swap(_boardTiles, fromX, fromY, toX, toY);

            return Cascade(_boardTiles);
        }
        
        private void Swap(List<List<Tile>> board, int fromX, int fromY, int toX, int toY)
        {
            (board[toY][toX], board[fromY][fromX]) = (board[fromY][fromX], board[toY][toX]);
        }
        
        private List<BoardSequence> Cascade(List<List<Tile>> board)
        {
            var boardSequences = new List<BoardSequence>();

            while (true)
            {
                var matches = FindMatches(board);
                
                if (!HasMatch(matches))
                    break;
                
                List<Vector2Int> matchedPositions = AddMatchedPositions(board, matches, out int scoreToAdd);
                List<MovedTileInfo> movedTiles = MoveTiles(board, matchedPositions);
                List<AddedTileInfo> addedTiles = AddNewTiles(board);

                var seq = new BoardSequence
                {
                    MatchedPosition = matchedPositions,
                    MovedTiles = movedTiles,
                    AddedTiles = addedTiles,
                    ScoreToAdd = scoreToAdd
                };

                boardSequences.Add(seq);
            }

            _boardTiles = board;

            return boardSequences;
        }
        
        private List<Vector2Int> AddMatchedPositions(List<List<Tile>> board, List<List<bool>> matchedTiles, out int totalScore)
        {
            List<Vector2Int> matchedPositions = new();
            totalScore = 0;

            for (int y = 0; y < board.Count; y++)
            {
                for (int x = 0; x < board[y].Count; x++)
                {
                    if (matchedTiles[y][x])
                    {
                        matchedPositions.Add(new Vector2Int(x, y));
                        totalScore += board[y][x].Score;
                        board[y][x] = new Tile { Id = -1, Type = -1, Score = -1 };
                    }
                }
            }

            return matchedPositions;
        }
        
        private List<Vector2Int> AddMatchesFromPositions(List<List<Tile>> board, List<Vector2Int> positions, out int scoreGained)
        {
            var unique = new HashSet<(int x, int y)>();
            var result = new List<Vector2Int>();
            scoreGained = 0;

            foreach (var p in positions)
            {
                // validate bounds
                if (p.y < 0 || p.y >= board.Count) continue;
                if (p.x < 0 || p.x >= board[p.y].Count) continue;

                if (!unique.Add((p.x, p.y))) continue; // skip duplicates

                // Only count/clear existing tiles
                if (board[p.y][p.x].Type != -1)
                {
                    result.Add(new Vector2Int(p.x, p.y));
                    scoreGained += board[p.y][p.x].Score;
                    board[p.y][p.x] = new Tile { Id = -1, Type = -1, Score = 0 };
                }
            }

            return result;
        }
        
        private List<MovedTileInfo> MoveTiles(List<List<Tile>> board, List<Vector2Int> matchedPositions)
        {
            var moved = new List<MovedTileInfo>();
            int height = board.Count;
            int width  = board[0].Count;

            // Build a fast lookup of matched rows by column
            var matchedByColumn = new Dictionary<int, HashSet<int>>();
            foreach (var p in matchedPositions)
            {
                if (!matchedByColumn.TryGetValue(p.x, out var rows))
                {
                    rows = new HashSet<int>();
                    matchedByColumn[p.x] = rows;
                }
                rows.Add(p.y);
            }

            // Only process columns that actually had matched tiles
            foreach (var kv in matchedByColumn)
            {
                int x = kv.Key;
                var matchedRows = kv.Value;

                int writeY = height - 1; // lowest slot we can write into

                // Compact this column: skip matched (empties), pack non-empty tiles down
                for (int y = height - 1; y >= 0; y--)
                {
                    // Treat matched cells as empty; never try to move from them
                    if (matchedRows.Contains(y)) continue;

                    var tile = board[y][x];
                    if (tile.Type == -1) continue; // already empty, nothing to move

                    if (y != writeY)
                    {
                        // Move tile down to writeY
                        board[writeY][x] = tile;
                        board[y][x] = new Tile { Id = -1, Type = -1, Score = 0 };

                        moved.Add(new MovedTileInfo
                        {
                            From = new Vector2Int(x, y),
                            To   = new Vector2Int(x, writeY)
                        });
                    }

                    writeY--;
                }

                // Everything above the last written tile becomes empty
                for (int y = writeY; y >= 0; y--)
                {
                    board[y][x] = new Tile { Id = -1, Type = -1, Score = 0 };
                }
            }

            return moved;
        }
        
        private List<AddedTileInfo> AddNewTiles(List<List<Tile>> board)
        {
            List<AddedTileInfo> addedTiles = new();

            for (int y = board.Count - 1; y >= 0; y--)
            {
                for (int x = board[y].Count - 1; x >= 0; x--)
                {
                    if (board[y][x].Type == -1)
                    {
                        int tileType = Random.Range(0, _tilesTypes.Count);

                        Tile tile = board[y][x];
                        tile.Id = _tileCount++;
                        tile.Type = _tilesTypes[tileType];
                        tile.Score = tile.Type * 10;

                        addedTiles.Add(new AddedTileInfo
                        {
                            Position = new Vector2Int(x, y),
                            Type = tile.Type
                        });
                    }
                }
            }

            return addedTiles;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
        {
            List<List<Tile>> board = new(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile { Id = -1, Type = -1, Score = -1});
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    if (x > 1 &&
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        noMatchTypes.Remove(board[y][x - 1].Type);
                    }

                    if (y > 1 &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
                    {
                        noMatchTypes.Remove(board[y - 1][x].Type);
                    }

                    board[y][x].Id = _tileCount++;
                    board[y][x].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                    board[y][x].Score = board[y][x].Type * 10;
                }
            }

            return board;
        }

        private static List<List<bool>> FindMatches(List<List<Tile>> newBoard)
        {
            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                matchedTiles.Add(new List<bool>(newBoard[y].Count));
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y][x - 1] = true;
                        matchedTiles[y][x - 2] = true;
                    }

                    if (y > 1 &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y - 1][x] = true;
                        matchedTiles[y - 2][x] = true;
                    }
                }
            }

            return matchedTiles;
        }

        private static bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
            {
                for (int x = 0; x < list[y].Count; x++)
                {
                    if (list[y][x])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public List<Vector2Int> LookForHint()
        {
            int height = _boardTiles.Count;
            int width = _boardTiles[0].Count;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < width - 1 && IsValidMovement(x, y, x + 1, y))
                        return new List<Vector2Int> { new(x, y), new(x + 1, y) };

                    if (y < height - 1 && IsValidMovement(x, y, x, y + 1))
                        return new List<Vector2Int> { new(x, y), new(x, y + 1) };
                }
            }

            return new List<Vector2Int>();
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

            List<Vector2Int> affected = _powerUp.Activate(info) ?? new List<Vector2Int>();
            _powerUp = null;

            if (affected.Count == 0)
                return sequences;
            
            List<Vector2Int> matchedPositions = AddMatchesFromPositions(_boardTiles, affected, out int scoreToAdd);
            List<MovedTileInfo> moved = MoveTiles(_boardTiles, matchedPositions);
            List<AddedTileInfo> added = AddNewTiles(_boardTiles);

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
