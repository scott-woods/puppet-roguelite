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
        StatusComponent _statusComponent;

        float _knockbackSpeed;
        float _knockbackDuration;
        int _hitsUntilImmune = 0;
        float _immunityDuration = 0f;
        int _hitCounter = 0;
        float _hitLifespan = 1.5f;
        bool _isImmune;

        ITween<float> _speedTween;
        float _speed = 0f;

        Status _status = new Status(Status.StatusType.Stunned, (int)StatusPriority.Stunned);

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

            _statusComponent = Entity.GetComponent<StatusComponent>();
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            _hurtbox.Emitter.RemoveObserver(HurtboxEventTypes.Hit, OnHurtboxHit);

            _speedTween?.Stop();
        }

        public void Update()
        {
            if (_statusComponent != null)
            {
                //if affected by a greater priority status, do nothing
                if (_statusComponent.CurrentStatus.Priority > _status.Priority)
                {
                    return;
                }
            }

            //if stunned
            if (IsStunned)
            {
                //move at current speed
                _velocityComponent.Move(_speed);

                ////if speed tween is not null
                //if (_speedTween != null)
                //{
                //    //if speed tween is still running, continue to move
                //    if (_speedTween.IsRunning())
                //    {
                //        _velocityComponent.Move(_speed);
                //    }
                //    else //tween finished, end stun
                //    {
                //        _speedTween = null;
                //        IsStunned = false;
                //        if (Entity.TryGetComponent<StatusComponent>(out var statusComponent))
                //        {
                //            statusComponent.PopStatus(_status);
                //        }
                //        return;
                //    }
                //}

                //play a hit animation if the entity has a sprite animator with one
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
            //if affected by a higher priority status, do nothing
            if (_statusComponent != null)
            {
                if (_statusComponent.CurrentStatus.Priority > _status.Priority)
                {
                    return;
                }
            }
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

                //set stunned to true
                IsStunned = true;

                //update status component
                if (Entity.TryGetComponent<StatusComponent>(out var statusComponent))
                {
                    statusComponent.PushStatus(_status);
                }

                //move in direction of Hitbox direction if it exists, otherwise just use collision normal
                var dir = hurtboxHit.Hitbox.Direction;
                if (dir == Vector2.Zero)
                {
                    dir = -hurtboxHit.CollisionResult.Normal;
                    dir.Normalize();
                }

                //start moving
                _velocityComponent.SetDirection(dir);
                _speed = _knockbackSpeed * hurtboxHit.Hitbox.PushForce;

                //var test = this.Tween("_speed", 0, _knockbackDuration);
                //test.SetEaseType(EaseType.CubicOut);
                //test.Start();

                //if tween not null, one is already in progress. cancel that one and start over
                if (_speedTween != null)
                {
                    _speedTween.Stop();
                    _speedTween.SetCompletionHandler(null);
                    _speedTween = null;
                }
                _speedTween = this.Tween("_speed", 0f, _knockbackDuration);
                _speedTween.SetEaseType(EaseType.CubicOut);
                _speedTween.Start();
                _speedTween.SetCompletionHandler(OnTweenFinished);
            }
        }

        void OnTweenFinished(ITween<float> tween)
        {
            _speedTween = null;
            IsStunned = false;
            if (Entity.TryGetComponent<StatusComponent>(out var statusComponent))
            {
                statusComponent.PopStatus(_status);
            }
        }
    }
}
