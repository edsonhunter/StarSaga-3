using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public class MatchDetector : IMatchDetector
    {
        public bool IsValidMovement(Tile[,] board, int x1, int y1, int x2, int y2)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height) return false;
            if (x2 < 0 || x2 >= width || y2 < 0 || y2 >= height) return false;

            (board[y2, x2], board[y1, x1]) = (board[y1, x1], board[y2, x2]);

            bool hasMatch = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x > 1 &&
                        board[y, x].Type != -1 &&
                        board[y, x].Type == board[y, x - 1].Type &&
                        board[y, x - 1].Type == board[y, x - 2].Type)
                    {
                        hasMatch = true;
                        break;
                    }

                    if (y > 1 &&
                        board[y, x].Type != -1 &&
                        board[y, x].Type == board[y - 1, x].Type &&
                        board[y - 1, x].Type == board[y - 2, x].Type)
                    {
                        hasMatch = true;
                        break;
                    }
                }
                if (hasMatch) break;
            }

            (board[y2, x2], board[y1, x1]) = (board[y1, x1], board[y2, x2]);

            return hasMatch;
        }

        public List<List<bool>> FindMatches(Tile[,] newBoard, List<List<bool>> matchBuffer)
        {
            int height = newBoard.GetLength(0);
            int width = newBoard.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matchBuffer[y][x] = false;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (newBoard[y, x].Type == -1) continue;

                    if (x > 1 &&
                        newBoard[y, x].Type == newBoard[y, x - 1].Type &&
                        newBoard[y, x - 1].Type == newBoard[y, x - 2].Type)
                    {
                        matchBuffer[y][x] = true;
                        matchBuffer[y][x - 1] = true;
                        matchBuffer[y][x - 2] = true;
                    }

                    if (y > 1 &&
                        newBoard[y, x].Type == newBoard[y - 1, x].Type &&
                        newBoard[y - 1, x].Type == newBoard[y - 2, x].Type)
                    {
                        matchBuffer[y][x] = true;
                        matchBuffer[y - 1][x] = true;
                        matchBuffer[y - 2][x] = true;
                    }
                }
            }

            return matchBuffer;
        }

        public bool HasMatch(List<List<bool>> list)
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
    }
}
