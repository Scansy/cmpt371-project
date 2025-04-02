using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TCPClient : MonoBehaviour
{
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    public string serverIP = " 10.0.0.25"; 
    public int serverPort = 7636; 
    public GameObject playerPrefab; // Assign your player prefab in the Inspector

    private bool isRunning = true; // Flag to control the thread loop
    void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            Debug.Log("Attempting to connect to server at " + serverIP + ":" + serverPort);
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("Connected to server!");

            // Notify the server that a new client has joined
            SendMessageToServer("ClientConnected");
            //SceneManager.LoadScene("Game");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect to server: " + e.Message);
        }
    }

    void SendMessageToServer(string message)
    {
        if (stream != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
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
            if (!otherPlayers.ContainsKey(playerId))
            {
                // Spawn the player
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                    otherPlayers[playerId] = newPlayer;
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

            // Update the player's position
            if (otherPlayers.ContainsKey(playerId))
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    otherPlayers[playerId].transform.position = position;
                });
            }
        }
    }

    // Handle player removal message from the server (To be used later)
    void HandleRemovePlayerMessage(string message)
    {
        string[] parts = message.Split('|');
        if (parts.Length > 1)
        {
            string playerId = parts[1];

            // Remove the player from the scene
            if (otherPlayers.ContainsKey(playerId))
            {
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    Destroy(otherPlayers[playerId]);
                    otherPlayers.Remove(playerId);
                    Debug.Log($"Removed player {playerId} from the scene.");
                });
            }
        }
    }

    
    void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (isRunning)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Received from server: " + message);

                // Handle server messages
                if (message.StartsWith("SpawnPlayer"))
                {
                    HandleSpawnPlayerMessage(message);
                }
                else if (message.StartsWith("PlayerPositions"))
                {
                    HandlePlayerPositionsMessage(message);
                }
                else if (message == "StartGame")
                {
                    // Transition to the main game scene
                    SceneManager.LoadScene("Game");

                    // Spawn the player GameObject in the main game scene
                    SceneManager.sceneLoaded += OnGameSceneLoaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving data: " + e.Message);
                break;
            }
        }
    }

    void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab != null)
        {
            // Enqueue the Instantiate call to the main thread
            MainThreadDispatcher.RunOnMainThread(() =>
            {
                Instantiate(playerPrefab, position, Quaternion.identity);
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
                Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
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
        if (client != null)
        {
            client.Close();
        }
        if (stream != null)
        {
            stream.Close();
        }
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }

        // Load the Main Menu scene
        SceneManager.LoadScene("Main Menu");
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        receiveThread?.Join(); // Wait for the thread to finish
        client?.Close();
        stream?.Close();
    }
}