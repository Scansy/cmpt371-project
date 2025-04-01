using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TCPClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    public string serverIP = "127.0.0.1"; // Default server IP
    public int serverPort = 7777; // Default server port
    public GameObject playerPrefab; // Assign your player prefab in the Inspector

    void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("Connected to server!");

            // Notify the server that a new client has joined
            SendMessageToServer("ClientConnected");
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

    void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (client.Connected)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Received from server: " + message);

                // Handle server messages
                if (message == "StartGame")
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

    void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Spawn the player GameObject
        if (scene.name == "Game" && playerPrefab != null)
        {
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("Player spawned in the game scene.");
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
    }
}