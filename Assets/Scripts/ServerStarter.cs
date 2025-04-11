using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerStarter : MonoBehaviour
{
   void Start() {
        GameConfig.CurrentRole = GameConfig.Role.Server;
        SceneManager.LoadScene("GameScene");
    }
    void Update() {
        // Handle any server-specific updates here
    }
}