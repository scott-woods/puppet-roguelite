﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class FreezeComponent : Component
    {
        float _duration;

        //components
        SpriteAnimator _spriteAnimator;
        StatusComponent _statusComponent;

        Status _status = new Status(Status.StatusType.Stunned, (int)StatusPriority.Stunned);

        public FreezeComponent(float duration)
        {
            _duration = duration;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _statusComponent = Entity.GetComponent<StatusComponent>();
            _statusComponent?.PushStatus(_status);

            _spriteAnimator = Entity.GetComponent<SpriteAnimator>();
            if (_spriteAnimator != null)
            {
                var lastSprite = _spriteAnimator.CurrentAnimation.Sprites[_spriteAnimator.CurrentFrame];
                _spriteAnimator.SetColor(Color.LightBlue);
                _spriteAnimator.Stop();
                _spriteAnimator.SetSprite(lastSprite);
            }

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Freeze);

            Game1.Schedule(_duration, Unfreeze);
        }

        void Unfreeze(ITimer timer)
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Unfreeze);

            _statusComponent?.PopStatus(_status);
            _spriteAnimator?.SetColor(Color.White);

            Entity.RemoveComponent(this);
        }
    }
}