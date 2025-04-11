using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientStarter : MonoBehaviour
{
 void Start() {
        GameConfig.CurrentRole = GameConfig.Role.Client;
        SceneManager.LoadScene("GameScene");
    }
    void Update() {
        // Handle any client-specific updates here
    }
}
