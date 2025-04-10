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
        private bool isServer;

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
            capturePoint = FindObjectOfType<CapturePoint>();
            if (capturePoint == null)
            {
                Debug.LogError("No CapturePoint found in the scene!");
                return;
            }

            // Determine if this is server or client
            isServer = GameServer.Instance != null;

            if (isServer)
            {
                // Server starts the game
                StartGame();
            }
            else
            {
                // Client waits for server to start the game
                GameClient.Instance.RegisterPacketHandler(new GameStateHandler(this));
            }
        }

        public void StartGame()
        {
            if (!isServer) return; // Only server can start the game

            currentTime = gameDuration;
            isGameRunning = true;
            if (endGamePanel != null) endGamePanel.SetActive(false);
            if (winnerText != null) winnerText.gameObject.SetActive(false);

            // Broadcast game start to all clients
            var startGamePacket = new StartGamePacket();
            GameServer.Instance.BroadcastData(startGamePacket);

            StartCoroutine(GameTimer());
        }

        private IEnumerator GameTimer()
        {
            while (currentTime > 0 && isGameRunning)
            {
                currentTime -= Time.deltaTime;
                
                if (isServer)
                {
                    // Server broadcasts time updates
                    var timeUpdatePacket = new GameTimeUpdatePacket(currentTime);
                    GameServer.Instance.BroadcastData(timeUpdatePacket);
                }
                
                UpdateTimerDisplay();
                yield return null;
            }

            if (isGameRunning && isServer)
            {
                // Server determines game end
                var endGamePacket = new EndGamePacket();
                GameServer.Instance.BroadcastData(endGamePacket);
                EndGame();
            }
        }

        public void UpdateGameTime(float newTime)
        {
            if (!isServer) // Only clients update time from server
            {
                currentTime = newTime;
                UpdateTimerDisplay();
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

        public void EndGame()
        {
            isGameRunning = false;
            
            if (isServer)
            {
                // Server determines winner
                winner = capturePoint.GetControllingPlayer();
                
                // Broadcast winner to all clients
                var winnerPacket = new GameWinnerPacket(winner != null ? winner.name : "No Winner");
                GameServer.Instance.BroadcastData(winnerPacket);
            }

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
            if (!isServer) return; // Only server can restart the game

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