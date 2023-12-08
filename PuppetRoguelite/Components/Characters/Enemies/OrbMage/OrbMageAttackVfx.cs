using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies.OrbMage
{
    public class OrbMageAttackVfx : Component, IUpdatable
    {
        //constants
        const int _damage = 2;
        const int _hitboxActiveFrame = 0;
        const float _delay = .25f;

        //components
        SpriteAnimator _animator;
        BoxHitbox _hitbox;
        YSorter _ySorter;

        //misc
        bool _hasPlayedSound = false;

        public override void Initialize()
        {
            base.Initialize();

            _hitbox = Entity.AddComponent(new BoxHitbox(_damage, new Rectangle(-3, -14, 7, 28)));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _hitbox.SetEnabled(false);

            _animator = Entity.AddComponent(new SpriteAnimator());
            _animator.SetLocalOffset(new Vector2(5, 0));
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Attackvfx);
            var sprites = Sprite.SpritesFromAtlas(texture, 119, 34);
            _animator.AddAnimation("Telegraph", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 5, 4, 3 }, true));
            _animator.AddAnimation("Attack", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 0, 1, 2, 3, 4, 5 }, true));

            var origin = Entity.AddComponent(new OriginComponent(_hitbox));
            _ySorter = Entity.AddComponent(new YSorter(_animator, origin));

            var shadow = Entity.AddComponent(new Shadow(_animator, new Vector2(0, 17), Vector2.One));
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Orb_mage_telegraph);
            _animator.Play("Telegraph", SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnTelegraphCompleted;
        }

        public void Update()
        {
            if (_animator.IsAnimationActive("Attack") && _animator.CurrentFrame == _hitboxActiveFrame)
            {
                _hitbox.SetEnabled(true);
                if (!_hasPlayedSound)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Orb_mage_attack);
                    _hasPlayedSound = true;
                }
            }
            else
                _hitbox.SetEnabled(false);
        }

        void OnTelegraphCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnTelegraphCompleted;

            _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
            Game1.Schedule(_delay, OnTimerFinished);
        }

        void OnTimerFinished(ITimer timer)
        {
            _animator.Play("Attack", SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAttackFinished;
        }

        void OnAttackFinished(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnAttackFinished;

            _animator.SetEnabled(false);
            _hitbox.SetEnabled(false);
            Entity.Destroy();
        }
    }
}
