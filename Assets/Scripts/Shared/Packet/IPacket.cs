using System;

namespace Shared.Packet
{
    public interface IPacket : IDisposable
    {
        public void HandlePacket();
    }
}