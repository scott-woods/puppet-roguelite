using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Models;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Effects
{
    public class HitEffectComponent : Component
    {
        //components
        SpriteAnimator _animator;

        HitEffect _effect;
        Color _color;

        public HitEffectComponent(HitEffect effect, Color? color = null)
        {
            _effect = effect;

            _color = color == null ? Color.White : color.Value;
        }

        public override void Initialize()
        {
            base.Initialize();

            _animator = Entity.AddComponent(new SpriteAnimator());
            _animator.SetColor(_color);
            var texture = Entity.Scene.Content.LoadTexture(_effect.AnimationPath);
            var sprites = Sprite.SpritesFromAtlas(texture, _effect.CellWidth, _effect.CellHeight);
            _animator.AddAnimation("Hit", sprites.ToArray(), 13);
            _animator.OnAnimationCompletedEvent += OnAnimationCompleted;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _animator.Play("Hit", SpriteAnimator.LoopMode.Once);
        }

        void OnAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnAnimationCompleted;
            _animator.SetEnabled(false);

            Entity.Destroy();
        }
    }
}
