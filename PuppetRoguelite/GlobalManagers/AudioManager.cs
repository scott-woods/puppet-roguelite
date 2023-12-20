using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;
using PuppetRoguelite.Audio;
using PuppetRoguelite.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PuppetRoguelite.GlobalManagers
{
    public class AudioManager : GlobalManager
    {
        const float _defaultMusicVolume = .18f;
        const float _defaultSoundVolume = .25f;
        const float _volumeReductionFactor = .01f;


        StreamingVoice _musicVoice;

        public AudioDevice AudioDevice { get; }

        public string CurrentSongName;
        public Song CurrentSong;

        Dictionary<SoundEffect, List<SoundEffectInstance>> _soundInstances = new Dictionary<SoundEffect, List<SoundEffectInstance>>();

        public AudioManager()
        {
            AudioDevice = new AudioDevice();

            Game1.Emitter.AddObserver(CoreEvents.Exiting, OnGameExiting);
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

        public void PlaySound(string soundName)
        {
            Game1.StartCoroutine(PlaySoundCoroutine(soundName));
        }

        public IEnumerator PlaySoundCoroutine(string soundName)
        {
            var sound = Game1.Scene.Content.LoadSoundEffect(soundName);

            if (!_soundInstances.ContainsKey(sound))
            {
                _soundInstances[sound] = new List<SoundEffectInstance>();
            }

            CleanupSoundInstances(sound);

            var soundInstance = sound.CreateInstance();
            int instanceCount = _soundInstances[sound].Count;

            float volume = Math.Max(0, _defaultSoundVolume * (1 - (instanceCount * _volumeReductionFactor)));
            soundInstance.Volume = volume;

            soundInstance.Play();
            _soundInstances[sound].Add(soundInstance);

            while (soundInstance.State == Microsoft.Xna.Framework.Audio.SoundState.Playing)
            {
                yield return null;
            }
        }

        void CleanupSoundInstances(SoundEffect sound)
        {
            _soundInstances[sound].RemoveAll(instance => instance.State == Microsoft.Xna.Framework.Audio.SoundState.Stopped);
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
        }

        public void PauseMusic()
        {
            _musicVoice?.Pause();
        }

        public void ResumeMusic()
        {
            _musicVoice?.Play();
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
