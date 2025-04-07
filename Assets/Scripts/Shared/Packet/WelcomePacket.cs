using System;

namespace Shared.Packet
{
    [Serializable]
    public class WelcomePacket : IPacket
    {
        private string _data = "test1test2test3";
        public string Data => _data; //getter

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}