using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Textures;

namespace PuppetRoguelite.Components.Shared
{
    public class NewHealthbar : RenderableComponent
    {
        public override float Width => _width;
        public override float Height => _height;

        HealthComponent _healthComponent;
        int _width = 32;
        int _height = 16;

        ITimer _visibleTimer;
        float _secondsVisible = 1f;

        Sprite _healthBarBackground;
        Sprite _healthBarFill;

        public NewHealthbar(HealthComponent healthComponent, int width = 32)
        {
            _healthComponent = healthComponent;
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnHealthChanged);

            _width = width;
        }

        public override void Initialize()
        {
            base.Initialize();

            Color = Color.Transparent;

            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Healthbar);
            _healthBarBackground = new Sprite(texture, new Rectangle(0, 0, 48, 16));
            _healthBarFill = new Sprite(texture, new Rectangle(0, 16, 48, 16));
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var healthPercentage = (float)_healthComponent.Health / (float)_healthComponent.MaxHealth;
            var healthBarWidth = _width * healthPercentage;
            var position = Entity.Position + _localOffset - new Vector2(_width / 2, 0);

            var fillDestRect = new Rectangle((int)position.X, (int)position.Y, (int)healthBarWidth, _height);
            var fillSourceRect = new Rectangle(16, 16, 16, 16);
            batcher.Draw(_healthBarFill.Texture2D, fillDestRect, fillSourceRect, Color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);

            var leftEdgeDestRect = new Rectangle((int)position.X - 16, (int)position.Y, 16, 16);
            var leftEdgeSourceRect = new Rectangle(0, 0, 16, 16);
            batcher.Draw(_healthBarBackground.Texture2D, leftEdgeDestRect, leftEdgeSourceRect, Color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);

            var backgroundDestRect = new Rectangle((int)position.X, (int)position.Y, _width, _height);
            var backgroundSourceRect = new Rectangle(16, 0, 16, 16);
            batcher.Draw(_healthBarBackground.Texture2D, backgroundDestRect, backgroundSourceRect, Color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);

            var rightEdgeDestRect = new Rectangle((int)position.X + _width, (int)position.Y, 16, 16);
            var rightEdgeSourceRect = new Rectangle(32, 0, 16, 16);
            batcher.Draw(_healthBarBackground.Texture2D, rightEdgeDestRect, rightEdgeSourceRect, Color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);
        }

        void OnHealthChanged(HealthComponent healthComponent)
        {
            Color = Color.White;

            if (_visibleTimer != null)
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
