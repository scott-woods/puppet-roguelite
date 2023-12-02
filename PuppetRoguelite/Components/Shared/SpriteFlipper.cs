using Microsoft.Xna.Framework;
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
        List<SpriteRenderer> _renderers;
        List<RenderableComponent> _otherRenderableComponents;
        VelocityComponent _velocityComponent;
        float _cooldown;
        bool _isOnCooldown = false;
        bool _flipped = false;

        public SpriteFlipper(List<SpriteRenderer> renderers, VelocityComponent velocityComponent, float cooldown = .2f, List<RenderableComponent> otherRenderableComponents = null)
        {
            _renderers = renderers;
            _velocityComponent = velocityComponent;
            _cooldown = cooldown;
            _otherRenderableComponents = otherRenderableComponents;
        }

        public SpriteFlipper(SpriteRenderer renderer, VelocityComponent velocityComponent, float cooldown = .2f) : this(new List<SpriteRenderer>() { renderer }, velocityComponent, cooldown)
        {
        }

        public void Update()
        {
            if (!_isOnCooldown)
            {
                var dir = _velocityComponent.Direction;
                var flip = dir.X < 0;

                foreach (var renderer in _renderers)
                {
                    //if facing a different direction than previous frame, emit flipped signal
                    if (renderer.FlipX != flip)
                    {
                        _isOnCooldown = true;
                        Game1.Schedule(_cooldown, timer => _isOnCooldown = false);

                        //flip renderer
                        renderer.FlipX = flip;

                        //adjust offset
                        var newOffsetX = renderer.LocalOffset.X * -1;
                        var newOffset = new Vector2(newOffsetX, renderer.LocalOffset.Y);
                        renderer.SetLocalOffset(newOffset);
                    }
                }

                if (_flipped != flip)
                {
                    foreach (var renderable in _otherRenderableComponents)
                    {
                        //adjust offset
                        var newOffsetX = renderable.LocalOffset.X * -1;
                        var newOffset = new Vector2(newOffsetX, renderable.LocalOffset.Y);
                        renderable.SetLocalOffset(newOffset);
                    }
                }

                _flipped = flip;
            }
        }
    }
}
