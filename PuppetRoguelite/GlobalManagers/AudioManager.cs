using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Tweens;
using PuppetRoguelite.Models;
using SDL2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class AudioManager : GlobalManager
    {
        const float _defaultMusicVolume = .2f;
        const float _defaultPitch = 0f;
        const float _defaultPan = 0;
        const float _defaultSoundVolume = .25f;

        nint _handler;
        nint _masteringVoice;
        IntPtr _musicFileHandle;
        IntPtr _musicVoiceHandle;

        uint _loopPoint;

        public string CurrentSongName;
        public Song CurrentSong;

        protected object StateLock = new object();

        internal AudioManager()
        {
            //create handler
            FAudio.FAudioCreate(out _handler, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);

            //create master voice
            FAudio.FAudio_CreateMasteringVoice(_handler, out _masteringVoice, FAudio.FAUDIO_DEFAULT_CHANNELS, FAudio.FAUDIO_DEFAULT_SAMPLERATE, 0, 0, IntPtr.Zero);
        }

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

        //public void PlayMusic(string musicName, bool looping = true, uint loopTime = 0)
        //{
        //    var uri = new Uri(musicName, UriKind.Relative);
        //    var song = Song.FromUri(musicName, uri);

        //    if (MediaPlayer.State == MediaState.Playing)
        //    {
        //        MediaPlayer.Stop();
        //    }

        //    MediaPlayer.Play(song);
        //    MediaPlayer.Volume = _defaultMusicVolume;

        //    CurrentSong = song;

        //    _loopPoint = loopTime;
        //}

        //public void PlayMusic(SongModel songModel, bool looping = true)
        //{
        //    var uri = new Uri(songModel.Path, UriKind.Relative);
        //    var song = Song.FromUri(songModel.Path, uri);

        //    if (MediaPlayer.State == MediaState.Playing)
        //    {
        //        MediaPlayer.Stop();
        //    }

        //    MediaPlayer.Play(song);
        //    MediaPlayer.Volume = _defaultMusicVolume;

        //    CurrentSong = song;

        //    _loopPoint = songModel.LoopTime;
        //}

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
            lock (StateLock)
            {
                FAudio.FAudioSourceVoice_Stop(_musicVoiceHandle, 0, FAudio.FAUDIO_COMMIT_NOW);
                FAudio.FAudioSourceVoice_FlushSourceBuffers(_musicVoiceHandle);
                MediaPlayer.Stop();
            }
        }

        public bool IsPlayingMusic(string music)
        {
            if (MediaPlayer.State == MediaState.Playing && CurrentSong.Name == music)
                return true;
            return false;
        }

        public override void Update()
        {
            base.Update();

            //if (_loopPoint > 0)
            //{
            //    if (MediaPlayer.State == MediaState.Stopped)
            //    {
            //        MediaPlayer.Play(CurrentSong);
            //        MediaPlayer.
            //    }
            //    if (!_currentMusicChannel.IsPlaying)
            //    {
            //        _currentMusicChannel = _currentMusic.Play();
            //        _currentMusicChannel.Volume = _defaultMusicVolume;
            //        _currentMusicChannel.TrackPosition = _loopPoint;
            //    }
            //}

            //if (_soundChannels.Count > 0)
            //{
            //    var queue = new Queue<KeyValuePair<string, Channel>>(_soundChannels.ToList());
            //    for (int i = 0; i < queue.Count; i++)
            //    {
            //        var pair = queue.Dequeue();
            //        if (!pair.Value.IsPlaying)
            //        {
            //            _soundChannels.Remove(pair.Key);
            //        }
            //    }
            //}
        }

        public unsafe void PlayMusic(SongModel songModel, bool looping = true)
        {
            PlayMusic(songModel.Path, looping);
        }

        public unsafe void PlayMusic(string filePath, bool looping = true)
        {
            //open file
            _musicFileHandle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

            //get file info
            var info = FAudio.stb_vorbis_get_info(_musicFileHandle);

            //get format
            var blockAlign = (ushort)((32 / 8) * info.channels);
            var format = new FAudio.FAudioWaveFormatEx
            {
                wFormatTag = 3,
                nChannels = (ushort)info.channels,
                nSamplesPerSec = info.sample_rate,
                wBitsPerSample = 32,
                nBlockAlign = blockAlign,
                nAvgBytesPerSec = blockAlign * info.sample_rate
            };

            //create buffer
            var lengthInFloats = FAudio.stb_vorbis_stream_length_in_samples(_musicFileHandle) * info.channels;
            var lengthInBytes = lengthInFloats * Marshal.SizeOf<float>();
            var bufferDataPointer = NativeMemory.Alloc((nuint)lengthInBytes);
            FAudio.stb_vorbis_get_samples_float_interleaved(_musicFileHandle, info.channels, (nint)bufferDataPointer, (int)lengthInFloats);
            FAudio.stb_vorbis_close(_musicFileHandle);
            var buffer = new FAudio.FAudioBuffer
            {
                Flags = FAudio.FAUDIO_END_OF_STREAM,
                AudioBytes = (uint)lengthInBytes,
                pAudioData = (nint)bufferDataPointer,
                PlayBegin = 0,
                PlayLength = 0,
                LoopBegin = 0,
                LoopLength = 0,
                LoopCount = FAudio.FAUDIO_LOOP_INFINITE
            };

            //create source voice
            FAudio.FAudio_CreateSourceVoice(_handler, out _musicVoiceHandle, ref format, FAudio.FAUDIO_VOICE_USEFILTER, FAudio.FAUDIO_DEFAULT_FREQ_RATIO, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            //FAudio.FAudioSendDescriptor* sendDesc = stackalloc FAudio.FAudioSendDescriptor[1];
            //sendDesc[0].Flags = 0;
            //sendDesc[0].pOutputVoice = _masteringVoice;

            //var sends = new FAudio.FAudioVoiceSends();
            //sends.SendCount = 1;
            //sends.pSends = (nint)sendDesc;

            //FAudio.FAudioVoice_SetOutputVoices(_sourceVoiceHandle, ref sends);

            //set volume
            FAudio.FAudioVoice_SetVolume(_musicVoiceHandle, _defaultMusicVolume, 0);

            lock (StateLock)
            {
                //submit buffer
                FAudio.FAudioSourceVoice_SubmitSourceBuffer(_musicVoiceHandle, ref buffer, IntPtr.Zero);
            }

            lock (StateLock)
            {
                //start music voice
                FAudio.FAudioSourceVoice_Start(_musicVoiceHandle, 0, FAudio.FAUDIO_COMMIT_NOW);
            }
        }
    }
}
