using System;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class PowerUpButtonController : MonoBehaviour
    {
        [field: SerializeField] private Button _button;

        private PowerUp _powerUp;
        private PowerUpInfo _info;
        public event Action<PowerUp> OnPressed;
        
        public void Initialize(PowerUp powerUp)
        {
            _powerUp = powerUp;
            _button.onClick.AddListener(Activate);
        }

        private void Activate()
        {
           OnPressed?.Invoke(_powerUp);
        }
    }
}