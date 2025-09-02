using System;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class StripedPowerUpController : MonoBehaviour
    {
        [field: SerializeField] private Button _button;

        private StripedPowerUp _powerUp;
        private PowerUpInfo _info;
        public event Action<PowerUp> OnStripedPowerUp;
        
        private void Awake()
        {
            _powerUp = new StripedPowerUp(true);
            _button.onClick.AddListener(Activate);
        }

        private void Activate()
        {
           OnStripedPowerUp?.Invoke(_powerUp);
        }
    }
}