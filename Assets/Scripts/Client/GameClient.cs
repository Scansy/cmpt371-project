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
        private readonly Dictionary<Type, IPacketHandler> _packetHandlers = new Dictionary<Type, IPacketHandler>();
        
        private Thread _sendThread;

        private ConcurrentQueue<IDisposable> _packetQueue = new ConcurrentQueue<IDisposable>(new Queue<IDisposable>());

        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = GameServer.DEFAULT_PORT;
        public GameObject playerPrefab;

        private PlayerSpawner _playerSpawner;
        private string _localPlayerId;

        public string getPlayerId(){
            return _localPlayerId;
        }

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

        private void InitSendThread2()
        {
            _sendThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    try
                    {
                        // Create a dummy movement vector (replace with actual movement logic)
                        Vector2 movementVector = new Vector2(1, 1);
                        Vector2 posVector = new Vector2(2, 2);

                        // Send the PlayerMovementPacket to the server
                        SendMessage(new PlayerMovementPacket(posVector, movementVector, 0.0f));

                        // Sleep to avoid overloading the server with too many packets
                        Thread.Sleep(50); // Adjust the interval as needed (e.g., 50ms = 20 updates per second)
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error in send thread: {ex.Message}");
                    }
                }
            });

            _sendThread.IsBackground = true;
            _sendThread.Start();
}

        private int _welcomePacketIdCounter = 1; // Counter for generating unique packet IDs
        private readonly object _idLock = new object(); // Lock object for thread safety

        private void ConnectToServer()
        {
            try
            {
                Debug.Log("Attempting to connect to server at " + ServerIP + ":" + ServerPort);
                _client = new TcpClient(ServerIP, ServerPort);
                _stream = _client.GetStream();
                
                if (_packetHandlers.Count == 0)
                {
                    InitializePacketHandlers();
                }
                InitReceiveThread();
                InitSendThread();
                
                lock (_idLock)
                {
                    _welcomePacketIdCounter++;
                    _localPlayerId = _welcomePacketIdCounter.ToString();
                }

                SendMessage(new TestPacket(_welcomePacketIdCounter));
                InitSendThread2();
                //Make a new thread and call this loop in the thread
                // while (_isRunning) {
                //     Vector2 myVector = new Vector2(1, 1);
                //     SendMessage(new PlayerMovementPacket(transform.position, myVector, 0.0f));
                // }
                
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to connect to server: " + e.Message);
            }
        }

        private void InitializePacketHandlers()
        {
            _packetHandlers.Add(typeof(TestPacket), new TestHandler());
            _packetHandlers.Add(typeof(SpawnPlayerPacket), new SpawnHandler());
            _packetHandlers.Add(typeof(UpdatePosServerPacket), new UpdatePosServerHandler());
            _packetHandlers.Add(typeof(UpdatePosClientPacket), new UpdatePosClientHandler());
            _packetHandlers.Add(typeof(PlayerMovementPacket), new PlayerMovementHandler());
        }

        private void InitReceiveThread()
        {
             Debug.Log("Going to make thread...");
            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            Debug.Log("Connected to server!");
        }
        
        public void ReceiveMessage()
        {
            while (_client.Connected)
            {
                try
                {
                    var stream = _client.GetStream();
                    var packet = (IDisposable)_formatter.Deserialize(stream); // Deserialize the packet
                    //Server.HandlePacket(packet); // Pass the packet to the server
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error receiving message: {ex.Message}");
                }
            }
        }

        private void HandlePacket(IDisposable packet)
        {
            _packetHandlers[packet.GetType()].HandlePacket(packet);
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
                //Debug.Log("Processing send queue...");
                bool dequeued;
                IDisposable packet;

                lock (_packetQueue)
                {
                    dequeued = _packetQueue.TryDequeue(out packet);
                }

                if (dequeued)
                {
                    Debug.Log("Sending packet: " + packet.GetType().Name);
                    try
                    {
                        _formatter.Serialize(_stream, packet); // CURRENT MAIN ERROR
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
                    if (_playerSpawner != null)
                    {
                        _playerSpawner.SpawnNetworkPlayer(_localPlayerId, position);
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
                    if (_playerSpawner != null)
                    {
                        _playerSpawner.SpawnNetworkPlayer(_localPlayerId, Vector3.zero);
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
            Shutdown();

            // Load the Main Menu scene
            SceneManager.LoadScene("Main Menu");
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down GameClient...");
            _isRunning = false;
            
            // Wait for threads to finish
            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                Debug.Log("Waiting for receive thread to finish...");
                _receiveThread.Join(1000); // Wait up to 1 second
                if (_receiveThread.IsAlive)
                {
                    Debug.LogWarning("Receive thread did not finish in time, aborting...");
                    _receiveThread.Abort();
                }
            }
            
            if (_sendThread != null && _sendThread.IsAlive)
            {
                Debug.Log("Waiting for send thread to finish...");
                _sendThread.Join(1000); // Wait up to 1 second
                if (_sendThread.IsAlive)
                {
                    Debug.LogWarning("Send thread did not finish in time, aborting...");
                    _sendThread.Abort();
                }
            }
            
            // Close network resources
            if (_stream != null)
            {
                try
                {
                    _stream.Close();
                    _stream = null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error closing stream: {ex.Message}");
                }
            }
            
            if (_client != null)
            {
                try
                {
                    _client.Close();
                    _client = null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error closing client: {ex.Message}");
                }
            }
            
            Debug.Log("GameClient shutdown complete.");
        }

        void OnApplicationQuit()
        {
            Shutdown();
        }
    }
}