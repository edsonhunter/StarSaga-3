using System.Collections.Generic;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.PowerUp
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
            var x = powerUpInfo.FromX;
            var y = powerUpInfo.FromY;
            
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