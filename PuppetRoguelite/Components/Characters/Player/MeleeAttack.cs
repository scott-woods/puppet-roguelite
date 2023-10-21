using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tweens;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class MeleeAttack : Component, IUpdatable
    {
        const float _comboThreshold = .5f;
        const float _maxCombo = 2;
        const float _minComboThreshold = .2f;
        const float _offset = 8;
        const float _attackMoveSpeed = 65f;

        int _comboCounter = 0;
        float _timeSinceLastClick = 0f;
        float _timeSinceAttackStarted = 0f;
        bool _isAttacking = false;
        bool _continueCombo = false;
        int _activeFrame = -1;

        Action _attackCompleteCallback;

        //components from parent
        SpriteAnimator _animator;
        VelocityComponent _velocityComponent;

        //added components
        Hitbox _hitbox;

        public MeleeAttack(SpriteAnimator animator, VelocityComponent velocityComponent)
        {
            _animator = animator;
            _velocityComponent = velocityComponent;
        }

        public override void Initialize()
        {
            base.Initialize();

            var hitboxCollider = Entity.AddComponent(new CircleCollider(8));
            Flags.SetFlagExclusive(ref hitboxCollider.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref hitboxCollider.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            hitboxCollider.IsTrigger = true;
            _hitbox = Entity.AddComponent(new Hitbox(hitboxCollider, 1));
            _hitbox.SetEnabled(false);

            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);
            _animator.AddAnimation("MeleeRight_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 1, 4));
            _animator.AddAnimation("MeleeRight_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 5, 7));
            _animator.AddAnimation("MeleeLeft_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 10, 13));
            _animator.AddAnimation("MeleeLeft_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 14, 16));
            _animator.AddAnimation("MeleeDown_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 19, 22));
            _animator.AddAnimation("MeleeDown_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 23, 25));
            _animator.AddAnimation("MeleeUp_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 28, 31));
            _animator.AddAnimation("MeleeUp_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 32, 34));
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            _hitbox.SetEnabled(false);
        }

        public void Update()
        {
            if (_isAttacking)
            {
                //increment timers
                _timeSinceLastClick += Time.DeltaTime;
                _timeSinceAttackStarted += Time.DeltaTime;

                //spam clicking for combo
                if (Input.LeftMouseButtonPressed && _timeSinceAttackStarted >= _minComboThreshold && _timeSinceLastClick <= _comboThreshold)
                {
                    _timeSinceLastClick = 0f;
                    _continueCombo = true;
                }

                //active hitbox if on active frame
                _hitbox.SetEnabled(_animator.CurrentFrame == _activeFrame);

                //move
                _velocityComponent.Move();
            }
        }

        public void ExecuteAttack(Action attackCompleteCallback)
        {
            _attackCompleteCallback = attackCompleteCallback;

            _comboCounter = 0;
            _timeSinceLastClick = 0f;
            _timeSinceAttackStarted = 0f;

            _isAttacking = true;

            PerformAttack();
        }

        void PerformAttack()
        {
            //increment combo
            _comboCounter += 1;

            //determine direction to mouse
            var dir = (Entity.Scene.Camera.MouseToWorldPoint() - Entity.Position);
            dir.Normalize();

            //set hitbox position
            var hitboxPosition = Entity.Position + (dir * _offset);
            _hitbox.Collider.SetLocalOffset(hitboxPosition - Entity.Position);
            _hitbox.SetEnabled(true);

            //get angle in degrees
            var angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(Entity.Position, hitboxPosition));
            angle = (angle + 360) % 360;

            //determine animation by angle
            var animation = $"MeleeRight_{_comboCounter}";
            if (angle >= 45 && angle < 135) animation = $"MeleeDown_{_comboCounter}";
            else if (angle >= 135 && angle < 225) animation = $"MeleeLeft_{_comboCounter}";
            else if (angle >= 225 && angle < 315) animation = $"MeleeUp_{_comboCounter}";

            //play animation
            _animator.Play(animation, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAttackAnimationCompleted;

            //start moving
            _velocityComponent.SetDirection(dir);
            _velocityComponent.Speed = _attackMoveSpeed;
            var animationDuration = _animator.CurrentAnimation.Sprites.Count() / _animator.CurrentAnimation.FrameRate;
            var tween = new FloatTween(_velocityComponent, 0, animationDuration);
            tween.SetEaseType(EaseType.CubicOut);
            tween.Start();

            //determine active frame
            switch (_comboCounter)
            {
                case 1:
                    _activeFrame = 0;
                    break;
                case 2:
                    _activeFrame = 0;
                    break;
            }
        }

        void OnAttackAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;

            //if reached max combo, end attack
            if (_comboCounter >= _maxCombo)
            {
                EndAttack();
            }
            else if (_continueCombo || Input.LeftMouseButtonDown)
            {
                _timeSinceLastClick = 0f;
                _timeSinceAttackStarted = 0f;
                _continueCombo = false;
                _activeFrame = -1;

                PerformAttack();
            }
            else
            {
                EndAttack();
            }
        }

        void EndAttack()
        {
            _isAttacking = false;
            _continueCombo = false;
            _timeSinceLastClick = 0f;
            _timeSinceAttackStarted = 0f;
            _comboCounter = 0;
            _activeFrame = -1;

            _attackCompleteCallback?.Invoke();
        }
    }
}
