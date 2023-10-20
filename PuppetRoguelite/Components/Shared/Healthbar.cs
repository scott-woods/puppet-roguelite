using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Healthbar : RenderableComponent
    {
        public override float Width
        {
            get
            {
                return _totalWidth;
            }
        }
        public override float Height
        {
            get
            {
                return 16;
            }
        }

        HealthComponent _healthComponent;

        Sprite _leftEdge, _rightEdge, _zeroBars, _oneBar, _twoBars, _threeBars, _fourBars;
        int _totalSpaces = 0;
        int _totalWidth = 0;

        ITimer _visibleTimer;
        float _secondsVisible = 1f;

        public Healthbar(HealthComponent healthComponent)
        {
            _healthComponent = healthComponent;
        }

        public override void Initialize()
        {
            base.Initialize();

            Color = Color.Transparent;

            //load texture and sprites
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Borders_and_hp);
            var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);
            _leftEdge = sprites[266];
            _rightEdge = sprites[269];
            _fourBars = sprites[267];
            _threeBars = sprites[288];
            _twoBars = sprites[309];
            _oneBar = sprites[330];
            _zeroBars = sprites[268];

            //determine size
            _totalSpaces = (int)Math.Ceiling((double)_healthComponent.MaxHealth / 4) + 2;
            _totalWidth = _totalSpaces * 16;

            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnHealthChanged);
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var center = Entity.Transform.Position + LocalOffset;
            var scale = Entity.Transform.Scale;

            //draw left edge
            batcher.Draw(_leftEdge, center + new Vector2(-(_totalWidth / 2), -8), Color,
                Entity.Transform.Rotation, new Vector2(0, 0), scale, SpriteEffects.None, _layerDepth);

            //draw middle
            for (int i = 0; i < _totalSpaces - 2; i++)
            {
                var sprite = GetBarSprite(i);
                batcher.Draw(sprite, center + new Vector2(-(_totalWidth / 2) + 16 + (i * 16), -8), Color,
                    Entity.Transform.Rotation, new Vector2(0, 0), scale, SpriteEffects.None, _layerDepth);
            }

            //draw right edge
            batcher.Draw(_rightEdge, center + new Vector2((_totalWidth / 2) - 16, -8), Color,
                Entity.Transform.Rotation, new Vector2(0, 0), scale, SpriteEffects.None, _layerDepth);
        }

        Sprite GetBarSprite(int spaceIndex)
        {
            int remainingHealth = _healthComponent.Health - (spaceIndex * 4);
            if (remainingHealth >= 4)
            {
                return _fourBars;  // Four bars
            }
            else if (remainingHealth == 3)
            {
                return _threeBars;  // Three bars
            }
            else if (remainingHealth == 2)
            {
                return _twoBars;  // Two bars
            }
            else if (remainingHealth == 1)
            {
                return _oneBar;  // One bar
            }
            else
            {
                return _zeroBars;  // Zero bars
            }
        }

        void OnHealthChanged(HealthComponent healthComponent)
        {
            Color = Color.White;

            if (_visibleTimer != null )
            {
                _visibleTimer.Reset();
            }
            _visibleTimer = Game1.Schedule(_secondsVisible, timer =>
            {
                TweenExt.TweenColorTo(this, Color.Transparent, 3f).Start();
            });
        }
    }
}
