using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using Nez.Tweens;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class MeleeAttack : Component, IUpdatable
    {
        public Emitter<MeleeAttackEvents, int> Emitter = new Emitter<MeleeAttackEvents, int>();

        public bool IsOnCooldown = false;

        //const float _comboThreshold = .8f;
        const float _maxCombo = 3;
        const float _minComboThreshold = .05f;
        const float _offset = 12;
        const float _attackMoveSpeed = 350f;
        const float _fps = 15;
        const float _cooldown = .2f;
        const float _delayBeforeFinisher = .1f;
        const int _damage = 1;

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
        CircleHitbox _hitbox;

        List<Collider> _hitColliders = new List<Collider>();

        FloatTween _speedTween;
        ITimer _delayBeforeFinisherTimer;
        ITimer _delayBeforeEndTimer;

        public MeleeAttack(SpriteAnimator animator, VelocityComponent velocityComponent)
        {
            _animator = animator;
            _velocityComponent = velocityComponent;
        }

        public override void Initialize()
        {
            base.Initialize();

            _hitbox = Entity.AddComponent(new CircleHitbox(_damage, 12));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.SetEnabled(false);

            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);
            _animator.AddAnimation("MeleeRight_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 1, 4), _fps);
            _animator.AddAnimation("MeleeRight_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 5, 7), _fps);
            _animator.AddAnimation("MeleeRight_3", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 1, 4), _fps);
            _animator.AddAnimation("MeleeLeft_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 10, 13), _fps);
            _animator.AddAnimation("MeleeLeft_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 14, 16), _fps);
            _animator.AddAnimation("MeleeLeft_3", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 10, 13), _fps);
            _animator.AddAnimation("MeleeDown_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 19, 22), _fps);
            _animator.AddAnimation("MeleeDown_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 23, 25), _fps);
            _animator.AddAnimation("MeleeDown_3", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 19, 22), _fps);
            _animator.AddAnimation("MeleeUp_1", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 28, 31), _fps);
            _animator.AddAnimation("MeleeUp_2", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 32, 34), _fps);
            _animator.AddAnimation("MeleeUp_3", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 28, 31), _fps);
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
                if (Input.LeftMouseButtonPressed && _timeSinceAttackStarted >= _minComboThreshold)
                {
                    _timeSinceLastClick = 0f;
                    _continueCombo = true;
                }

                //active hitbox if on active frame
                _hitbox.SetEnabled(_animator.CurrentFrame == _activeFrame);

                //move
                _velocityComponent.Move();

                //check for hit
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_hitbox, _hitbox.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    foreach(var collider in colliders)
                    {
                        if (!_hitColliders.Contains(collider))
                        {
                            Emitter.Emit(MeleeAttackEvents.Hit, _damage);
                            _hitColliders.Add(collider);
                        }
                    }
                }
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
            _timeSinceLastClick = 0f;
            _timeSinceAttackStarted = 0f;
            _continueCombo = false;
            _activeFrame = -1;
            _hitColliders.Clear();

            //increment combo
            _comboCounter += 1;

            //determine direction to mouse
            var dir = (Entity.Scene.Camera.MouseToWorldPoint() - Entity.Position);
            dir.Normalize();

            //set hitbox position
            var hitboxPosition = Entity.Position + (dir * _offset);
            _hitbox.SetLocalOffset(hitboxPosition - Entity.Position);
            _hitbox.Direction = dir;
            _hitbox.SetEnabled(true);

            //if this is the final hit in combo, increase force
            if (_comboCounter == _maxCombo)
            {
                _hitbox.PushForce = 1.5f;
            }

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
            _velocityComponent.Speed = _comboCounter == _maxCombo ? _attackMoveSpeed * 1.25f : _attackMoveSpeed;
            var animationDuration = _animator.CurrentAnimation.Sprites.Count() / _animator.CurrentAnimation.FrameRate;
            _speedTween = new FloatTween(_velocityComponent, 0, 1);
            _speedTween.SetEaseType(EaseType.CubicOut);
            _speedTween.SetDuration(animationDuration);
            _speedTween.Start();

            //determine active frame and sound
            switch (_comboCounter)
            {
                case 1:
                    _activeFrame = 0;
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._32_Swoosh_sword_2, .5f);
                    break;
                case 2:
                    _activeFrame = 0;
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._33_Swoosh_Sword_3, .5f);
                    break;
                case 3:
                    _activeFrame = 0;
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._31_swoosh_sword_1, .5f);
                    break;
            }
        }

        void OnAttackAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;

            _activeFrame = -1;

            //if reached max combo, end attack
            if (_comboCounter >= _maxCombo)
            {
                EndAttack();
            }
            else if (_continueCombo || Input.LeftMouseButtonDown)
            {
                if (_comboCounter == _maxCombo - 1)
                {
                    _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
                    _delayBeforeFinisherTimer = Game1.Schedule(_delayBeforeFinisher, timer => PerformAttack());
                }
                else
                {
                    PerformAttack();
                }
            }
            else
            {
                //hold last frame for a moment, give player a chance to extend combo
                _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
                _delayBeforeEndTimer = Game1.Schedule(.16f, timer =>
                {
                    if (_continueCombo || Input.LeftMouseButtonDown)
                    {
                        PerformAttack();
                    }
                    else
                    {
                        EndAttack();
                    }
                });
            }
        }

        public void CancelAttack()
        {
            _isAttacking = false;
            _continueCombo = false;
            _timeSinceLastClick = 0f;
            _timeSinceAttackStarted = 0f;
            _comboCounter = 0;
            _activeFrame = -1;
            _hitbox.PushForce = 1f;
            _hitbox.SetEnabled(false);
            _hitColliders.Clear();
            _speedTween?.Stop();
            _delayBeforeFinisherTimer?.Stop();
            _delayBeforeEndTimer?.Stop();
        }

        void EndAttack()
        {
            _isAttacking = false;
            _continueCombo = false;
            _timeSinceLastClick = 0f;
            _timeSinceAttackStarted = 0f;
            _comboCounter = 0;
            _activeFrame = -1;
            _hitbox.PushForce = 1f;
            _hitbox.SetEnabled(false);

            IsOnCooldown = true;
            Game1.Schedule(_cooldown, timer => IsOnCooldown = false);

            _attackCompleteCallback?.Invoke();
        }
    }

    public enum MeleeAttackEvents
    {
        Hit
    }
}
