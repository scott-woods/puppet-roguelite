using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tweens;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class Dash : Component, IUpdatable
    {
        const float _dashSpeed = 350f;
        const float _dashDuration = .2f;
        const float _shortDashCooldown = .025f;
        const float _dashCooldown = .75f;
        const float _successionLifespan = .75f;
        const int _maxSuccession = 2;

        public bool IsOnCooldown = false;

        Action _dashCompleteCallback;
        VelocityComponent _velocityComponent;
        SpriteAnimator _spriteAnimator;
        SpriteTrail _spriteTrail;

        bool _isDashing = false;
        float _dashTimer = 0f;
        int _successionCount = 0;

        public void ExecuteDash(Action dashCompleteCallback, SpriteAnimator animator, VelocityComponent velocityComponent,
            SpriteTrail spriteTrail)
        {
            _dashCompleteCallback = dashCompleteCallback;
            _velocityComponent = velocityComponent;
            _spriteAnimator = animator;
            _spriteTrail = spriteTrail;

            _isDashing = true;
            _dashTimer = _dashDuration;
            _successionCount += 1;
            Game1.Schedule(_successionLifespan, timer => _successionCount -= 1);

            _velocityComponent.Speed = _dashSpeed;

            //configure trail
            _spriteTrail.FadeDelay = 0;
            _spriteTrail.FadeDuration = .2f;
            _spriteTrail.MinDistanceBetweenInstances = 20f;
            _spriteTrail.InitialColor = Color.White * .5f;
            _spriteTrail.EnableSpriteTrail();

            //animation
            var animation = "DashRight";
            if (_velocityComponent.Direction.X != 0)
            {
                animation = _velocityComponent.Direction.X > 0 ? "DashRight" : "DashLeft";
            }
            else if (_velocityComponent.Direction.Y != 0)
            {
                animation = _velocityComponent.Direction.Y > 0 ? "DashDown" : "DashUp";
            }

            _spriteAnimator.Color = Color.White * .8f;
            _spriteAnimator.Play(animation);
        }

        public void Update()
        {
            if (_isDashing)
            {
                _dashTimer -= Time.DeltaTime;
                if (_dashTimer > 0f)
                {
                    _velocityComponent.Move();
                }
                else
                {
                    var cooldown = _successionCount >= _maxSuccession ? _dashCooldown : _shortDashCooldown;

                    IsOnCooldown = true;
                    Game1.Schedule(cooldown, timer => IsOnCooldown = false);

                    _spriteAnimator.Color = Color.White;
                    _spriteTrail.DisableSpriteTrail();
                    _isDashing = false;
                    _dashCompleteCallback?.Invoke();
                }
            }
        }
    }
}
