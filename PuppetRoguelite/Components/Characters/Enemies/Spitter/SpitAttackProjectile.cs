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

namespace PuppetRoguelite.Components.Characters.Enemies.Spitter
{
    public class SpitAttackProjectile : Component, IUpdatable
    {
        //stats
        int _damage = 1;
        float _speed = 210f;
        float _radius = 3f;

        //components
        public SpriteAnimator Animator;
        public ProjectileMover Mover;
        public CircleHitbox Hitbox;
        public SpriteTrail Trail;

        //misc
        Vector2 _direction;
        bool _isBursting = false;
        float _timeSinceLaunched = 0; //TODO: REMOVE THIS CRAP
        float _maxTime = 10;

        public SpitAttackProjectile(Vector2 direction)
        {
            _direction = direction;
        }

        public override void Initialize()
        {
            base.Initialize();

            Animator = Entity.AddComponent(new SpriteAnimator());

            Mover = Entity.AddComponent(new ProjectileMover());

            Hitbox = Entity.AddComponent(new CircleHitbox(_damage, _radius));
            Flags.SetFlagExclusive(ref Hitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref Hitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            Hitbox.PushForce = 1f;

            Trail = Entity.AddComponent(new SpriteTrail(Animator));
            Trail.FadeDelay = 0;
            Trail.FadeDuration = .2f;
            Trail.MinDistanceBetweenInstances = 20f;
            Trail.InitialColor = Color.White * .5f;

            var texture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Spitter.Spitter_projectile);
            var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);
            Animator.AddAnimation("Travel", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 0, 4, 7));
            Animator.AddAnimation("Burst_1", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 1, 7, 7));
            Animator.AddAnimation("Burst_2", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 2, 6, 7));
        }

        public void Update()
        {
            if (!_isBursting)
            {
                _timeSinceLaunched += Time.DeltaTime;
                if (_timeSinceLaunched >= _maxTime) Entity.Destroy();

                if (Mover.Move(_direction * _speed * Time.DeltaTime))
                {
                    _isBursting = true;
                    Animator.Play("Burst_1", SpriteAnimator.LoopMode.Once);
                    Animator.OnAnimationCompletedEvent += OnBurstAnimationCompleted;
                }
                else if (Animator.CurrentAnimationName != "Travel")
                {
                    Animator.Play("Travel");
                }
            }
        }

        void OnBurstAnimationCompleted(string animationName)
        {
            Animator.OnAnimationCompletedEvent -= OnBurstAnimationCompleted;
            Entity.Destroy();
        }
    }
}
