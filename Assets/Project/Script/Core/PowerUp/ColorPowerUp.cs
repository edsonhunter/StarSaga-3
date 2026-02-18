using System.Collections.Generic;
using StarSaga3.Project.Script.Models;
using UnityEngine;

namespace StarSaga3.Project.Script.Core.PowerUp
{
    public class ColorPowerUp : PowerUp
    {
        public override List<Vector2Int> Activate(PowerUpInfo powerUpInfo)
        {
            var board = powerUpInfo.Board;
            var x = powerUpInfo.FromX;
            var y = powerUpInfo.FromY;
            List<Vector2Int> affectedTiles = new();

            if (board == null || board.Count == 0)
                return affectedTiles;

            int targetType = board[y][x].Type;

            if (targetType == -1)
                return affectedTiles;

            for (int row = 0; row < board.Count; row++)
            {
                for (int col = 0; col < board[row].Count; col++)
                {
                    if (board[row][col].Type == targetType)
                    {
                        affectedTiles.Add(new Vector2Int(col, row));
                    }
                }
            }

            return affectedTiles;
        }
    }
}