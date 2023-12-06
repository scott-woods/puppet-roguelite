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
using Nez.Tweens;
using System.Collections;

namespace PuppetRoguelite.Components.Shared
{
    public class Healthbar : RenderableComponent
    {
        public override float Width => _width;
        public override float Height => _height;

        HealthComponent _healthComponent;
        int _width = 32;
        int _height = 16;

        ITimer _visibleTimer;
        ITween<Color> _fadeTween;
        float _secondsVisible = 1f;
        ICoroutine _hideHealthbarCoroutine;
        Color _color;

        Sprite _healthBarBackground;
        Sprite _healthBarFill;

        public Healthbar(HealthComponent healthComponent, int width = 32)
        {
            _healthComponent = healthComponent;

            _width = width;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnHealthChanged);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            _healthComponent.Emitter.RemoveObserver(HealthComponentEventType.HealthChanged, OnHealthChanged);
        }

        public override void Initialize()
        {
            base.Initialize();

            _color = Color.Transparent;

            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Healthbar);
            _healthBarBackground = new Sprite(texture, new Rectangle(0, 0, 48, 16));
            _healthBarFill = new Sprite(texture, new Rectangle(0, 16, 48, 16));
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (Entity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                RenderLayer = animator.RenderLayer;
            }

            var healthPercentage = (float)_healthComponent.Health / (float)_healthComponent.MaxHealth;
            var healthBarWidth = _width * healthPercentage;
            var position = Entity.Position + _localOffset - new Vector2(_width / 2, 0);

            var fillDestRect = new Rectangle((int)position.X, (int)position.Y, (int)healthBarWidth, _height);
            var fillSourceRect = new Rectangle(16, 16, 16, 16);
            batcher.Draw(_healthBarFill.Texture2D, fillDestRect, fillSourceRect, _color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);

            var leftEdgeDestRect = new Rectangle((int)position.X - 16, (int)position.Y, 16, 16);
            var leftEdgeSourceRect = new Rectangle(0, 0, 16, 16);
            batcher.Draw(_healthBarBackground.Texture2D, leftEdgeDestRect, leftEdgeSourceRect, _color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);

            var backgroundDestRect = new Rectangle((int)position.X, (int)position.Y, _width, _height);
            var backgroundSourceRect = new Rectangle(16, 0, 16, 16);
            batcher.Draw(_healthBarBackground.Texture2D, backgroundDestRect, backgroundSourceRect, _color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);

            var rightEdgeDestRect = new Rectangle((int)position.X + _width, (int)position.Y, 16, 16);
            var rightEdgeSourceRect = new Rectangle(32, 0, 16, 16);
            batcher.Draw(_healthBarBackground.Texture2D, rightEdgeDestRect, rightEdgeSourceRect, _color,
                Entity.Transform.Rotation, new Vector2(0, 0), SpriteEffects.None, _layerDepth);
        }

        void OnHealthChanged(HealthComponent healthComponent)
        {
            _color = Color.White;

            if (_hideHealthbarCoroutine != null )
            {
                _hideHealthbarCoroutine.Stop();
            }

            _hideHealthbarCoroutine = Game1.StartCoroutine(HideHealthbarAfterDelay());

            //_visibleTimer?.Stop();
            //_fadeTween?.Stop();

            //_visibleTimer = Game1.Schedule(3f, timer =>
            //{
            //    Debug.Log("test");
            //    _fadeTween = _color.
            //    _fadeTween = this.TweenColorTo(Color.Transparent, 1f);
            //    _fadeTween.SetCompletionHandler(tween =>
            //    {
            //        Debug.Log("tween completed");
            //    });
            //    _fadeTween.Start();
            //});

            //_fadeTween?.Stop(true);

            //if (_hideHealthbarCoroutine != null)
            //{
            //    _hideHealthbarCoroutine.Stop();
            //    _hideHealthbarCoroutine = null;
            //}
            //_hideHealthbarCoroutine = Game1.StartCoroutine(HideHealthbarAfterDelay());

            //_fadeTween?.Stop();

            //if (_visibleTimer != null)
            //{
            //    _visibleTimer.Reset();
            //}
            //_visibleTimer = Game1.Schedule(_secondsVisible, timer =>
            //{
            //    _fadeTween = this.TweenColorTo(Color.Transparent, 3f);
            //    _fadeTween.Start();
            //});
        }

        IEnumerator HideHealthbarAfterDelay()
        {
            //yield return Coroutine.WaitForSeconds(2f);
            var elapsed = 0f;
            while (elapsed < 10f)
            {
                //Debug.Log(elapsed);
                //Debug.Log(_color);
                elapsed += Time.DeltaTime;
                _color = Lerps.Ease(EaseType.Linear, Color.White, Color.Transparent, elapsed, 10);

                yield return null;
            }
        }
    }
}
