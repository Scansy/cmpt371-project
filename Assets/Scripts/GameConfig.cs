using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConfig {
    public enum Role {None, Server, Client}
    public static Role CurrentRole = Role.None;
}
