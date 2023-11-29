﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class SpriteFlipper : Component, IUpdatable
    {
        public Emitter<SpriteFlipperEvents, bool> Emitter = new Emitter<SpriteFlipperEvents, bool>();

        SpriteRenderer _renderer;
        VelocityComponent _velocityComponent;
        float _cooldown;
        bool _isOnCooldown = false;

        public SpriteFlipper(SpriteRenderer renderer, VelocityComponent velocityComponent, float cooldown = .2f)
        {
            _renderer = renderer;
            _velocityComponent = velocityComponent;
            _cooldown = cooldown;
        }

        public void Update()
        {
            if (!_isOnCooldown)
            {
                var dir = _velocityComponent.Direction;
                var flip = dir.X < 0;

                //if facing a different direction than previous frame, emit flipped signal
                if (_renderer.FlipX != flip)
                {
                    Emitter.Emit(SpriteFlipperEvents.Flipped, flip);
                    _isOnCooldown = true;
                    Game1.Schedule(_cooldown, timer => _isOnCooldown = false);

                    //flip renderer
                    _renderer.FlipX = flip;

                    //adjust offset
                    var newOffsetX = _renderer.LocalOffset.X * -1;
                    var newOffset = new Vector2(newOffsetX, _renderer.LocalOffset.Y);
                    _renderer.SetLocalOffset(newOffset);
                }
            }
        }
    }

    public enum SpriteFlipperEvents
    {
        Flipped
    }
}
