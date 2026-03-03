using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IHintProvider
    {
        List<Vector2Int> LookForHint(Tile[,] board, IMatchDetector matchDetector);
    }

    public class HintProvider : IHintProvider
    {
        public List<Vector2Int> LookForHint(Tile[,] board, IMatchDetector matchDetector)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < width - 1 && matchDetector.IsValidMovement(board, x, y, x + 1, y))
                        return new List<Vector2Int> { new(x, y), new(x + 1, y) };

                    if (y < height - 1 && matchDetector.IsValidMovement(board, x, y, x, y + 1))
                        return new List<Vector2Int> { new(x, y), new(x, y + 1) };
                }
            }

            return new List<Vector2Int>();
        }
    }
}
