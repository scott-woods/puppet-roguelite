﻿using FmodForFoxes;
using Microsoft.Xna.Framework.Media;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class AudioManager : GlobalManager
    {
        const float _defaultMusicVolume = .5f;
        const float _defaultPitch = 0f;
        const float _defaultPan = 0;
        const float _defaultSoundVolume = 1f;

        uint _loopPoint;

        Sound _currentMusic;
        Channel _currentMusicChannel;

        public void PlaySound(string soundName, float volume = 0f)
        {
            volume = volume > 0f ? volume : _defaultSoundVolume;
            var sound = CoreSystem.LoadSound(soundName);
            var channel = sound.Play();
            channel.Volume = volume;
        }

        public IEnumerator PlaySoundCoroutine(string soundName)
        {
            var sound = CoreSystem.LoadSound(soundName);
            var channel = sound.Play();

            while (channel.IsPlaying)
            {
                yield return null;
            }
        }

        public void PlayMusic(string musicName, bool looping = true, uint loopTime = 0)
        {
            if (_currentMusicChannel.IsPlaying)
            {
                _currentMusicChannel.Stop();
                _currentMusic = null;
                _loopPoint = 0;
            }

            var music = CoreSystem.LoadStreamedSound(musicName);
            var channel = music.Play();
            channel.Volume = _defaultMusicVolume;
            channel.Looping = looping;
            _currentMusic = music;
            _currentMusicChannel = channel;

            _loopPoint = loopTime;
        }

        public void PauseMusic()
        {
            _currentMusicChannel.Pause();
        }

        public void ResumeMusic()
        {
            _currentMusicChannel.Resume();
        }

        public void StopMusic()
        {
            _currentMusicChannel.Stop();
            _currentMusic = null;
            _loopPoint = 0;
        }

        public override void Update()
        {
            base.Update();

            if (_loopPoint > 0)
            {
                if (!_currentMusicChannel.IsPlaying)
                {
                    _currentMusicChannel = _currentMusic.Play();
                    _currentMusicChannel.TrackPosition = _loopPoint;
                }
            }
        }
    }
}
