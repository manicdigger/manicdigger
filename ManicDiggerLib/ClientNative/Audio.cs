using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Audio;
using System.Threading;
using System.Diagnostics;
using OpenTK;

namespace ManicDigger
{
    public class AudioOpenAl
    {
        public GameExit d_GameExit;
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

        public class AudioTask : AudioCi
        {
            public AudioTask(GameExit gameexit, AudioDataCs sample, AudioOpenAl audio)
            {
                this.gameexit = gameexit;
                this.sample = sample;
                this.audio = audio;
            }
            AudioOpenAl audio;
            GameExit gameexit;
            AudioDataCs sample;
            public Vector3 position;
            public void Play()
            {
                if (started)
                {
                    shouldplay = true;
                    return;
                }
                started = true;
                ThreadPool.QueueUserWorkItem(delegate { play(); });
            }
            //bool resume = true;
            bool started = false;
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
                int source = OpenTK.Audio.OpenAL.AL.GenSource();
                int state;
                //using ()
                {
                    //Trace.WriteLine("Testing WaveReader({0}).ReadToEnd()", filename);

                    int buffer = OpenTK.Audio.OpenAL.AL.GenBuffer();

                    OpenTK.Audio.OpenAL.AL.BufferData(buffer, GetSoundFormat(sample.Channels, sample.BitsPerSample), sample.Pcm, sample.Pcm.Length, sample.Rate);
                    //audiofiles[filename]=buffer;

                    OpenTK.Audio.OpenAL.AL.DistanceModel(OpenTK.Audio.OpenAL.ALDistanceModel.InverseDistance);
                    OpenTK.Audio.OpenAL.AL.Source(source, OpenTK.Audio.OpenAL.ALSourcef.RolloffFactor, 0.3f);
                    OpenTK.Audio.OpenAL.AL.Source(source, OpenTK.Audio.OpenAL.ALSourcef.ReferenceDistance, 1);
                    OpenTK.Audio.OpenAL.AL.Source(source, OpenTK.Audio.OpenAL.ALSourcef.MaxDistance, (int)(64 * 1));
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
                        if (stop)
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
                                if (restart)
                                {
                                    OpenTK.Audio.OpenAL.AL.SourceRewind(source);
                                    restart = false;
                                }
                                OpenTK.Audio.OpenAL.AL.SourcePlay(source);
                            }
                        }
                        
                        OpenTK.Audio.OpenAL.AL.Source(source, OpenTK.Audio.OpenAL.ALSource3f.Position, position.X, position.Y, position.Z);
         
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
                    Finished = true;
                    OpenTK.Audio.OpenAL.AL.SourceStop(source);
                    OpenTK.Audio.OpenAL.AL.DeleteSource(source);
                    OpenTK.Audio.OpenAL.AL.DeleteBuffer(buffer);
                }
            }
            public bool loop = false;
            bool stop;
            public void Stop()
            {
                stop = true;
            }
            public bool shouldplay;
            public bool restart;
            public void Restart()
            {
                restart = true;
            }

            internal void Pause()
            {
                shouldplay = false;
            }


            internal bool Finished;
        }

        public AudioDataCs GetSampleFromArray(byte[] data)
        {
            Stream stream = new MemoryStream(data);
            if (stream.ReadByte() == 'R'
                && stream.ReadByte() == 'I'
                && stream.ReadByte() == 'F'
                && stream.ReadByte() == 'F')
            {
                stream.Position = 0;
                int channels, bits_per_sample, sample_rate;
                byte[] sound_data = LoadWave(stream, out channels, out bits_per_sample, out sample_rate);
                AudioDataCs sample = new AudioDataCs()
                {
                    Pcm = sound_data,
                    BitsPerSample = bits_per_sample,
                    Channels = channels,
                    Rate = sample_rate,
                };
                return sample;
            }
            else
            {
                stream.Position = 0;
                AudioDataCs sample = new OggDecoder().OggToWav(stream);
                return sample;
            }
        }
        Vector3 lastlistener;
        Vector3 lastorientation;
        public AudioTask CreateAudio(AudioDataCs sample)
        {
            return new AudioTask(d_GameExit, sample, this);
        }
        public void UpdateListener(Vector3 position, Vector3 orientation)
        {
            lastlistener = position;
            OpenTK.Audio.OpenAL.AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position, position.X, position.Y, position.Z);
            Vector3 up = Vector3.UnitY;
            OpenTK.Audio.OpenAL.AL.Listener(OpenTK.Audio.OpenAL.ALListenerfv.Orientation, ref orientation, ref up);
        }
    }
}
