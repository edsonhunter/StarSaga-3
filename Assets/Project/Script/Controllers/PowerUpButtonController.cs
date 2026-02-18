using System;
using StarSaga3.Project.Script.Core.PowerUp;
using StarSaga3.Project.Script.Models;
using UnityEngine;
using UnityEngine.UI;

namespace StarSaga3.Project.Script.Controllers
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