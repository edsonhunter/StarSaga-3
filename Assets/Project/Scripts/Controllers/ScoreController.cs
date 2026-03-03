using DG.Tweening;
using TMPro;
using UnityEngine;

namespace StarSaga3.Project.Script.Controllers
{
    public class ScoreController : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI scoreText;
        [field: SerializeField] private TextMeshProUGUI scoreToAdd;
        private int _currentScore;
        
        public Tween AddScore(int value)
        {
            Sequence  sequence = DOTween.Sequence();
            if (value <= 0)
            {
                return sequence;
            }

            sequence.AppendCallback(() => 
            {
                _currentScore += value;
                UpdateScore();
            });
            return sequence;
        }

        private void UpdateScore()
        {
            scoreText.text = $"Score: {_currentScore.ToString()}";
        }
    }
}