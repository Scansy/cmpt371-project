using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedStarter : MonoBehaviour
{
    void Start() {
    if (GameConfig.CurrentRole == GameConfig.Role.Server) {
        // Enable server stuff
        Debug.Log("Running as Server");
    }
    else if (GameConfig.CurrentRole == GameConfig.Role.Client) {
        // Enable client visuals
        Debug.Log("Running as Client");
    }
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
