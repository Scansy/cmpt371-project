using Shared.Packet;
using UnityEngine;

namespace Shared.PacketHandler
{
    public class WelcomeHandler : IPacketHandler
    {
        public void HandlePacket(IPacket packet)
        {
            Debug.Log("this is the welcome packet");
            
            WelcomePacket welcomePacket = packet as WelcomePacket;
            Debug.Log("this is the data from the packet: " + welcomePacket.Data);
        }
    }
}