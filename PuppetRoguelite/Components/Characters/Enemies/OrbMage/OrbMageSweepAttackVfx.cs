using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies.OrbMage
{
    public class OrbMageSweepAttackVfx : Component, IUpdatable
    {
        //constants
        const int _damage = 2;
        const int _hitboxActiveFrame = 0;
        const int _offset = 44;

        //components
        SpriteAnimator _animator;
        BoxHitbox _hitbox;

        //misc
        bool _hasPlayedSound = false;
        Vector2 _directionToPlayer;

        public OrbMageSweepAttackVfx(Vector2 directionToPlayer)
        {
            _directionToPlayer = directionToPlayer;
        }

        public override void Initialize()
        {
            base.Initialize();

            _animator = Entity.AddComponent(new SpriteAnimator());
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.VFXforSweep);
            var sprites = Sprite.SpritesFromAtlas(texture, 87, 34);
            _animator.AddAnimation("Attack", sprites.ToArray());

            _hitbox = Entity.AddComponent(new BoxHitbox(_damage, new Rectangle(-22, 6, 62, 11)));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _hitbox.SetEnabled(false);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (_directionToPlayer.X < 0)
                _animator.FlipY = true;

            Entity.Position += (_offset * _directionToPlayer);
            Entity.SetRotation((float)Math.Atan2(_directionToPlayer.Y, _directionToPlayer.X));

            _animator.Play("Attack", SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAnimationCompleted;
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
            {
                _hitbox.SetEnabled(false);
            }
        }

        void OnAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnAnimationCompleted;

            _hitbox.SetEnabled(false);
            _animator.SetEnabled(false);

            Entity.Destroy();
        }
    }
}
