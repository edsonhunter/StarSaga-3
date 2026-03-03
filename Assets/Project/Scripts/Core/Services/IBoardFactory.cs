using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.Services
{
    public interface IBoardFactory
    {
        Tile[,] CreateBoard(int width, int height, IReadOnlyList<int> tileTypes, ref int tileCount);
    }
}
