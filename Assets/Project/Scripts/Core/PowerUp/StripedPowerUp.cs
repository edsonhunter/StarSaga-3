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

            int height = board.GetLength(0);
            int width = board.GetLength(1);

            if (_isHorizontal)
            {
                // Clear the entire row
                for (int col = 0; col < width; col++)
                {
                    affectedTiles.Add(new Vector2Int(col, y));
                }
            }
            else
            {
                // Clear the entire column
                for (int row = 0; row < height; row++)
                {
                    affectedTiles.Add(new Vector2Int(x, row));
                }
            }

            return affectedTiles;
        }
    }
}