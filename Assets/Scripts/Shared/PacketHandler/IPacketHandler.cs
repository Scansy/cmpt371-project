using System;

namespace Shared.PacketHandler
{
    public interface IPacketHandler
    {
        public void HandlePacket(IDisposable packet);
        // everytime handler called, tell server to broadcast
    }
}