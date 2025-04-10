using UnityEngine;
using Shared.Packet;
using Shared.PacketHandler;
using System;
using GameLogic;

namespace Client
{
    public class GameStateHandler : IPacketHandler
    {
        private GameManager gameManager;

        public GameStateHandler(GameManager manager)
        {
            gameManager = manager;
        }

        public void HandlePacket(IDisposable packet)
        {
            switch (packet)
            {
                case StartGamePacket startGamePacket:
                    gameManager.StartGame();
                    break;

                case EndGamePacket endGamePacket:
                    gameManager.EndGame();
                    break;

                case GameTimeUpdatePacket timeUpdatePacket:
                    gameManager.UpdateGameTime(timeUpdatePacket.CurrentTime);
                    break;

                case GameWinnerPacket winnerPacket:
                    // The winner is already set by the server, just update display
                    gameManager.EndGame();
                    break;
            }
        }
    }
} 