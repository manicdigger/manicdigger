using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace ManicDigger
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
    public interface ITicks
    {
        void DoCommand(byte[] command);
    }
    public class TicksDummy : ITicks
    {
        [Inject]
        public IGameWorldRun game { get; set; }
        public void DoCommand(byte[] command)
        {
            game.DoCommand(command, 0);
        }
    }
}
