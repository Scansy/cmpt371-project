using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Shared.Packet;
using Shared.PacketHandler;
using UnityEngine;

namespace MockTest
{
    public class TestMain : MonoBehaviour
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private IDisposable _originalPacket = new TestPacket(1);
        // private bool _isRunning = true;
        private Thread _serverThread;
        private TcpListener _server;
        private const int PORT = 12345;
        private readonly Dictionary<Type, IPacketHandler> _packetHandlers = new Dictionary<Type, IPacketHandler>();

        void Start()
        {
            _packetHandlers.Add(typeof(TestPacket), new TestHandler());
            // TestSerialization();
            TestTcpSerialization();
        }

        public void TestSerialization()
        {
            Debug.Log("Starting serialization test...");
            
            // Create a memory stream to simulate network transmission
            using (MemoryStream stream = new MemoryStream())
            {
                // Serialize the packet
                _formatter.Serialize(stream, _originalPacket);
                Debug.Log("Packet serialized successfully");
                
                // Reset stream position to beginning
                stream.Position = 0;
                
                // Deserialize the packet
                IDisposable deserializedPacket = (IDisposable)_formatter.Deserialize(stream);
                Debug.Log("Packet deserialized successfully");
                
                // Call HandlePacket on the deserialized packet
                _packetHandlers[deserializedPacket.GetType()].HandlePacket(deserializedPacket);
            }
        }

        public void TestTcpSerialization()
        {
            Debug.Log("Starting TCP serialization test...");
            
            // Start server in a separate thread
            _serverThread = new Thread(StartServer);
            _serverThread.IsBackground = true;
            _serverThread.Start();
            
            // Give the server a moment to start
            Thread.Sleep(100);
            
            // Connect client to server
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect("127.0.0.1", PORT);
                    Debug.Log("Client connected to server");
                    
                    // Get the network stream
                    NetworkStream stream = client.GetStream();
                    
                    // Serialize the packet and send it
                    _formatter.Serialize(stream, _originalPacket);
                    Debug.Log("Packet sent over TCP successfully");
                    
                    // Wait for response from server
                    IDisposable receivedPacket = (IDisposable)_formatter.Deserialize(stream);
                    Debug.Log("Packet received over TCP successfully");
                    
                    // Call HandlePacket on the received packet
                    _packetHandlers[receivedPacket.GetType()].HandlePacket(receivedPacket);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"TCP test error: {ex.Message}");
                }
            }
            
            // Stop the server
            // _isRunning = false;
            _server?.Stop();
            _serverThread?.Join(1000);
        }
        
        private void StartServer()
        {
            try
            {
                _server = new TcpListener(IPAddress.Loopback, PORT);
                _server.Start();
                Debug.Log($"Server started on port {PORT}");
                
                // Accept a single client connection
                TcpClient client = _server.AcceptTcpClient();
                Debug.Log("Server accepted client connection");
                
                // Get the network stream
                NetworkStream stream = client.GetStream();
                
                // Wait for packet from client
                IDisposable receivedPacket = (IDisposable)_formatter.Deserialize(stream);
                Debug.Log("Server received packet");
                
                // Echo the packet back to the client
                _formatter.Serialize(stream, receivedPacket);
                Debug.Log("Server sent packet back to client");
                
                // Clean up
                client.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Server error: {ex.Message}");
            }
            finally
            {
                _server?.Stop();
            }
        }
        
        void OnDestroy()
        {
            // _isRunning = false;
            _server?.Stop();
            _serverThread?.Join(1000);
        }
    }
}