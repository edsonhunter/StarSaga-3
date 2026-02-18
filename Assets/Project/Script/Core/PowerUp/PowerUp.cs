using System.Collections.Generic;
using StarSaga3.Project.Script.Models;
using UnityEngine;

namespace StarSaga3.Project.Script.Core.PowerUp
{
    public abstract class PowerUp
    {
        public abstract List<Vector2Int> Activate(PowerUpInfo powerUpInfo);
    }
}