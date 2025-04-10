using System;
using UnityEngine;
using Shared.PacketHandler;

namespace Shared.Packet
{
    [Serializable]
    public class StartGamePacket : IDisposable
    {
        public string Type => "StartGame";

        public void Dispose()
        {
        }
    }

    [Serializable]
    public class EndGamePacket : IDisposable
    {
        public string Type => "EndGame";

        public void Dispose()
        {
        }
    }

    [Serializable]
    public class GameTimeUpdatePacket : IDisposable
    {
        private float currentTime;

        public GameTimeUpdatePacket(float currentTime)
        {
            this.currentTime = currentTime;
        }

        public float CurrentTime => currentTime;

        public void Dispose()
        {
        }
    }

    [Serializable]
    public class GameWinnerPacket : IDisposable
    {
        private string winnerName;

        public GameWinnerPacket(string winnerName)
        {
            this.winnerName = winnerName;
        }

        public string WinnerName => winnerName;

        public void Dispose()
        {
        }
    }
} 