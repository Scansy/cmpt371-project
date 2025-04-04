namespace Shared.PacketHandler
{
    public interface IPacketHandler
    {
        public void HandlePacket(Packet.IPacket packet);
        // everytime handler called, tell server to broadcast
    }
}