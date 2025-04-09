using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class CapturePoint : MonoBehaviour
    {
        public Slider captureBar;
        public float captureTime = 5f;
    
        // Colors for each player
        private readonly Color PLAYER1_COLOR = new Color(1f, 0f, 0f, 1f);    // Red
        private readonly Color PLAYER2_COLOR = new Color(0f, 0f, 1f, 1f);    // Blue
        private readonly Color PLAYER3_COLOR = new Color(0f, 1f, 0f, 1f);    // Green
    
        private float captureProgress = 0f;
        private bool playerInZone = false;
        private GameObject controllingPlayer = null;
        private bool isCaptured = false;
        private HashSet<GameObject> playersInZone = new HashSet<GameObject>();
        private Color originalColor;
        private SpriteRenderer spriteRenderer;
    
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        void Update()
        {
            playerInZone = playersInZone.Count > 0;

            // If point is captured and controlling player is in zone, maintain capture
            if (isCaptured && playersInZone.Contains(controllingPlayer))
            {
                captureProgress = captureTime; // Keep progress at max
            }
            // If point is captured and a different player is in zone, reset progress
            else if (isCaptured && playerInZone && !playersInZone.Contains(controllingPlayer))
            {
                captureProgress = 0; // Reset progress for new player
                isCaptured = false; // Point is now contested
                controllingPlayer = null;
                UpdatePointColor();
            }
            // If point is not captured and exactly one player is in zone, increase progress
            else if (!isCaptured && playersInZone.Count == 1)
            {
                captureProgress += Time.deltaTime;
                if (captureProgress >= captureTime)
                {
                    captureProgress = captureTime;
                    isCaptured = true;
                    controllingPlayer = playersInZone.First();
                    UpdatePointColor();
                    Debug.Log($"Point Captured by {controllingPlayer.name}!");
                }
            }
            // If point is captured and no players are in zone, maintain capture
            else if (isCaptured && !playerInZone)
            {
                captureProgress = captureTime; // Keep progress at max
            }
            // If point is not captured and no players are in zone, maintain neutral state
            else if (!isCaptured && !playerInZone)
            {
                captureProgress = 0; // Keep progress at zero
            }
            // If multiple players are in zone, progress stays the same (paused)

            captureBar.value = captureProgress / captureTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playersInZone.Add(other.gameObject);
                Debug.Log($"Player {other.gameObject.name} entered. Total players: {playersInZone.Count}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playersInZone.Remove(other.gameObject);
                Debug.Log($"Player {other.gameObject.name} left. Total players: {playersInZone.Count}");
            }
        }

        private Color GetPlayerColor(GameObject player)
        {
            // Get player number from the name (assumes names like "Player1", "Player2", "Player3")
            string playerName = player.name.ToLower();
        
            if (playerName.Contains("1") || playerName.Contains("one"))
                return PLAYER1_COLOR;
            if (playerName.Contains("2") || playerName.Contains("two"))
                return PLAYER2_COLOR;
            if (playerName.Contains("3") || playerName.Contains("three"))
                return PLAYER3_COLOR;
            
            return Color.white; // Default color if player number can't be determined
        }

        private void UpdatePointColor()
        {
            if (spriteRenderer != null)
            {
                if (isCaptured && controllingPlayer != null)
                {
                    Color playerColor = GetPlayerColor(controllingPlayer);
                    spriteRenderer.color = playerColor;
                }
                else
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        // Returns the player currently controlling the capture point
        public GameObject GetControllingPlayer()
        {
            return controllingPlayer;
        }

        // Resets the capture point to its initial state
        public void ResetPoint()
        {
            captureProgress = 0f;
            isCaptured = false;
            controllingPlayer = null;
            playersInZone.Clear();
            UpdatePointColor();
            if (captureBar != null)
            {
                captureBar.value = 0f;
            }
        }
    }
}
