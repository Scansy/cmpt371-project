using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using GameLogic;
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

        private ConcurrentQueue<IDisposable> _packetQueue = new ConcurrentQueue<IDisposable>(new Queue<IDisposable>());
        
        private readonly string serverIP = "127.0.0.1";
        private const int serverPort = GameServer.DEFAULT_PORT;
        public GameObject playerPrefab;

        private PlayerSpawner playerSpawner;
        private string localPlayerId;

        void Start()
        {
            // playerSpawner = PlayerSpawner.Instance;
            // if (playerSpawner == null)
            // {
            //     Debug.LogError("PlayerSpawner not found in scene!");
            //     return;
            // }
            
            ConnectToServer();
        }

        private int welcomePacketIdCounter = 1; // Counter for generating unique packet IDs
        private readonly object idLock = new object(); // Lock object for thread safety

        private void ConnectToServer()
        {
            try
            {
                Debug.Log("Attempting to connect to server at " + serverIP + ":" + serverPort);
                _client = new TcpClient(serverIP, serverPort);
                _stream = _client.GetStream();
                
                if (PacketHandlers.Count == 0)
                {
                    InitializePacketHandlers();
                }
                InitReceiveThread();
                InitSendThread();
                lock (idLock)
                {
                    welcomePacketIdCounter++;
                }

                SendMessage(new WelcomePacket(welcomePacketIdCounter));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to connect to server: " + e.Message);
            }
        }

        private void InitializePacketHandlers()
        {
            PacketHandlers.Add(typeof(TestPacket), new TestHandler());
        }

        private void InitReceiveThread()
        {
             Debug.Log("Going to make thread...");
            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            Debug.Log("Connected to server!");
        }
        
        private void ReceiveMessage()
        {
            while (_isRunning)
            {
                var packet = (IDisposable)_formatter.Deserialize(_stream);
                HandlePacket(packet);
            }
        }

        private void HandlePacket(IDisposable packet)
        {
            PacketHandlers[packet.GetType()].HandlePacket(packet);
        }

        private void InitSendThread()
        {
            _sendThread = new Thread(ProcessSendQueue);
            _sendThread.IsBackground = true;
            _sendThread.Start();
        }

        private void ProcessSendQueue()
        {
            while (_isRunning)
            {
                bool dequeued;
                IDisposable packet;

                lock (_packetQueue)
                {
                    dequeued = _packetQueue.TryDequeue(out packet);
                }

                if (dequeued)
                {
                    try
                    {
                        _formatter.Serialize(_stream, packet);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error sending packet: {ex.Message}");
                    }
                }
                Thread.Sleep(1); // make it not run too many resources
            }
        }

        public void SendMessage(IDisposable packet)
        {
            if (packet != null)
            {
                lock (_packetQueue)
                {
                    _packetQueue.Enqueue(packet);
                }
            }
        }
    
        private void SpawnPlayer(Vector3 position)
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

        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
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