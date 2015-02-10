public class ModWalkSound : ClientMod
{
    public ModWalkSound()
    {
        one = 1;
        walksoundtimer = 0;
        lastwalksound = 0;
        stepsoundduration = one * 4 / 10;
    }
    float one;
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.FollowId() == null)
        {
            if (game.soundnow.value)
            {
                UpdateWalkSound(game, -1);
            }
            if (game.isplayeronground && game.controls.movedx != 0 || game.controls.movedy != 0)
            {
                UpdateWalkSound(game, args.GetDt());
            }
        }
    }
    internal void UpdateWalkSound(Game game, float dt)
    {
        if (dt == -1)
        {
            dt = stepsoundduration / 2;
        }
        walksoundtimer += dt;
        string[] soundwalk = soundwalkcurrent(game);
        if (GetSoundCount(soundwalk) == 0)
        {
            return;
        }
        if (walksoundtimer >= stepsoundduration)
        {
            walksoundtimer = 0;
            lastwalksound++;
            if (lastwalksound >= GetSoundCount(soundwalk))
            {
                lastwalksound = 0;
            }
            if ((game.rnd.Next() % 100) < 40)
            {
                lastwalksound = game.rnd.Next() % (GetSoundCount(soundwalk));
            }
            game.AudioPlay(soundwalk[lastwalksound]);
        }
    }
    internal float walksoundtimer;
    internal int lastwalksound;
    internal float stepsoundduration;

    internal string[] soundwalkcurrent(Game game)
    {
        int b = game.BlockUnderPlayer();
        if (b != -1)
        {
            return game.d_Data.WalkSound()[b];
        }
        return game.d_Data.WalkSound()[0];
    }

    internal int GetSoundCount(string[] soundwalk)
    {
        int count = 0;
        for (int i = 0; i < GameData.SoundCount; i++)
        {
            if (soundwalk[i] != null)
            {
                count++;
            }
        }
        return count;
    }
}
