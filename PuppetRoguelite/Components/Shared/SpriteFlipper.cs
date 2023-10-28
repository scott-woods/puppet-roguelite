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

        public SpriteFlipper(SpriteRenderer renderer, VelocityComponent velocityComponent)
        {
            _renderer = renderer;
            _velocityComponent = velocityComponent;
        }

        public void Update()
        {
            var dir = _velocityComponent.Direction;
            var flip = dir.X < 0;
            if (_renderer.FlipX != flip)
            {
                Emitter.Emit(SpriteFlipperEvents.Flipped, flip);
            }
            _renderer.FlipX = flip;
        }
    }

    public enum SpriteFlipperEvents
    {
        Flipped
    }
}
