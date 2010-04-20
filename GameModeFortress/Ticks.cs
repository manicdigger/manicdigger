using System;
using System.Collections.Generic;
using System.Text;

namespace GameModeDungeon
{
    public interface IGameWorld : IGameWorldRun
    {
        byte[] SaveState();
        void LoadState(byte[] savegame);
        string GameInfo { get; }
    }
    public interface IGameWorldRun
    {
        void Tick();
        void DoCommand(byte[] command, int player_id);
        int GetStateHash();
    }
}
