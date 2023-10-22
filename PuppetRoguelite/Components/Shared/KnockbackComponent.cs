using Microsoft.Xna.Framework;
using Nez;
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

        FloatTween _tween;

        public KnockbackComponent(float knockbackSpeed, float knockbackDuration, VelocityComponent velocityComponent, Hurtbox hurtbox)
        {
            _knockbackSpeed = knockbackSpeed;
            _knockbackDuration = knockbackDuration;
            _velocityComponent = velocityComponent;
            _hurtbox = hurtbox;
        }

        public override void Initialize()
        {
            base.Initialize();

            _hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHurtboxHit);
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
                    }
                }
            }
        }

        void OnHurtboxHit(HurtboxHit hurtboxHit)
        {
            IsStunned = true;

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
}
