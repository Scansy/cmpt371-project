using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class SpawnHandler : IPacketHandler
    {
        public void HandlePacket(IDisposable packet)
        {
            Debug.Log("this is the spawn packet");
            
            SpawnPlayerPacket spawnPlayerPacket = (SpawnPlayerPacket)packet;
            
            Debug.Log("this is the data from the packet: " + spawnPlayerPacket.spawnPosition + " " + spawnPlayerPacket.startingRotation);
        }
    }
}