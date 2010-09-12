using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Audio;
using System.Threading;
using System.Diagnostics;

namespace ManicDigger
{
    public interface IAudio
    {
        void Play(string filename);
        void PlayAudioLoop(string filename, bool play);
    }
    public class AudioDummy : IAudio
    {
        public void Play(string filename)
        {
        }
        public void PlayAudioLoop(string filename, bool play)
        {
        }
    }
    public class AudioOpenAl : IAudio
    {
        [Inject]
        public IGameExit gameexit { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        public AudioOpenAl()
        {
            try
            {
                IList<string> x = AudioContext.AvailableDevices;//only with this line an exception can be catched.
                context = new AudioContext();
            }
            catch (Exception e)
            {
                string oalinst = "oalinst.exe";
                if (File.Exists(oalinst))
                {
                    try
                    {
                        Process.Start(oalinst, "/s");
                    }
                    catch
                    {
                    }
                }
                Console.WriteLine(e);
            }
        }
        AudioContext context;
        /*
        static byte[] LoadOgg(Stream stream, out int channels, out int bits, out int rate)
        {
            byte[] bytes;
            Jarnbjo.Ogg.OggPage.Create(
            return bytes;
        }
        */
        // Loads a wave/riff audio file.
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }
        public static OpenTK.Audio.OpenAL.ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? OpenTK.Audio.OpenAL.ALFormat.Mono8 : OpenTK.Audio.OpenAL.ALFormat.Mono16;
                case 2: return bits == 8 ? OpenTK.Audio.OpenAL.ALFormat.Stereo8 : OpenTK.Audio.OpenAL.ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
        class X
        {
            public X(string filename, IGameExit gameexit)
            {
                this.filename = filename;
                this.gameexit = gameexit;
            }
            IGameExit gameexit;
            public string filename;
            public void Play()
            {
                if (started)
                {
                    shouldplay = true;
                    return;
                }
                started = true;
                new Thread(play).Start();
            }
            //bool resume = true;
            bool started = false;
            //static Dictionary<string, int> audiofiles = new Dictionary<string, int>();
            void play()
            {
                try
                {
                    DoPlay();
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            private void DoPlay()
            {
                //if(!audiofiles.ContainsKey(filename))
                {

                }
                int source = OpenTK.Audio.OpenAL.AL.GenSource();
                int state;
                //using ()
                {
                    //Trace.WriteLine("Testing WaveReader({0}).ReadToEnd()", filename);

                    int buffer = OpenTK.Audio.OpenAL.AL.GenBuffer();

                    int channels, bits_per_sample, sample_rate;
                    byte[] sound_data = LoadWave(File.Open(filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
                    OpenTK.Audio.OpenAL.AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);
                    //audiofiles[filename]=buffer;

                    OpenTK.Audio.OpenAL.AL.Source(source, OpenTK.Audio.OpenAL.ALSourcei.Buffer, buffer);
                    OpenTK.Audio.OpenAL.AL.SourcePlay(source);

                    // Query the source to find out when it stops playing.
                    for (; ; )
                    {
                        OpenTK.Audio.OpenAL.AL.GetSource(source, OpenTK.Audio.OpenAL.ALGetSourcei.SourceState, out state);
                        if ((!loop) && (OpenTK.Audio.OpenAL.ALSourceState)state != OpenTK.Audio.OpenAL.ALSourceState.Playing)
                        {
                            break;
                        }
                        if (gameexit.exit)
                        {
                            break;
                        }
                        if (loop)
                        {
                            if (state == (int)OpenTK.Audio.OpenAL.ALSourceState.Playing && (!shouldplay))
                            {
                                OpenTK.Audio.OpenAL.AL.SourcePause(source);
                            }
                            if (state != (int)OpenTK.Audio.OpenAL.ALSourceState.Playing && (shouldplay))
                            {
                                OpenTK.Audio.OpenAL.AL.SourcePlay(source);
                            }
                        }
                        /*
                        if (stop)
                        {
                            AL.SourcePause(source);
                            resume = false;
                        }
                        if (resume)
                        {
                            AL.SourcePlay(source);
                            resume = false;
                        }
                        */
                        Thread.Sleep(1);
                    }
                    OpenTK.Audio.OpenAL.AL.SourceStop(source);
                    OpenTK.Audio.OpenAL.AL.DeleteSource(source);
                    OpenTK.Audio.OpenAL.AL.DeleteBuffer(buffer);
                }
            }
            public bool loop = false;
            //bool stop;
            //public void Stop()
            //{
            //    stop = true;
            //}
            public bool shouldplay;
        }
        public void Play(string filename)
        {
            if (context == null)
            {
                return;
            }
            new X(getfile.GetFile(filename), gameexit).Play();
        }
        Dictionary<string, X> soundsplaying = new Dictionary<string, X>();
        public void PlayAudioLoop(string filename, bool play)
        {
            if (context == null)
            {
                return;
            }
            filename = getfile.GetFile(filename);
            //todo: resume playing.
            if (play)
            {
                if (!soundsplaying.ContainsKey(filename))
                {
                    var x = new X(filename, gameexit);
                    x.loop = true;
                    soundsplaying[filename] = x;
                }
                soundsplaying[filename].Play();
            }
            else
            {
                if (soundsplaying.ContainsKey(filename))
                {
                    soundsplaying[filename].shouldplay = false;
                    //soundsplaying.Remove(filename);
                }
            }
        }
    }
}
