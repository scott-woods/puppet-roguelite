using FmodForFoxes;
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
        const float _defaultVolume = 1f;
        const float _defaultPitch = 0f;
        const float _defaultPan = 0;

        uint _loopPoint;

        Sound _currentMusic;
        Channel _currentMusicChannel;

        public Channel PlaySound(string soundName)
        {
            var sound = CoreSystem.LoadSound(soundName);
            var channel = sound.Play();

            return channel;
        }

        public void PlayMusic(string musicName, bool looping = true, uint loopTime = 0)
        {
            var music = CoreSystem.LoadStreamedSound(musicName);
            var channel = music.Play();
            channel.Looping = looping;
            _currentMusic = music;
            _currentMusicChannel = channel;

            _loopPoint = loopTime;
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
