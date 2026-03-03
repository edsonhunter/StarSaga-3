using System;
using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public class HintProvider : IHintProvider
    {
        private readonly List<Vector2Int> _hintBuffer = new List<Vector2Int>();
        private readonly Random _random = new Random();

        public List<Vector2Int> LookForHint(Tile[,] board, IMatchDetector matchDetector)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            _hintBuffer.Clear();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < width - 1)
                    {
                        if (matchDetector.IsValidMovement(board, x, y, x + 1, y))
                        {
                            _hintBuffer.Add(new Vector2Int(x, y));
                            _hintBuffer.Add(new Vector2Int(x + 1, y));
                        }
                    }

                    if (y < height - 1)
                    {
                        if (matchDetector.IsValidMovement(board, x, y, x, y + 1))
                        {
                            _hintBuffer.Add(new Vector2Int(x, y));
                            _hintBuffer.Add(new Vector2Int(x, y + 1));
                        }
                    }
                }
            }

            if (_hintBuffer.Count == 0)
                return new List<Vector2Int>();

            int pairCount = _hintBuffer.Count / 2;
            int randomPairIndex = _random.Next(pairCount);
            
            int firstIndex = randomPairIndex * 2;
            
            return new List<Vector2Int> 
            { 
                _hintBuffer[firstIndex], 
                _hintBuffer[firstIndex + 1] 
            };
        }
    }
}
