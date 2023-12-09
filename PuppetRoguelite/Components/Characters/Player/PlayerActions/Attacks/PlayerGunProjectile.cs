using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    public class PlayerGunProjectile : Component, IUpdatable
    {
        //constants
        const int _damage = 3;
        const float _speed = 450f;
        const float _maxTime = 10f;

        //components
        SpriteAnimator _animator;
        CircleHitbox _hitbox;
        ProjectileMover _mover;
        SpriteTrail _trail;

        //params
        Vector2 _direction;
        bool _simulation;

        //misc
        bool _isBursting;
        float _timeSinceLaunched = 0f;

        public PlayerGunProjectile(Vector2 direction, bool simulation)
        {
            _direction = direction;
            _simulation = simulation;
        }

        public override void Initialize()
        {
            base.Initialize();

            _hitbox = Entity.AddComponent(new CircleHitbox(_damage, 3));
            if (_simulation)
                _hitbox.PhysicsLayer = 0;
            else
                Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            _hitbox.CollidesWithLayers = 0;
            Flags.SetFlag(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlag(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.Environment);

            _mover = Entity.AddComponent(new ProjectileMover());

            _animator = Entity.AddComponent(new SpriteAnimator());
            if (_simulation)
                _animator.SetColor(new Color(255, 255, 255, 128));
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Player_gun_projectile);
            var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);
            _animator.AddAnimation("Travel", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 0, 1));
            _animator.AddAnimation("Burst", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 4, 7));

            _trail = Entity.AddComponent(new SpriteTrail(_animator));
            _trail.MinDistanceBetweenInstances = 16;
            _trail.FadeDelay = .1f;
            _trail.FadeDuration = .2f;
        }

        public void Update()
        {
            if (!_isBursting)
            {
                //increment timer. if past max time, destroy
                _timeSinceLaunched += Time.DeltaTime;
                if (_timeSinceLaunched >= _maxTime)
                {
                    Burst();
                    return;
                }

                if (_mover.Move(_direction * _speed * Time.DeltaTime))
                {
                    Burst();
                    return;
                }
                if (_animator.CurrentAnimationName != "Travel")
                {
                    _animator.Play("Travel");
                }
            }
        }

        void Burst()
        {
            _isBursting = true;
            _animator.Play("Burst", SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnBurstAnimationCompleted;
        }

        void OnBurstAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnBurstAnimationCompleted;
            _animator.SetEnabled(false);
            Entity.Destroy();
        }
    }
}
