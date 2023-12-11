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
        bool _isSturdy;

        ITween<float> _speedTween;
        float _speed = 0f;

        string _animationName;

        Status _status = new Status(Status.StatusType.Stunned, (int)StatusPriority.Stunned);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="knockbackSpeed"></param>
        /// <param name="knockbackDuration"></param>
        /// <param name="velocityComponent"></param>
        /// <param name="hurtbox"></param>
        public KnockbackComponent(float knockbackSpeed, float knockbackDuration, VelocityComponent velocityComponent, Hurtbox hurtbox, string animationName)
        {
            _knockbackSpeed = knockbackSpeed;
            _knockbackDuration = knockbackDuration;
            _velocityComponent = velocityComponent;
            _hurtbox = hurtbox;
            _animationName = animationName;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHurtboxHit);

            if (Entity.TryGetComponent<SturdyComponent>(out var sturdyComponent))
            {
                sturdyComponent.Emitter.AddObserver(SturdyComponentEvents.SturdyActivated, OnSturdyActivated);
                sturdyComponent.Emitter.AddObserver(SturdyComponentEvents.SturdyDeactivated, OnSturdyDeactivated);
            }

            _statusComponent = Entity.GetComponent<StatusComponent>();
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            _hurtbox.Emitter.RemoveObserver(HurtboxEventTypes.Hit, OnHurtboxHit);

            if (Entity.TryGetComponent<SturdyComponent>(out var sturdyComponent))
            {
                sturdyComponent.Emitter.RemoveObserver(SturdyComponentEvents.SturdyActivated, OnSturdyActivated);
                sturdyComponent.Emitter.RemoveObserver(SturdyComponentEvents.SturdyDeactivated, OnSturdyDeactivated);
            }

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

                //play a hit animation if the entity has a sprite animator with one
                if (Entity.TryGetComponent<SpriteAnimator> (out var animator))
                {
                    if (animator.Animations.ContainsKey(_animationName))
                    {
                        if (!animator.IsAnimationActive(_animationName))
                            animator.Play(_animationName);
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

            //only get stunned if not sturdy
            if (!_isSturdy)
            {
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

        void EndStun()
        {
            if (_speedTween != null)
            {
                if (_speedTween.IsRunning())
                    _speedTween.Stop();
            }

            _speedTween = null;

            IsStunned = false;
            if (Entity.TryGetComponent<StatusComponent>(out var statusComponent))
            {
                statusComponent.PopStatus(_status);
            }
        }

        void OnTweenFinished(ITween<float> tween)
        {
            EndStun();
        }

        void OnSturdyActivated()
        {
            _isSturdy = true;

            if (IsStunned)
                EndStun();
        }

        void OnSturdyDeactivated()
        {
            _isSturdy = false;
        }
    }
}
