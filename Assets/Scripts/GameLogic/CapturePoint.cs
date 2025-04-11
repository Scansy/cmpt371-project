using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using Server;
using Shared.Packet;

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
        public GameObject ControllingPlayer
        {
            get => ControllingPlayer;
            set => ControllingPlayer = value;
        }
        public bool IsCaptured { get; set; } = false;
        private HashSet<GameObject> playersInZone = new HashSet<GameObject>();
        private Color originalColor;
        public SpriteRenderer spriteRenderer { get; set; }
        private bool stateChanged = false;
        private bool progressChanged = false;
        private Color currentColor;
    
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
            if (IsCaptured && playersInZone.Contains(ControllingPlayer))
            {
                captureProgress = captureTime; // Keep progress at max
            }
            // If point is captured and a different player is in zone, decrease progress
            else if (IsCaptured && playerInZone && !playersInZone.Contains(ControllingPlayer))
            {
                captureProgress -= Time.deltaTime;
                if (captureProgress <= 0)
                {
                    // When progress reaches 0, the new player starts capturing
                    IsCaptured = false;
                    ControllingPlayer = playersInZone.First();
                    UpdatePointColor();
                    Debug.Log($"Point contested by {ControllingPlayer.name}!");
                }
            }
            // If point is not captured and exactly one player is in zone, increase progress
            else if (!IsCaptured && playersInZone.Count == 1)
            {
                captureProgress += Time.deltaTime;
                if (captureProgress >= captureTime)
                {
                    captureProgress = captureTime;
                    IsCaptured = true;
                    ControllingPlayer = playersInZone.First();
                    UpdatePointColor();
                    Debug.Log($"Point Captured by {ControllingPlayer.name}!");
                }
            }
            // If point is captured and no players are in zone, maintain capture
            else if (IsCaptured && !playerInZone)
            {
                captureProgress = captureTime; // Keep progress at max
            }
            // If point is not captured and no players are in zone, maintain current progress
            else if (!IsCaptured && !playerInZone)
            {
                // Progress stays the same
            }
            // If multiple players are in zone, progress stays the same (paused)

            float previousProgress = captureProgress;
            captureProgress = Mathf.Clamp(captureProgress, 0f, captureTime);
            progressChanged = Mathf.Abs(previousProgress - captureProgress) > 0.001f;

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
                if (IsCaptured && ControllingPlayer != null)
                {
                    Color playerColor = GetPlayerColor(ControllingPlayer);
                    if (spriteRenderer.color != playerColor)
                    {
                        spriteRenderer.color = playerColor;
                        currentColor = playerColor;
                        stateChanged = true;
                    }
                }
                else if (spriteRenderer.color != originalColor)
                {
                    spriteRenderer.color = originalColor;
                    currentColor = originalColor;
                    stateChanged = true;
                }
            }
        }

        // Returns the player currently controlling the capture point
        public GameObject GetControllingPlayer()
        {
            return ControllingPlayer;
        }

        // Resets the capture point to its initial state
        public void ResetPoint()
        {
            captureProgress = 0f;
            IsCaptured = false;
            ControllingPlayer = null;
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

            string controllerId = ControllingPlayer != null ? 
                ControllingPlayer.GetComponent<PlayerControl>().client.getPlayerId() : "";

            var updatePacket = new CapturePointUpdatePacket(
                captureProgress / captureTime,
                IsCaptured,
                controllerId,
                currentColor
            );

            server.BroadcastData(updatePacket);
        }
    }

   
}
