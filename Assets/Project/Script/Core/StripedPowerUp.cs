using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public class StripedPowerUp : PowerUp
    {
        private readonly bool _isHorizontal;

        public StripedPowerUp(bool isHorizontal)
        {
            _isHorizontal = isHorizontal;
        }

        public override List<Vector2Int> Activate(PowerUpInfo powerUpInfo)
        {
            var board =  powerUpInfo.Board;
            var x = powerUpInfo.OriginX;
            var y = powerUpInfo.OriginY;
            
            List<Vector2Int> affectedTiles = new();

            if (_isHorizontal)
            {
                // Clear the entire row
                for (int col = 0; col < board[y].Count; col++)
                {
                    affectedTiles.Add(new Vector2Int(col, y));
                }
            }
            else
            {
                // Clear the entire column
                for (int row = 0; row < board.Count; row++)
                {
                    affectedTiles.Add(new Vector2Int(x, row));
                }
            }

            return affectedTiles;
        }
    }
}