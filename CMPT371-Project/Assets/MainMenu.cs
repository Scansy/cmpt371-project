using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void HostLobby()
    {
        // Load the game scene where the TCPServer is active
        SceneManager.LoadScene("Game");
    }

    public void JoinLobby()
    {
        // Load the client scene (you'll need to implement a client connection script)
        SceneManager.LoadScene("JoiningScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // This will only show in the editor, not in the built game
    }
}