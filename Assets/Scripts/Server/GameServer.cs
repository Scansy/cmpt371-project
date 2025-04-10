using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameLogic;
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
            PacketHandlers.Add(typeof(TestPacket), new TestHandler());
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
                    UpdatePlayerPositions(); // Send game state to all clients every frame
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
                            serverSideClient.SendMessage(packet);
                        }
                        else
                        {
                            disconnectedClients.Add(clientId);
                        }
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

        private void UpdatePlayerPositions()
        {
            // string gameState = "GameState|";
            // lock (_players)
            // {
            //     foreach (var player in _players.Values)
            //     {
            //         // send position data
            //         gameState += $"{player.id},{player.position.x},{player.position.y},{player.position.z};";
            //     }
            // }
            // BroadcastData(gameState);

            //TODO:
            // loop through all players
            // create new UpdatePosition packet
            // broadcast each one
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
}