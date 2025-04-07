using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Shared.Packet;
using Shared.PacketHandler;
using UnityEngine;

namespace Server
{
    public class GameServer : MonoBehaviour
    {
        private TcpListener _server;
        private Thread _serverThread;
        private bool _isRunning = false;

        // private List<TcpClient> _connectedClients = new List<TcpClient>();
        public readonly Dictionary<int, ServerSideClient> ServerSideClients = new Dictionary<int, ServerSideClient>();
        private Dictionary<string, PlayerData> _players = new Dictionary<string, PlayerData>(); // Track players by ID
        public readonly Dictionary<Type, IPacketHandler> PacketHandlers = new Dictionary<Type, IPacketHandler>(); // maps packet class to handler
            
        public const short DEFAULT_PORT = 7777;
        
        public void HandlePacket(IDisposable packet)
        {
            PacketHandlers[packet.GetType()].HandlePacket(packet);
        }
    
        void Start()
        {
            _serverThread = new Thread(StartServer);
            _serverThread.IsBackground = true;
            InitializePacketHandlers();
            _serverThread.Start();
        }

        private void InitializePacketHandlers()
        {
            PacketHandlers.Add(typeof(WelcomePacket), new WelcomeHandler());
        }

        private void StartServer()
        {
            try
            {
                _server = new TcpListener(IPAddress.Any, 7636);
                _server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // Allow address reuse
                _server.Start();
                _isRunning = true;
                Debug.Log("Server started on port 7636...");

                while (_isRunning)
                {
                    SendGameState(); // Send game state to all clients every frame
                    Thread.Sleep(1); // Adjust the sleep time as needed for performance
                    Debug.Log("Waiting for a client to connect...");
                    
                    // TODO: everytime new client tries to connect, create new ServerSideClient
                        
                        
                    
                    // catch (SocketException ex)
                    // {
                    //     Debug.LogError("Socket exception: " + ex.Message);
                    // }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to start server: " + ex.Message);
            }
        }
    
        
        void BroadcastData(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            // lock (_connectedClients)
            // {
            //     for (int i = _connectedClients.Count - 1; i >= 0; i--)
            //     {
            //         try
            //         {
            //             NetworkStream stream = _connectedClients[i].GetStream();
            //             stream.Write(data, 0, data.Length);
            //         }
            //         catch (Exception ex)
            //         {
            //             Debug.LogError("Error sending data to client: " + ex.Message);
            //             _connectedClients.RemoveAt(i); // Remove disconnected client
            //         }
            //     }
            // }
        }

        void SendGameState()
        {
            string gameState = "GameState|";
            lock (_players)
            {
                foreach (var player in _players.Values)
                {
                    gameState += $"{player.id},{player.position.x},{player.position.y},{player.position.z};";
                }
            }

            BroadcastData(gameState);
        }

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

        void OnApplicationQuit()
        {
            _isRunning = false;

            try
            {
                _server?.Stop();
                _serverThread?.Join(); // Wait for the server thread to finish
                Debug.Log("Server stopped.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error stopping server: " + ex.Message);
            }
        }
    }

    public class PlayerData
    {
        public string id; // Unique identifier for the player
        public Vector3 position; // Player's position
    }
}