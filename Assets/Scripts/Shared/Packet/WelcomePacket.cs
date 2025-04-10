using System;

namespace Shared.Packet
{
    [Serializable]
    public class WelcomePacket : IDisposable
    {
        private string _data = "test1test2test3";
        public string Data => _data; //getter

        public int ID { get; set; } // Unique ID for the packet
       

        public WelcomePacket(int id)
        {
            ID = id;

        }

        public void Dispose()
        {
            _data = null;
        }
    }
}