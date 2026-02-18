using System.Collections.Generic;
using System.Numerics;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.PowerUp
{
    public class ExplodePowerUp : PowerUp
    {
        private readonly int _radius;

        public ExplodePowerUp(int radius)
        {
            _radius = radius;
        }

        public override List<Vector2> Activate(PowerUpInfo powerUpInfo)
        {
            var board =  powerUpInfo.Board;
            var x = powerUpInfo.FromX;
            var y = powerUpInfo.FromY;
            
            List<Vector2> affectedTiles = new();
            int height = board.Count;
            int width = board[0].Count;

            for (int row = y - _radius; row <= y + _radius; row++)
            {
                for (int col = x - _radius; col <= x + _radius; col++)
                {
                    if (row >= 0 && row < height && col >= 0 && col < width)
                    {
                        affectedTiles.Add(new Vector2(col, row));
                    }
                }
            }

            return affectedTiles;
        }
    }
}