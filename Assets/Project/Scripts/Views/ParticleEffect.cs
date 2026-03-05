using UnityEngine;

namespace StarSaga3.Project.Script.Views
{
    public class ParticleEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        private System.Action<ParticleEffect> _onComplete;
        private bool _isPlaying;

        private void Awake()
        {
            if (_particleSystem == null)
                _particleSystem = GetComponent<ParticleSystem>();
        }

        public void Play(Vector3 position, System.Action<ParticleEffect> onComplete)
        {
            transform.position = position;
            _onComplete = onComplete;
            _isPlaying = true;
            gameObject.SetActive(true);
            _particleSystem.Play(true);
        }

        private void Update()
        {
            if (_isPlaying && !_particleSystem.IsAlive(true))
            {
                _isPlaying = false;
                gameObject.SetActive(false);
                _onComplete?.Invoke(this);
            }
        }
    }
}
