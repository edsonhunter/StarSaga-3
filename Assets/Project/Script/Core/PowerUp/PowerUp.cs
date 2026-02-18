using System.Collections.Generic;
using System.Numerics;
using StarSaga3.Project.Script.Models;

namespace StarSaga3.Project.Script.Core.PowerUp
{
    public abstract class PowerUp
    {
        public abstract List<Vector2> Activate(PowerUpInfo powerUpInfo);
    }
}