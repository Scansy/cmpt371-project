using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject menuPanel; // Assign the parent GameObject of the buttons in the Inspector

    public void HostLobby()
    {
        // Hide the menu buttons
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Load the game scene where the TCPServer is active
        SceneManager.LoadScene("Game");
    }

    public void JoinLobby()
    {
        // Hide the menu buttons
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Load the client scene (you'll need to implement a client connection script)
        SceneManager.LoadScene("JoiningScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // This will only show in the editor, not in the built game
    }
}