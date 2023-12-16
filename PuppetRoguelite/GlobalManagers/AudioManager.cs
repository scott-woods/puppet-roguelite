using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Tweens;
using PuppetRoguelite.Audio;
using PuppetRoguelite.Models;
using SDL2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class AudioManager : GlobalManager
    {
        const float _defaultMusicVolume = .15f;
        const float _defaultPitch = 0f;
        const float _defaultPan = 0;
        const float _defaultSoundVolume = .25f;

        const int _step = 200;

        nint _handler;
        nint _masteringVoice;
        IntPtr _musicFileHandle;
        IntPtr _musicVoiceHandle;

        uint _loopPoint;

        StreamingVoice _musicVoice;

        //Thread _thread;
        //bool _running;
        //Stopwatch _tickStopWatch = new Stopwatch();
        //long _previousTickTime;
        //TimeSpan _updateInterval;
        //AutoResetEvent _wakeSignal;
        //internal readonly object StateLock = new object();

        public AudioDevice AudioDevice { get; }

        public string CurrentSongName;
        public Song CurrentSong;

        public AudioManager()
        {
            AudioDevice = new AudioDevice();

            Game1.Emitter.AddObserver(CoreEvents.Exiting, OnGameExiting);

            //create handler
            //FAudio.FAudioCreate(out _handler, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);

            ////create master voice
            //FAudio.FAudio_CreateMasteringVoice(_handler, out _masteringVoice, FAudio.FAUDIO_DEFAULT_CHANNELS, FAudio.FAUDIO_DEFAULT_SAMPLERATE, 0, 0, IntPtr.Zero);

            //_updateInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / _step);

            //_wakeSignal = new AutoResetEvent(true);

            //_thread = new Thread(ThreadMain);
            //_thread.IsBackground = true;
            //_thread.Start();

            //_running = true;

            //_tickStopWatch.Start();
            //_previousTickTime = 0;
        }

        public override void Update()
        {
            base.Update();

            AudioDevice.WakeThread();
        }

        void OnGameExiting()
        {
            AudioDevice.Dispose();
        }

        //void ThreadMain()
        //{
        //    while (_running)
        //    {
        //        lock (StateLock)
        //        {
        //            try
        //            {
        //                ThreadMainTick();
        //            }
        //            catch (Exception e)
        //            {
        //                Nez.Debug.Log(e.ToString());
        //            }
        //        }

        //        _wakeSignal.WaitOne(_updateInterval);
        //    }
        //}

        //void ThreadMainTick()
        //{
        //    long tickDelta = _tickStopWatch.Elapsed.Ticks - _previousTickTime;
        //    _previousTickTime = _tickStopWatch.Elapsed.Ticks;
        //    float elapsedSeconds = (float)tickDelta / System.TimeSpan.TicksPerSecond;

        //    //AudioTweenManager.Update(elapsedSeconds);

        //    foreach (var voice in updatingSourceVoices)
        //    {
        //        voice.Update();
        //    }

        //    foreach (var voice in VoicesToReturn)
        //    {
        //        if (voice is UpdatingSourceVoice updatingSourceVoice)
        //        {
        //            updatingSourceVoices.Remove(updatingSourceVoice);
        //        }

        //        voice.Reset();
        //        VoicePool.Return(voice);
        //    }

        //    VoicesToReturn.Clear();
        //}

        public void PlaySound(string soundName)
        {
            var sound = Game1.Scene.Content.LoadSoundEffect(soundName);
            var soundInstance = sound.CreateInstance();
            soundInstance.Volume = _defaultSoundVolume;
            soundInstance.Play();
        }

        public IEnumerator PlaySoundCoroutine(string soundName)
        {
            var sound = Game1.Scene.Content.LoadSoundEffect(soundName);
            var soundInstance = sound.CreateInstance();
            soundInstance.Volume = _defaultSoundVolume;
            soundInstance.Play();

            while (soundInstance.State == Microsoft.Xna.Framework.Audio.SoundState.Playing)
            {
                yield return null;
            }
        }

        /// <summary>
        /// play music via song model
        /// </summary>
        /// <param name="songModel"></param>
        /// <param name="looping"></param>
        public unsafe void PlayMusic(SongModel songModel, bool looping = true)
        {
            PlayMusic(songModel.Path, looping);
        }

        /// <summary>
        /// play music by file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="looping"></param>
        public unsafe void PlayMusic(string filePath, bool looping = true)
        {
            StopMusic();

            var audioDataOgg = new AudioDataOgg(AudioDevice, filePath);

            _musicVoice = new StreamingVoice(AudioDevice, audioDataOgg.Format);

            _musicVoice.Load(audioDataOgg);

            _musicVoice.SetVolume(_defaultMusicVolume);

            _musicVoice.Loop = looping;

            _musicVoice.Play();

            ////open file
            //_musicFileHandle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

            ////get file info
            //var info = FAudio.stb_vorbis_get_info(_musicFileHandle);

            ////get format
            //var blockAlign = (ushort)((32 / 8) * info.channels);
            //var format = new FAudio.FAudioWaveFormatEx
            //{
            //    wFormatTag = 3,
            //    nChannels = (ushort)info.channels,
            //    nSamplesPerSec = info.sample_rate,
            //    wBitsPerSample = 32,
            //    nBlockAlign = blockAlign,
            //    nAvgBytesPerSec = blockAlign * info.sample_rate
            //};

            ////create buffer
            //var lengthInFloats = FAudio.stb_vorbis_stream_length_in_samples(_musicFileHandle) * info.channels;
            //var lengthInBytes = lengthInFloats * Marshal.SizeOf<float>();
            //var bufferDataPointer = NativeMemory.Alloc((nuint)lengthInBytes);
            //FAudio.stb_vorbis_get_samples_float_interleaved(_musicFileHandle, info.channels, (nint)bufferDataPointer, (int)lengthInFloats);
            //FAudio.stb_vorbis_close(_musicFileHandle);
            //var buffer = new FAudio.FAudioBuffer
            //{
            //    Flags = FAudio.FAUDIO_END_OF_STREAM,
            //    AudioBytes = (uint)lengthInBytes,
            //    pAudioData = (nint)bufferDataPointer,
            //    PlayBegin = 0,
            //    PlayLength = 0,
            //    LoopBegin = 0,
            //    LoopLength = 0,
            //    LoopCount = FAudio.FAUDIO_LOOP_INFINITE
            //};

            ////create source voice
            //FAudio.FAudio_CreateSourceVoice(_handler, out _musicVoiceHandle, ref format, FAudio.FAUDIO_VOICE_USEFILTER, FAudio.FAUDIO_DEFAULT_FREQ_RATIO, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            ////set volume
            //FAudio.FAudioVoice_SetVolume(_musicVoiceHandle, _defaultMusicVolume, 0);

            //lock (StateLock)
            //{
            //    //submit buffer
            //    FAudio.FAudioSourceVoice_SubmitSourceBuffer(_musicVoiceHandle, ref buffer, IntPtr.Zero);
            //}

            //lock (StateLock)
            //{
            //    //start music voice
            //    FAudio.FAudioSourceVoice_Start(_musicVoiceHandle, 0, FAudio.FAUDIO_COMMIT_NOW);
            //}
        }

        public void PauseMusic()
        {
            MediaPlayer.Pause();
        }

        public void ResumeMusic()
        {
            MediaPlayer.Resume();
        }

        public void StopMusic()
        {
            _musicVoice?.Unload();
            //FAudio.FAudioSourceVoice_Stop(_musicVoiceHandle, 0, FAudio.FAUDIO_COMMIT_NOW);
            //FAudio.FAudioSourceVoice_FlushSourceBuffers(_musicVoiceHandle);
            //MediaPlayer.Stop();
        }
    }
}
