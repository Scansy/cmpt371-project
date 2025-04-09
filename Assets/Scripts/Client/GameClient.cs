using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Server;
using Shared;
using Shared.Packet;
using Shared.PacketHandler;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client
{
    public class GameClient : MonoBehaviour
    {
        private Dictionary<string, GameObject> _otherPlayers = new Dictionary<string, GameObject>();
        private TcpClient _client;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private bool _isRunning = true;
        
        private Thread _receiveThread;
        private NetworkStream _stream;
        public readonly Dictionary<Type, IPacketHandler> PacketHandlers = new Dictionary<Type, IPacketHandler>();
        
        private Thread _sendThread;
        private ConcurrentQueue<IPacket> _packetQueue = new ConcurrentQueue<IPacket>(new Queue<IPacket>());
        
        public string serverIP = "10.0.0.25"; 
        public int serverPort = GameServer.DEFAULT_PORT;
        public GameObject playerPrefab;

        private PlayerSpawner playerSpawner;
        private string localPlayerId;

        void Start()
        {
            playerSpawner = PlayerSpawner.Instance;
            if (playerSpawner == null)
            {
                Debug.LogError("PlayerSpawner not found in scene!");
                return;
            }
            
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                Debug.Log("Attempting to connect to server at " + serverIP + ":" + serverPort);
                _client = new TcpClient(serverIP, serverPort);
                _stream = _client.GetStream();
                
                InitReceiveThread();
                InitSendThread();

                // Notify the server that a new client has joined
                SendMessageToServer("ClientConnected");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to connect to server: " + e.Message);
            }
        }

        private void InitReceiveThread()
        {
            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            Debug.Log("Connected to server!");
        }
        
        private void ReceiveMessage()
        {
            while (_isRunning)
            {
                var packet = (IPacket)_formatter.Deserialize(_stream);
                HandlePacket(packet);
            }
        }

        private void HandlePacket(IPacket packet)
        {
            PacketHandlers[packet.GetType()].HandlePacket(packet);
        }

        private void InitSendThread()
        {
            _sendThread = new Thread(SendMessage);
            _sendThread.Start();
        }

        private void SendMessage()
        {
            while (_isRunning)
            {
                bool dequeued;
                IPacket packet;

                lock (_packetQueue)
                {
                    dequeued = _packetQueue.TryDequeue(out packet);
                }

                if (dequeued)
                {
                    _formatter.Serialize(_stream, packet);
                }
                Thread.Sleep(1); // make it not run too many resources
            }
        }

        void SendMessageToServer(string message)
        {
            if (_stream != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                Debug.Log("Sent: " + message);
            }
        }
        
        void HandleSpawnPlayerMessage(string message)
        {
            string[] parts = message.Split('|');
            if (parts.Length > 2)
            {
                string playerId = parts[1];
                string[] positionParts = parts[2].Split(',');
                Vector3 spawnPosition = new Vector3(
                    float.Parse(positionParts[0]),
                    float.Parse(positionParts[1]),
                    float.Parse(positionParts[2])
                );

                // Check if this player already exists
                if (!_otherPlayers.ContainsKey(playerId))
                {
                    // Spawn the player using PlayerSpawner if available
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        GameObject newPlayer;
                        if (playerSpawner != null)
                        {
                            newPlayer = playerSpawner.SpawnNetworkPlayer(playerId, spawnPosition);
                        }
                        else
                        {
                            newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                        }
                        _otherPlayers[playerId] = newPlayer;
                        Debug.Log($"Spawned player {playerId} at position {spawnPosition}");
                    });
                }
            }
        }

        void HandlePlayerPositionsMessage(string message)
        {
            string[] playerData = message.Substring("PlayerPositions|".Length).Split(';');
            foreach (var data in playerData)
            {
                if (string.IsNullOrWhiteSpace(data)) continue;

                string[] parts = data.Split(',');
                string playerId = parts[0];
                Vector3 position = new Vector3(
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                );

                // Update the player's position using PlayerSpawner if available
                if (_otherPlayers.ContainsKey(playerId))
                {
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        if (playerSpawner != null)
                        {
                            playerSpawner.UpdatePlayerPosition(playerId, position);
                        }
                        else
                        {
                            _otherPlayers[playerId].transform.position = position;
                        }
                    });
                }
            }
        }

        // Handle player removal message from the server
        void HandleRemovePlayerMessage(string message)
        {
            var parts = message.Split('|');
            if (parts.Length > 1)
            {
                var playerId = parts[1];

                // Remove the player from the scene using PlayerSpawner if available
                if (_otherPlayers.ContainsKey(playerId))
                {
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        if (playerSpawner != null)
                        {
                            playerSpawner.RemoveNetworkPlayer(playerId);
                        }
                        else
                        {
                            Destroy(_otherPlayers[playerId]);
                        }
                        _otherPlayers.Remove(playerId);
                        Debug.Log($"Removed player {playerId} from the scene.");
                    });
                }
            }
        }
        // TODO: move to HandlePacket of each packet handler
        // void ReceiveMessages()
        // {
        //     byte[] buffer = new byte[1024];
        //
        //     while (_isRunning)
        //     {
        //         try
        //         {
        //             int bytesRead = _stream.Read(buffer, 0, buffer.Length);
        //             if (bytesRead == 0) break;
        //
        //             string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //             Debug.Log("Received from server: " + message);
        //
        //             // Handle server messages
        //             if (message.StartsWith("SpawnPlayer"))
        //             {
        //                 HandleSpawnPlayerMessage(message);
        //             }
        //             else if (message.StartsWith("PlayerPositions"))
        //             {
        //                 HandlePlayerPositionsMessage(message);
        //             }
        //             else if (message == "StartGame")
        //             {
        //                 // Transition to the main game scene
        //                 SceneManager.LoadScene("Game");
        //
        //                 // Spawn the player GameObject in the main game scene
        //                 SceneManager.sceneLoaded += OnGameSceneLoaded;
        //             }
        //         }
        //         catch (Exception e)
        //         {
        //             Debug.LogError("Error receiving data: " + e.Message);
        //             break;
        //         }
        //     }
        // }
        void SpawnPlayer(Vector3 position)
        {
            if (playerPrefab != null)
            {
                // Enqueue the Instantiate call to the main thread
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    if (playerSpawner != null)
                    {
                        playerSpawner.SpawnNetworkPlayer(localPlayerId, position);
                    }
                    else
                    {
                        Instantiate(playerPrefab, position, Quaternion.identity);
                    }
                    Debug.Log("Player spawned at position: " + position);
                });
            }
            else
            {
                Debug.LogError("Player prefab is not assigned in the Inspector.");
            }
        }

        void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Spawn the player GameObject
            if (scene.name == "Game" && playerPrefab != null)
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    if (playerSpawner != null)
                    {
                        playerSpawner.SpawnNetworkPlayer(localPlayerId, Vector3.zero);
                    }
                    else
                    {
                        Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                    }
                    Debug.Log("Player spawned in the game scene.");
                });
            }

            // Unsubscribe from the event to avoid duplicate calls
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
        }

        public void GoBackToMenu()
        {
            Debug.Log("Returning to Main Menu...");

            // Clean up the client connection
            if (_client != null)
            {
                _client.Close();
            }
            if (_stream != null)
            {
                _stream.Close();
            }
            if (_receiveThread != null)
            {
                _receiveThread.Abort();
            }

            // Load the Main Menu scene
            SceneManager.LoadScene("Main Menu");
        }

        void OnApplicationQuit()
        {
            _isRunning = false;
            _receiveThread?.Join();
            _client?.Close();
            _stream?.Close();
        }
    }
}