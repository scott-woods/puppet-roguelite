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

        public HitEffectComponent(HitEffect effect)
        {
            _effect = effect;
        }

        public override void Initialize()
        {
            base.Initialize();

            _animator = Entity.AddComponent(new SpriteAnimator());
            var texture = Entity.Scene.Content.LoadTexture(_effect.AnimationPath);
            var sprites = Sprite.SpritesFromAtlas(texture, _effect.CellWidth, _effect.CellHeight);
            _animator.AddAnimation("Hit", sprites.ToArray());
            _animator.OnAnimationCompletedEvent += OnAnimationCompleted;
        }

        public void PlayAnimation()
        {
            _animator.Play("Hit", SpriteAnimator.LoopMode.Once);
        }

        void OnAnimationCompleted(string animationName)
        {
            Entity.Destroy();
        }
    }
}
