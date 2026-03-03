using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IHintProvider
    {
        List<Vector2Int> LookForHint(Tile[,] board, IMatchDetector matchDetector);
    }
}
