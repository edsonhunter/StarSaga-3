using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IGravityHandler
    {
        List<MovedTileInfo> MoveTiles(Tile[,] board, List<Vector2Int> matchedPositions);
        List<AddedTileInfo> FillEmptyTiles(Tile[,] board, IReadOnlyList<int> tileTypes, ref int tileCount);
    }
}
