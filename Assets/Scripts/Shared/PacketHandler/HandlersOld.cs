//  void HandleSpawnPlayerMessage(string message)
//         {
//             string[] parts = message.Split('|');
//             if (parts.Length > 2)
//             {
//                 string playerId = parts[1];
//                 string[] positionParts = parts[2].Split(',');
//                 Vector3 spawnPosition = new Vector3(
//                     float.Parse(positionParts[0]),
//                     float.Parse(positionParts[1]),
//                     float.Parse(positionParts[2])
//                 );

//                 // Check if this player already exists
//                 if (!_otherPlayers.ContainsKey(playerId))
//                 {
//                     // Spawn the player using PlayerSpawner if available
//                     MainThreadDispatcher.RunOnMainThread(() =>
//                     {
//                         GameObject newPlayer;
//                         if (playerSpawner != null)
//                         {
//                             newPlayer = playerSpawner.SpawnNetworkPlayer(playerId, spawnPosition);
//                         }
//                         else
//                         {
//                             newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
//                         }
//                         _otherPlayers[playerId] = newPlayer;
//                         Debug.Log($"Spawned player {playerId} at position {spawnPosition}");
//                     });
//                 }
//             }
//         }

//         void HandlePlayerPositionsMessage(string message)
//         {
//             string[] playerData = message.Substring("PlayerPositions|".Length).Split(';');
//             foreach (var data in playerData)
//             {
//                 if (string.IsNullOrWhiteSpace(data)) continue;

//                 string[] parts = data.Split(',');
//                 string playerId = parts[0];
//                 Vector3 position = new Vector3(
//                     float.Parse(parts[1]),
//                     float.Parse(parts[2]),
//                     float.Parse(parts[3])
//                 );

//                 // Update the player's position using PlayerSpawner if available
//                 if (_otherPlayers.ContainsKey(playerId))
//                 {
//                     MainThreadDispatcher.RunOnMainThread(() =>
//                     {
//                         if (playerSpawner != null)
//                         {
//                             playerSpawner.UpdatePlayerPosition(playerId, position);
//                         }
//                         else
//                         {
//                             _otherPlayers[playerId].transform.position = position;
//                         }
//                     });
//                 }
//             }
//         }

//         // Handle player removal message from the server
//         void HandleRemovePlayerMessage(string message)
//         {
//             var parts = message.Split('|');
//             if (parts.Length > 1)
//             {
//                 var playerId = parts[1];

//                 // Remove the player from the scene using PlayerSpawner if available
//                 if (_otherPlayers.ContainsKey(playerId))
//                 {
//                     MainThreadDispatcher.RunOnMainThread(() =>
//                     {
//                         if (playerSpawner != null)
//                         {
//                             playerSpawner.RemoveNetworkPlayer(playerId);
//                         }
//                         else
//                         {
//                             Destroy(_otherPlayers[playerId]);
//                         }
//                         _otherPlayers.Remove(playerId);
//                         Debug.Log($"Removed player {playerId} from the scene.");
//                     });
//                 }
//             }
//         }

        /*
         FROM SERVER:
        void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            string playerId = Guid.NewGuid().ToString(); // Generate a unique ID for the player
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0);

            lock (_players)
            {
                _players[playerId] = new PlayerData { id = playerId, position = spawnPosition };
            }

            // Notify all clients about the new player
            string spawnMessage = $"SpawnPlayer|{playerId}|{spawnPosition.x},{spawnPosition.y},{spawnPosition.z}";
            BroadcastData(spawnMessage);

            try
            {
                while (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Received from client: " + message);

                    // Handle player movement updates
                    if (message.StartsWith("MovePlayer"))
                    {
                        HandleMovePlayerMessage(playerId, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error handling client: " + ex.Message);
            }
            finally
            {
                // lock (_connectedClients)
                // {
                //     _connectedClients.Remove(client);
                // }

                lock (_players)
                {
                    _players.Remove(playerId);
                }

                client.Close();
                Debug.Log($"Client disconnected. Removed player {playerId}.");
            }
        }

        void HandleMovePlayerMessage(string playerId, string message)
        {
            string[] parts = message.Split('|');
            if (parts.Length > 1)
            {
                string[] positionParts = parts[1].Split(',');
                Vector3 newPosition = new Vector3(
                    float.Parse(positionParts[0]),
                    float.Parse(positionParts[1]),
                    float.Parse(positionParts[2])
                );

                lock (_players)
                {
                    if (_players.ContainsKey(playerId))
                    {
                        _players[playerId].position = newPosition;
                    }
                }

                // Broadcast the updated position to all clients
                string updateMessage = $"UpdatePlayer|{playerId}|{newPosition.x},{newPosition.y},{newPosition.z}";
                BroadcastData(updateMessage);
            }
        }
        */
