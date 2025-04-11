using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Shared.Packet;
using Server;

namespace GameLogic
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public float gameDuration = 300f; // 5 minutes in seconds
        public Text timerText;
        public Text winnerText;
        public GameObject endGamePanel;

        private float currentTime;
        private bool isGameRunning = false;
        private CapturePoint capturePoint;
        private GameObject winner;
        private RoleManager roleManager;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            roleManager = FindObjectOfType<RoleManager>();
            if (roleManager == null)
            {
                Debug.LogError("RoleManager not found in scene!");
                return;
            }

            capturePoint = FindObjectOfType<CapturePoint>();
            if (capturePoint == null)
            {
                Debug.LogError("No CapturePoint found in the scene!");
                return;
            }
            
            // Start game on the server side
            if (roleManager.isServer)
            {
                StartGame();
            }
        }

        public void StartGame()
        {
            currentTime = gameDuration;
            isGameRunning = true;
            if (endGamePanel != null) endGamePanel.SetActive(false);
            if (winnerText != null) winnerText.gameObject.SetActive(false);
            StartCoroutine(GameTimer());
        }

        // Game timer will NOT be updated on the client side, since client side also has its own GameManager
        // that times the game on the client side.
        private IEnumerator GameTimer()
        {
            while (currentTime > 0 && isGameRunning)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
                yield return null;
            }

            if (isGameRunning)
            {
                EndGame();
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        private void EndGame()
        {
            isGameRunning = false;
            
            // Get the winner (player controlling the point at game end)
            winner = capturePoint.GetControllingPlayer();
            
            // Display winner
            if (winner != null && winnerText != null)
            {
                winnerText.text = $"{winner.name} Wins!";
                winnerText.gameObject.SetActive(true);
            }
            else if (winnerText != null)
            {
                winnerText.text = "No Winner!";
                winnerText.gameObject.SetActive(true);
            }

            // Show end game panel
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(true);
            }

            // Disable player controls
            var players = FindObjectsOfType<PlayerControl>();
            foreach (var player in players)
            {
                player.enabled = false;
            }
        }

        public void RestartGame()
        {
            // Reset game state
            if (capturePoint != null)
            {
                capturePoint.ResetPoint();
            }

            // Enable player controls
            var players = FindObjectsOfType<PlayerControl>();
            foreach (var player in players)
            {
                player.enabled = true;
            }

            // Start new game
            StartGame();
        }

        public void ReturnToMenu()
        {
            // Clean up and return to main menu
            Destroy(gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
} 