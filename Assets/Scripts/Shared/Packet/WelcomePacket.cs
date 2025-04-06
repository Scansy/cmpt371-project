using System;
using UnityEngine;

namespace Shared.Packet
{
    [Serializable]
    public class WelcomePacket : IPacket
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public void HandlePacket()
        {
            Debug.Log("Hello world");
        }
    }
}