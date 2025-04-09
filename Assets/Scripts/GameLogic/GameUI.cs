using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class GameUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text timerText;
        public Text winnerText;
        public GameObject endGamePanel;
        public Button restartButton;
        public Button menuButton;

        private void Start()
        {
            // Set up button listeners
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
            }

            // Hide end game UI initially
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(false);
            }

            if (winnerText != null)
            {
                winnerText.gameObject.SetActive(false);
            }
        }
    }
} 