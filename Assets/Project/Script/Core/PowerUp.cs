using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public abstract class PowerUp
    {
        public abstract List<Vector2Int> Activate(PowerUpInfo powerUpInfo);
    }
}