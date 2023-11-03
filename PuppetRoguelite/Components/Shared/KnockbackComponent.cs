using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tweens;
using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    /// <summary>
    /// component for anything that can be knocked back by hits
    /// </summary>
    public class KnockbackComponent : Component, IUpdatable
    {
        public bool IsStunned;

        VelocityComponent _velocityComponent;
        Hurtbox _hurtbox;

        float _knockbackSpeed;
        float _knockbackDuration;
        int _hitsUntilImmune = 0;
        float _immunityDuration = 0f;
        int _hitCounter = 0;
        float _hitLifespan = 1.5f;
        bool _isImmune;

        FloatTween _tween;

        /// <summary>
        /// constructor with no immunity parameters
        /// </summary>
        /// <param name="knockbackSpeed"></param>
        /// <param name="knockbackDuration"></param>
        /// <param name="velocityComponent"></param>
        /// <param name="hurtbox"></param>
        public KnockbackComponent(float knockbackSpeed, float knockbackDuration, VelocityComponent velocityComponent, Hurtbox hurtbox)
        {
            _knockbackSpeed = knockbackSpeed;
            _knockbackDuration = knockbackDuration;
            _velocityComponent = velocityComponent;
            _hurtbox = hurtbox;
        }

        /// <summary>
        /// constructor that allows immunity after a certain number of hits
        /// </summary>
        /// <param name="knockbackSpeed"></param>
        /// <param name="knockbackDuration"></param>
        /// <param name="hitsUntilImmune"></param>
        /// <param name="immunityDuration"></param>
        /// <param name="velocityComponent"></param>
        /// <param name="hurtbox"></param>
        public KnockbackComponent(float knockbackSpeed, float knockbackDuration, int hitsUntilImmune, float immunityDuration, VelocityComponent velocityComponent, Hurtbox hurtbox)
        {
            _knockbackSpeed = knockbackSpeed;
            _knockbackDuration = knockbackDuration;
            _hitsUntilImmune = hitsUntilImmune;
            _immunityDuration = immunityDuration;
            _velocityComponent = velocityComponent;
            _hurtbox = hurtbox;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHurtboxHit);

            if (Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            _hurtbox.Emitter.RemoveObserver(HurtboxEventTypes.Hit, OnHurtboxHit);

            if (Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            }
        }

        public void Update()
        {
            if (IsStunned)
            {
                if (_tween != null)
                {
                    if (_tween.IsRunning())
                    {
                        _velocityComponent.Move();
                    }
                    else
                    {
                        _tween = null;
                        IsStunned = false;
                        return;
                    }
                }

                if (Entity.TryGetComponent<SpriteAnimator> (out var animator))
                {
                    if (animator.Animations.ContainsKey("Hit"))
                    {
                        if (animator.CurrentAnimationName != "Hit")
                        {
                            animator.Play("Hit");
                        }
                    }
                }
            }
        }

        void OnHurtboxHit(HurtboxHit hurtboxHit)
        {
            if (!_isImmune)
            {
                //if can become immune, increment hits
                if (_hitsUntilImmune > 0)
                {
                    _hitCounter += 1;
                    Game1.Schedule(_hitLifespan, timer => _hitCounter = Math.Max(0, _hitCounter - 1));

                    //if hits are greater or equal to hits until immune, become immune
                    if (_hitCounter >= _hitsUntilImmune)
                    {
                        _isImmune = true;
                        _hitCounter = 0;
                        Game1.Schedule(_immunityDuration, timer => _isImmune = false);
                        return;
                    }
                }

                IsStunned = true;

                //move in direction of Hitbox direction if it exists, otherwise just use collision normal
                var dir = hurtboxHit.Hitbox.Direction;
                if (dir == Vector2.Zero)
                {
                    dir = -hurtboxHit.CollisionResult.Normal;
                    dir.Normalize();
                }

                //start moving
                _velocityComponent.SetDirection(dir);
                _velocityComponent.Speed = _knockbackSpeed * hurtboxHit.Hitbox.PushForce;

                if (_tween != null)
                {
                    _tween.Stop();
                }
                _tween = new FloatTween(_velocityComponent, 0, _knockbackDuration);
                _tween.SetEaseType(EaseType.CubicOut);
                _tween.Start();
            }
        }

        void OnHealthDepleted(HealthComponent hc)
        {
            SetEnabled(false);
        }
    }
}
