using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class SpawnPacketHandler : IPacketHandler
    {
        public void HandlePacket(SpawnPlayerPacket packet)
        {
            Debug.Log("this is the spawn packet");
            
            SpawnPlayerPacket spawnPlayerPacket = packet as SpawnPlayerPacket;
            Debug.Log("this is the data from the packet: " + spawnPlayerPacket.spawnPosition + " " + spawnPlayerPacket.startingRotation);
        }
    }
}