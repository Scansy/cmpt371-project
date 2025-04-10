using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public class PlayerSpawner : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Vector2[] spawnPoints = new Vector2[3] {
            new Vector2(-5, 0),    // Player 1 spawn
            new Vector2(0, 0),     // Player 2 spawn
            new Vector2(5, 0)      // Player 3 spawn
        };
        public int playerCount; // Number of players in the game, set by the server once game starts

        // Track spawned players
        private Dictionary<string, GameObject> spawnedPlayers = new Dictionary<string, GameObject>();

        // Singleton instance
        public static PlayerSpawner Instance { get; private set; }

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
            SpawnPlayers();
        }

        public void SpawnPlayers()
        {
            for (int i = 0; i < playerCount; i++)
            {
                SpawnPlayer(i + 1);
            }
        }

        public GameObject SpawnPlayer(int playerNumber)
        {
            if (playerNumber < 1 || playerNumber > 3)
            {
                Debug.LogError("Player number must be between 1 and 3");
                return null;
            }

            // Get spawn position 
            Vector2 spawnPos = spawnPoints[playerNumber - 1];
        
            // Instantiate the player
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        
            // Name the player
            player.name = $"Player{playerNumber}";
        
            return player;
        }

        public GameObject SpawnPlayerAt(int playerNumber, Vector2 position)
        {
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.name = $"Player{playerNumber}";
            return player;
        }

        // Spawn a network player with a specific ID
        public GameObject SpawnNetworkPlayer(string playerId, Vector2 position)
        {
            // If player already exists, return it
            if (spawnedPlayers.TryGetValue(playerId, out GameObject existingPlayer))
            {
                return existingPlayer;
            }

            // Create new player
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.name = $"Player_{playerId}";

            // Store in our dictionary
            spawnedPlayers[playerId] = player;

            return player;
        }

        // Remove a network player
        public void RemoveNetworkPlayer(string playerId)
        {
            if (spawnedPlayers.TryGetValue(playerId, out GameObject player))
            {
                Destroy(player);
                spawnedPlayers.Remove(playerId);
            }
        }

        // Update a player's position (for network movement)
        public void UpdatePlayerPosition(string playerId, Vector2 newPosition)
        {
            if (spawnedPlayers.TryGetValue(playerId, out GameObject player))
            {
                player.transform.position = newPosition;
            }
        }

        // Get a player by ID
        public GameObject GetPlayer(string playerId)
        {
            spawnedPlayers.TryGetValue(playerId, out GameObject player);
            return player;
        }

        // Check if a player exists
        public bool HasPlayer(string playerId)
        {
            return spawnedPlayers.ContainsKey(playerId);
        }

        // Get all spawned players
        public Dictionary<string, GameObject> GetAllPlayers()
        {
            return spawnedPlayers;
        }

        // Clear all spawned players
        public void ClearAllPlayers()
        {
            foreach (var player in spawnedPlayers.Values)
            {
                if (player != null)
                {
                    Destroy(player);
                }
            }
            spawnedPlayers.Clear();
        }
    }
} 