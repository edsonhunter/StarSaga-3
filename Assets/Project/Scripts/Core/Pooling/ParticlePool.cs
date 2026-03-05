using System.Collections.Generic;
using StarSaga3.Project.Script.Views;
using UnityEngine;

namespace StarSaga3.Project.Script.Core.Pooling
{
    public class ParticlePool : MonoBehaviour
    {
        [SerializeField] private ParticleEffect _particlePrefab;
        [SerializeField] private int _initialPoolSize = 20;

        private readonly Queue<ParticleEffect> _pool = new Queue<ParticleEffect>();

        private void Start()
        {
            if (_particlePrefab == null)
            {
                Debug.LogWarning("ParticlePool: Missing Particle Prefab!");
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewParticle();
            }
        }

        private ParticleEffect CreateNewParticle()
        {
            var particle = Instantiate(_particlePrefab, transform);
            particle.gameObject.SetActive(false);
            _pool.Enqueue(particle);
            return particle;
        }

        public void PlayEffect(Vector3 position)
        {
            if (_particlePrefab == null) return;

            ParticleEffect effect = _pool.Count > 0 ? _pool.Dequeue() : CreateNewParticle();
            
            effect.Play(position, ReturnToPool);
        }

        private void ReturnToPool(ParticleEffect effect)
        {
            _pool.Enqueue(effect);
        }
    }
}
