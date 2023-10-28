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
                if (_renderer.FlipX != flip)
                {
                    Emitter.Emit(SpriteFlipperEvents.Flipped, flip);
                    _isOnCooldown = true;
                    Game1.Schedule(_cooldown, timer => _isOnCooldown = false);
                }
                _renderer.FlipX = flip;
            }
        }
    }

    public enum SpriteFlipperEvents
    {
        Flipped
    }
}
