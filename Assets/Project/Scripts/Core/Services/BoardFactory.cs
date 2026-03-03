using System;
using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IBoardFactory
    {
        Tile[,] CreateBoard(int width, int height, IReadOnlyList<int> tileTypes, ref int tileCount);
    }

    public class BoardFactory : IBoardFactory
    {
        public Tile[,] CreateBoard(int width, int height, IReadOnlyList<int> tileTypes, ref int tileCount)
        {
            Tile[,] board = new Tile[height, width];
            tileCount = 0;
            var random = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(tileTypes[i]);
                    }

                    if (x > 1 &&
                        board[y, x - 1].Type == board[y, x - 2].Type)
                    {
                        noMatchTypes.Remove(board[y, x - 1].Type);
                    }

                    if (y > 1 &&
                        board[y - 1, x].Type == board[y - 2, x].Type)
                    {
                        noMatchTypes.Remove(board[y - 1, x].Type);
                    }

                    int type = noMatchTypes[random.Next(0, noMatchTypes.Count)];
                    board[y, x] = new Tile
                    {
                        Id = tileCount++,
                        Type = type,
                        Score = type * 10
                    };
                }
            }

            return board;
        }
    }
}
