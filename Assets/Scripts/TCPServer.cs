using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private bool isRunning = false;

    private List<TcpClient> connectedClients = new List<TcpClient>();
    private Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>(); // Track players by ID

    void Start()
    {
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, 7636);
            server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // Allow address reuse
            server.Start();
            isRunning = true;
            Debug.Log("Server started on port 7636...");

            while (isRunning)
            {
                SendGameState(); // Send game state to all clients every frame
                Thread.Sleep(1); // Adjust the sleep time as needed for performance

                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    Debug.Log("Client connected!");

                    lock (connectedClients)
                    {
                        connectedClients.Add(client);
                    }

                    Thread clientThread = new Thread(HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch (SocketException ex)
                {
                    Debug.LogError("Socket exception: " + ex.Message);
                }
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
        lock (connectedClients)
        {
            for (int i = connectedClients.Count - 1; i >= 0; i--)
            {
                try
                {
                    NetworkStream stream = connectedClients[i].GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error sending data to client: " + ex.Message);
                    connectedClients.RemoveAt(i); // Remove disconnected client
                }
            }
        }
    }

    void SendGameState()
    {
        string gameState = "GameState|";
        lock (players)
        {
            foreach (var player in players.Values)
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

        lock (players)
        {
            players[playerId] = new PlayerData { id = playerId, position = spawnPosition };
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
            lock (connectedClients)
            {
                connectedClients.Remove(client);
            }

            lock (players)
            {
                players.Remove(playerId);
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

            lock (players)
            {
                if (players.ContainsKey(playerId))
                {
                    players[playerId].position = newPosition;
                }
            }

            // Broadcast the updated position to all clients
            string updateMessage = $"UpdatePlayer|{playerId}|{newPosition.x},{newPosition.y},{newPosition.z}";
            BroadcastData(updateMessage);
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;

        try
        {
            server?.Stop();
            serverThread?.Join(); // Wait for the server thread to finish
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