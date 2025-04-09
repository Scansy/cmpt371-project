using System;

namespace Shared.Packet
{
    [Serializable]
    public class WelcomePacket : IDisposable
    {
        private string _data = "test1test2test3";
        public string Data => _data; //getter

        public void Dispose()
        {
            _data = null;
        }
    }
}