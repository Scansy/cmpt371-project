using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    public bool isServer;
    
    void Start() {
    if (GameConfig.CurrentRole == GameConfig.Role.Server) {
        isServer = true;
    } else {
        isServer = false;
    }
}
}
