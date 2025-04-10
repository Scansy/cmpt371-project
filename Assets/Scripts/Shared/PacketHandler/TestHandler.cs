using System;
using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class TestHandler : IPacketHandler
    {
        public void HandlePacket(IDisposable packet)
        {
            Debug.Log("this is the welcome packet");
            
            TestPacket testPacket = packet as TestPacket;
            Debug.Log("this is the data from the packet: " + testPacket.Data);
        }
    }
}