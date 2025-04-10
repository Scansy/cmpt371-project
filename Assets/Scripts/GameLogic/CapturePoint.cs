using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    
        [Header("Network Settings")]
        public bool isServer = false; // Set to true on the server instance
        private GameServer server;
    
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
            if (!isServer) return; // Only run capture logic on server
            playerInZone = playersInZone.Count > 0;

            // If point is captured and controlling player is in zone, maintain capture
            if (isCaptured && playersInZone.Contains(controllingPlayer))
            {
                captureProgress = captureTime; // Keep progress at max
            }
            // If point is captured and a different player is in zone, decrease progress
            else if (isCaptured && playerInZone && !playersInZone.Contains(controllingPlayer))
            {
                captureProgress -= Time.deltaTime;
                if (captureProgress <= 0)
                {
                    // When progress reaches 0, the new player starts capturing
                    isCaptured = false;
                    controllingPlayer = playersInZone.First();
                    UpdatePointColor();
                    Debug.Log($"Point contested by {controllingPlayer.name}!");
                }
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
            // If point is not captured and no players are in zone, maintain current progress
            else if (!isCaptured && !playerInZone)
            {
                // Progress stays the same
            }
            // If multiple players are in zone, progress stays the same (paused)

            captureBar.value = captureProgress / captureTime;

            // Broadcast state changes
            if (stateChanged || progressChanged)
            {
                BroadcastCapturePointState();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (isServer)
            {
                playersInZone.Add(other.gameObject);
                // State change will be broadcast in Update
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

        private void BroadcastCapturePointState()
        {
            if (!isServer || server == null) return;

            string controllerId = controllingPlayer != null ? 
                controllingPlayer.GetComponent<PlayerControl>().client.getPlayerId() : "";

            var updatePacket = new CapturePointUpdatePacket(
                captureProgress / captureTime,
                isCaptured,
                controllerId,
                currentColor
            );

            server.BroadcastData(updatePacket);
        }
    }

   
}
