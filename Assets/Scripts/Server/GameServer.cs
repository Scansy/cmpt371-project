using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared.Packet;
using UnityEngine;
using GameLogic;

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
                if (_applicationIsQuitting) return null;

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

        private TcpListener _server;
        private Thread _serverThread;
        private bool _isRunning = false;

        public readonly Dictionary<int, ServerSideClient> ServerSideClients = new Dictionary<int, ServerSideClient>();
        public readonly Dictionary<string, PlayerData> Players = new Dictionary<string, PlayerData>();

        public const short DEFAULT_PORT = 7777;

        private void Start()
        {
            _serverThread = new Thread(StartServer);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        private void StartServer()
        {
            try
            {
                _server = new TcpListener(IPAddress.Any, DEFAULT_PORT);
                _server.Start();
                _isRunning = true;
                Debug.Log("Server started on port " + DEFAULT_PORT);

                while (_isRunning)
                {
                    if (_server.Pending())
                    {
                        TcpClient client = _server.AcceptTcpClient();
                        Debug.Log("Client connected!");

                        lock (ServerSideClients)
                        {
                            int clientId = ServerSideClients.Count + 1;
                            var serverSideClient = new ServerSideClient(this, client, clientId);
                            ServerSideClients[clientId] = serverSideClient;

                            Thread clientThread = new Thread(serverSideClient.ReceiveMessage);
                            clientThread.IsBackground = true;
                            clientThread.Start();
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Server error: " + ex.Message);
            }
        }

        public void HandlePacket(IDisposable packet)
        {
            if (packet is UpdatePosServerPacket updatePacket)
            {
                lock (Players)
                {
                    if (Players.ContainsKey(updatePacket.PlayerId))
                    {
                        Players[updatePacket.PlayerId].position = updatePacket.Position;
                        Players[updatePacket.PlayerId].rotation = updatePacket.Rotation;
                    }
                }

                BroadcastData(packet);
            }
        }

        public void BroadcastData(IDisposable packet)
        {
            lock (ServerSideClients)
            {
                foreach (var client in ServerSideClients.Values)
                {
                    client.SendMessage(packet);
                }
            }
        }

        private void OnApplicationQuit()
        {
            _isRunning = false;
            _server?.Stop();
            _serverThread?.Join();
        }
    }
}