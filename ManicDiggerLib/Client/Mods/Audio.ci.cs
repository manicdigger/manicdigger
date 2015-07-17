public class ModAudio : ClientMod
{
    public ModAudio()
    {
        audioData = new DictionaryStringAudioData();
        wasLoaded = false;
    }

    DictionaryStringAudioData audioData;
    bool wasLoaded;

    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        if (game.assetsLoadProgress.value != 1)
        {
            return;
        }

        if (!wasLoaded)
        {
            wasLoaded = true;
            Preload(game);
        }

        // Load audio
        for (int i = 0; i < game.audio.soundsCount; i++)
        {
            Sound_ sound = game.audio.sounds[i];
            if (sound == null) { continue; }
            if (sound.audio != null) { continue; }

            AudioData data = GetAudioData(game, sound.name);
            if (game.platform.AudioDataLoaded(data))
            {
                sound.audio = game.platform.AudioCreate(data);
                game.platform.AudioPlay(sound.audio);
            }
        }

        // Update audio position
        for (int i = 0; i < game.audio.soundsCount; i++)
        {
            Sound_ sound = game.audio.sounds[i];
            if (sound == null) { continue; }
            if (sound.audio == null) { continue; }
            game.platform.AudioSetPosition(sound.audio, sound.x, sound.y, sound.z);
        }

        // Stop audio
        for (int i = 0; i < game.audio.soundsCount; i++)
        {
            Sound_ sound = game.audio.sounds[i];
            if (sound == null) { continue; }
            if (sound.audio == null) { continue; }
            if (sound.stop)
            {
                game.platform.AudioDelete(sound.audio);
                game.audio.sounds[i] = null;
            }
        }

        // Finish or loop audio
        for (int i = 0; i < game.audio.soundsCount; i++)
        {
            Sound_ sound = game.audio.sounds[i];
            if (sound == null) { continue; }
            if (sound.audio == null) { continue; }
            if (sound.loop)
            {
                if (game.platform.AudioFinished(sound.audio) && sound.loop)
                {
                    //game.platform.AudioPlay(sound.audio);
                    AudioData data = GetAudioData(game, sound.name);
                    if (game.platform.AudioDataLoaded(data))
                    {
                        sound.audio = game.platform.AudioCreate(data);
                        game.platform.AudioPlay(sound.audio);
                    }
                }
            }
            else
            {
                if (game.platform.AudioFinished(sound.audio))
                {
                    game.audio.sounds[i] = null;
                }
            }
        }
    }

    void Preload(Game game)
    {
        for (int k = 0; k < game.assets.count; k++)
        {
            string s = game.assets.items[k].name;
            string sound = game.platform.StringReplace(s, ".ogg", "");
            if (s == sound)
            {
                continue;
            }
            GetAudioData(game, s);
        }
    }

    AudioData GetAudioData(Game game, string sound)
    {
        if (!audioData.Contains(sound))
        {
            AudioData a = game.platform.AudioDataCreate(game.GetFile(sound), game.GetFileLength(sound));
            audioData.Set(sound, a);
        }
        return audioData.GetById(audioData.GetId(sound));
    }
}

public class AudioControl
{
    public AudioControl()
    {
        soundsCount = 0;
        sounds = new Sound_[soundsMax];
        for (int i = 0; i < soundsMax; i++)
        {
            sounds[i] = null;
        }
    }

    internal Sound_[] sounds;
    internal int soundsCount;
    const int soundsMax = 64;

    public void Clear()
    {
        for (int i = 0; i < soundsCount; i++)
        {
            sounds[i] = null;
        }
        soundsCount = 0;
    }

    public void Add(Sound_ s)
    {
        for (int i = 0; i < soundsCount; i++)
        {
            if (sounds[i] == null)
            {
                sounds[i] = s;
                return;
            }
        }
        if (soundsCount < soundsMax)
        {
            sounds[soundsCount++] = s;
        }
    }

    public void StopAll()
    {
        for (int i = 0; i < soundsCount; i++)
        {
            if (sounds[i] == null) { continue; }
            sounds[i].stop = true;
        }
    }
}

public class Sound_
{
    public Sound_()
    {
        name = null;
        x = 0;
        y = 0;
        z = 0;
        loop = false;
        stop = false;

        audio = null;
    }
    internal string name;
    internal float x;
    internal float y;
    internal float z;
    internal bool loop;
    internal bool stop;

    internal AudioCi audio;
}
