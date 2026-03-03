using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IMatchDetector
    {
        bool IsValidMovement(Tile[,] board, int x1, int y1, int x2, int y2);
        List<List<bool>> FindMatches(Tile[,] board, List<List<bool>> matchBuffer);
        bool HasMatch(List<List<bool>> list);
    }
}
