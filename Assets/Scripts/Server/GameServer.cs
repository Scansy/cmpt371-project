using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameLogic;
using Shared.Packet;
using Shared.PacketHandler;
using UnityEngine;
using Shared;

namespace Server
{
    public class GameServer : MonoBehaviour
    {
        private static GameServer _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static GameServer Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<GameServer>();
                        
                        if (_instance == null)
                        {
                            GameObject go = new GameObject("GameServer");
                            _instance = go.AddComponent<GameServer>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    return _instance;
                }
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            _applicationIsQuitting = true;
        }

        private TcpListener _server;
        private Thread _serverThread;
        private bool _isRunning = false;

        public readonly Dictionary<int, ServerSideClient> ServerSideClients = new Dictionary<int, ServerSideClient>();
        public Dictionary<string, PlayerData> _players = new Dictionary<string, PlayerData>(); // Track players by ID
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
            PacketHandlers.Add(typeof(TestPacket), new TestHandler());
            PacketHandlers.Add(typeof(SpawnPlayerPacket), new SpawnHandler());
            PacketHandlers.Add(typeof(UpdatePosServerPacket), new UpdatePosServerHandler());
            PacketHandlers.Add(typeof(UpdatePosClientPacket), new UpdatePosClientHandler());
            PacketHandlers.Add(typeof(PlayerMovementPacket), new PlayerMovementHandler());
        }

        private int clientIdCounter = 1; // Counter for generating unique client IDs

        void HandleNewClient(TcpClient client)
        {

            Debug.Log("Handling new client connection..." + clientIdCounter);
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Debug.Log("Running on main thread...");
                string playerId = clientIdCounter.ToString(); // Generate unique ID
                Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0);

                // Create a temporary GameObject to generate a Transform
                GameObject tempPlayerObject = new GameObject("TempPlayer_" + playerId);
                tempPlayerObject.transform.position = spawnPosition;
                tempPlayerObject.transform.rotation = Quaternion.identity;

                lock (_players)
                {
                    _players[playerId] = new PlayerData
                    {
                        id = playerId,
                        position = spawnPosition,
                        rotation = tempPlayerObject.transform.rotation
                    };
                    clientIdCounter++;
                    Debug.Log("New player added: " + playerId);
                }

                // Use the Transform of the temporary GameObject for the SpawnPlayerPacket
                var spawnPacket = new SpawnPlayerPacket(spawnPosition, tempPlayerObject.transform);
                BroadcastData(spawnPacket);

                foreach (var player in _players.Values)
                {
                    var existingPlayerPacket = new SpawnPlayerPacket(player.position, tempPlayerObject.transform);
                    ServerSideClients[ServerSideClients.Count].SendMessage(existingPlayerPacket);
                    Debug.Log("Sent existing player data to new client: " + player.id);
                }

                // Clean up the temporary GameObject
                Destroy(tempPlayerObject);
            });
        }

        private void StartServer()
        {
            try
            {
                _server = new TcpListener(IPAddress.Any, 7777);
                _server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // Allow address reuse
                _server.Start();
                _isRunning = true;
                Debug.Log("Server started on port 7777...");

                while (_isRunning)
                {
                    // UpdatePlayerPositions(); // Send game state to all clients every frame
                    Thread.Sleep(1); // Adjust the sleep time as needed for performance
                    
                    if (_server.Pending()) // Check if a client is waiting to connect
                    {
                        TcpClient client = _server.AcceptTcpClient();
                        Debug.Log("Client connected!");

                        ServerSideClient serverSideClient = new ServerSideClient(this, client);

                        lock (ServerSideClients)
                        {
                            int clientId = ServerSideClients.Count + 1; // Generate a unique ID for the client
                            ServerSideClients[clientId] = serverSideClient;
                            Debug.Log("Client ID: " + clientId + "Added to server side clients.");
                        }

                        Thread clientThread = new Thread(serverSideClient.ReceiveMessage);
                        clientThread.IsBackground = true;
                        clientThread.Start();
                        Debug.Log("Client thread started!");
                        HandleNewClient(client); // Handle the new client
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to start server: " + ex.Message);
            }
        }
        
        public void BroadcastData(IDisposable packet)
        {
            lock (ServerSideClients)
            {
                List<int> disconnectedClients = new List<int>();
                
                foreach (var clientPair in ServerSideClients)
                {
                    try
                    {
                        int clientId = clientPair.Key;
                        ServerSideClient serverSideClient = clientPair.Value;
                        
                        if (serverSideClient != null && serverSideClient.IsConnected())
                        {
                            Debug.Log($"Sending data to client {clientId}... from BroadcastData()");
                            serverSideClient.SendMessage(packet);
                        }
                        // else
                        // {
                        //     disconnectedClients.Add(clientId);
                        // }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error sending data to client {clientPair.Key}: {ex.Message}");
                        disconnectedClients.Add(clientPair.Key);
                    }
                }
                
                RemoveDisconnectedClients(disconnectedClients);
            }
        }
        
        private void RemoveDisconnectedClients(List<int> disconnectedClientIds)
        {
            foreach (int clientId in disconnectedClientIds)
            {
                ServerSideClients.Remove(clientId);
                Debug.Log($"Removed disconnected client {clientId}");
            }
        }

        // private void UpdatePlayerPositions()
        // {
        //     lock (_players)
        //     {
        //         foreach (var player in _players.Values)
        //         {
        //             // Convert Vector3 to Vector2 for the packet
        //             Vector2 position2D = new Vector2(player.position.x, player.position.y);
        //             // Convert Quaternion to float (using euler angles)
        //             float rotation = player.rotation.eulerAngles.z;
                    
        //             // We need to generate a player ID for the packet
        //             // Since we're using string IDs in _players, we'll use a hash of the string
        //             int playerId = player.id.GetHashCode();
                    
        //             var updatePacket = new UpdatePosServerPacket(playerId, position2D, rotation);
        //             BroadcastData(updatePacket);
        //         }
        //     }
        // }
        
        void OnApplicationQuit()
        {
            Shutdown();
        }
        
        public void Shutdown()
        {
            Debug.Log("Shutting down GameServer...");
            _isRunning = false;

            try
            {
                // Close all client connections
                lock (ServerSideClients)
                {
                    List<int> clientIds = new List<int>(ServerSideClients.Keys);
                    foreach (int clientId in clientIds)
                    {
                        try
                        {
                            if (ServerSideClients.TryGetValue(clientId, out ServerSideClient client))
                            {
                                Debug.Log($"Closing client connection {clientId}...");
                                client.Shutdown();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error closing client {clientId}: {ex.Message}");
                        }
                    }
                    ServerSideClients.Clear();
                }
                
                // Stop the server
                if (_server != null)
                {
                    _server.Stop();
                    _server = null;
                }
                
                // Wait for server thread to finish
                if (_serverThread != null && _serverThread.IsAlive)
                {
                    Debug.Log("Waiting for server thread to finish...");
                    _serverThread.Join(1000); // Wait up to 1 second
                    if (_serverThread.IsAlive)
                    {
                        Debug.LogWarning("Server thread did not finish in time, aborting...");
                        _serverThread.Abort();
                    }
                }
                
                Debug.Log("Server stopped.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error stopping server: " + ex.Message);
            }
        }
    }
}