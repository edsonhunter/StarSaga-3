using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;
        private PowerUp _powerUp;

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        return true;
                    }

                    if (y > 1 &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            Swap(newBoard, fromX, fromY, toX, toY);

            return Cascade(newBoard);
        }
        
        private void Swap(List<List<Tile>> board, int fromX, int fromY, int toX, int toY)
        {
            (board[toY][toX], board[fromY][fromX]) = (board[fromY][fromX], board[toY][toX]);
        }
        
        private List<BoardSequence> Cascade(List<List<Tile>> boardTiles)
        {
            List<BoardSequence> boardSequences = new();
            List<List<bool>> matches = FindMatches(boardTiles);

            while (HasMatch(matches))
            {
                var matchedPosition = AddMatchedPositions(boardTiles, matches, out int tileScore);
                var movedTiles =  MoveTiles(boardTiles);
                var addedTiles =  AddNewTiles(boardTiles);

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTiles,
                    AddedTiles = addedTiles,
                    ScoreToAdd = tileScore
                };

                boardSequences.Add(sequence);
                
                matches = FindMatches(boardTiles);
            }

            _boardTiles = boardTiles;
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
        
        private List<MovedTileInfo> MoveTiles(List<List<Tile>> board)
        {
            List<MovedTileInfo> movedTilesList = new();

            int width = board[0].Count;
            int height = board.Count;

            for (int x = 0; x < width; x++)
            {
                int writeY = height - 1;

                for (int y = height - 1; y >= 0; y--)
                {
                    Tile currentTile = board[y][x];

                    if (currentTile.Type != -1)
                    {
                        if (y != writeY)
                        {
                            board[writeY][x] = currentTile;
                            board[y][x] = new Tile { Id = -1, Type = -1, Score = -1 };

                            movedTilesList.Add(new MovedTileInfo
                            {
                                From = new Vector2Int(x, y),
                                To = new Vector2Int(x, writeY)
                            });
                        }

                        writeY--;
                    }
                }
            }

            return movedTilesList;
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

        private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile { Id = tile.Id, Type = tile.Type, Score = tile.Score });
                }
            }

            return newBoard;
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
                for (int x = 0; x < newBoard.Count; x++)
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

        #region PowerUp
        
        public void ActivatePowerUp(PowerUp powerUp)
        {
            _powerUp = powerUp;
        }

        public List<BoardSequence> UsePowerUp(int x, int y)
        {
            if (_powerUp == null) throw new InvalidOperationException("Cannot use PowerUp without PowerUp");

            List<Vector2Int> affectedTiles =
                _powerUp.Activate(new PowerUpInfo { Board = _boardTiles, FromX = x, FromY = y });

            foreach (var pos in affectedTiles)
            {
                _boardTiles[pos.y][pos.x] = new Tile
                {
                    Id = -1,
                    Type = -1,
                    Score = -1
                };
            }

            _powerUp = null; // reset after use
            return Cascade(_boardTiles);
        }
        
        #endregion
    }
}
