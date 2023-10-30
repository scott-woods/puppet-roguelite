﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Attacks
{
    [PlayerActionInfo("Dash", 2)]
    public class DashAttack : PlayerAction
    {
        int _damage = 3;
        int _range = 32;
        float _rotationSpeed = 175f;
        int _startFrame = 3;
        int[] _hitboxActiveFrames = new int[] { 3, 4, 5 };

        Vector2 _direction = Vector2.One;
        float _angle = 0;
        SubpixelVector2 _subpixelV2 = new SubpixelVector2();
        bool _movementStarted = false;
        float _totalMovementTime;
        float _movementTimeRemaining;

        //components
        Mover _mover;
        SpriteAnimator _animator;
        CircleHitbox _hitbox;
        PrototypeSpriteRenderer _target;
        YSorter _ySorter;
        SpriteTrail _trail;

        List<Component> _componentsList = new List<Component>();

        public override void Initialize()
        {
            base.Initialize();

            _mover = Entity.AddComponent(new Mover());
            _componentsList.Add( _mover );
            
            _animator = Entity.AddComponent(new SpriteAnimator());
            _animator.OnAnimationCompletedEvent += OnAnimationCompleted;
            _animator.Speed = 2;
            _componentsList.Add(_animator );
            AddAnimations();

            //hitbox
            _hitbox = Entity.AddComponent(new CircleHitbox(_damage, 16));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.PushForce = 0f;
            _componentsList.Add(_hitbox);

            _ySorter = Entity.AddComponent(new YSorter(_animator, 12));
            _componentsList.Add(_ySorter);

            _target = new PrototypeSpriteRenderer(8, 8);

            _trail = Entity.AddComponent(new SpriteTrail(_animator));
            _trail.FadeDelay = 0f;
            _trail.FadeDuration = .5f;
            _trail.MinDistanceBetweenInstances = 8;
            _componentsList.Add(_trail);
        }

        void AddAnimations()
        {
            var dashTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var dashSprites = Sprite.SpritesFromAtlas(dashTexture, 64, 64);
            _animator.AddAnimation("DashRight", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 4, 4, 4, 5, 6, 7, 7, 7 }, true));
            _animator.AddAnimation("DashLeft", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 13, 13, 13, 14, 15, 16, 16, 16 }, true));
            _animator.AddAnimation("DashDown", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 22, 22, 22, 23, 24, 25, 25, 25 }, true));
            _animator.AddAnimation("DashUp", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 31, 31, 31, 32, 33, 34, 34, 34 }, true));
        }

        public override void Prepare()
        {
            base.Prepare();

            InitialPosition = Entity.Position;
            FinalPosition = Entity.Position + (_direction * _range);

            Entity.AddComponent(_target);
            _componentsList.Add(_target);

            _animator.Speed = 1;

            var animationName = GetNextAnimation();
            if (!_animator.IsAnimationActive(animationName))
            {
                _animator.SetColor(new Color(Color.White, 128));
                _animator.Play(animationName, SpriteAnimator.LoopMode.Once);
            }
        }

        public override void Execute(bool isSimulation = false)
        {
            base.Execute(isSimulation);

            Entity.Position = InitialPosition;

            _animator.Speed = 2;

            if (!_isSimulation)
            {
                var animationName = GetNextAnimation();
                _hitbox.PushForce = 0;
                _animator.SetColor(new Color(Color.White, 255));
                _animator.Play(animationName, SpriteAnimator.LoopMode.Once);
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._20_Slash_02, .5f);
            }
            else
            {
                var animationName = GetNextAnimation();
                _animator.SetColor(new Color(Color.White, 128));
                _animator.Play(animationName, SpriteAnimator.LoopMode.Once);
            }
        }

        public override void Update()
        {
            base.Update();

            if (_isPreparing)
            {
                //input
                HandleInput();
            }

            MoveEntity();
            UpdateHitbox();
        }

        void OnAnimationCompleted(string animation)
        {
            if (_isPreparing)
            {
                Core.Schedule(.35f, timer =>
                {
                    if (Entity != null)
                    {
                        Entity.Position = InitialPosition;
                        var nextAnimation = GetNextAnimation();
                        _animator.Play(nextAnimation, SpriteAnimator.LoopMode.Once);
                    }
                });

                _movementStarted = false;
            }
            else
            {
                Core.Schedule(.15f, timer =>
                {
                    var type = _isSimulation ? PlayerActionEvents.SimActionFinishedExecuting : PlayerActionEvents.ActionFinishedExecuting;
                    Emitters.PlayerActionEmitter.Emit(type, this);
                });

                _movementStarted = false;
            }
        }

        void HandleInput()
        {
            if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z))
            {
                _animator.Stop();
                _isPreparing = false;
                Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this);
                return;
            }

            //rotate left or right
            float speed = 0;
            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)) speed = -_rotationSpeed;
            else if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right)) speed = _rotationSpeed;

            speed *= Time.DeltaTime;

            //if pressing left or right, apply rotation
            if (speed != 0)
            {
                //rotate target around center
                FinalPosition = Mathf.RotateAround(FinalPosition, InitialPosition, speed);

                //rotate entity around center
                Entity.Position = Mathf.RotateAround(Entity.Position, InitialPosition, speed);

                //calculate direction
                _direction = FinalPosition - InitialPosition;
                _direction.Normalize();

                //calculate angle
                _angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(InitialPosition, FinalPosition));
                _angle = (_angle + 360) % 360;
            }

            //update target
            _target.SetLocalOffset(FinalPosition - Entity.Position);
        }

        string GetNextAnimation()
        {
            var animation = "DashRight";
            if (_angle >= 45 && _angle < 135) animation = "DashDown";
            else if (_angle >= 135 && _angle < 225) animation = "DashLeft";
            else if (_angle >= 225 && _angle < 315) animation = "DashUp";

            return animation;
        }

        void MoveEntity()
        {
            //if animation is in the movement frame
            if (_animator.CurrentFrame == _startFrame)
            {
                //should only be called the first frame we're in the movement frame
                if (!_movementStarted)
                {
                    _movementStarted = true;

                    //determine total amount of time needed to move to target 
                    var secondsPerFrame = 1 / (_animator.CurrentAnimation.FrameRate * _animator.Speed);
                    var movementFrames = 1;
                    _totalMovementTime = movementFrames * secondsPerFrame;
                    _movementTimeRemaining = movementFrames * secondsPerFrame;
                }
                else if (_movementTimeRemaining > 0)
                {
                    //lerp towards target position using progress towards total movement time
                    _movementTimeRemaining -= Time.DeltaTime;
                    var progress = (_totalMovementTime - _movementTimeRemaining) / _totalMovementTime;
                    var lerpPosition = Vector2.Lerp(InitialPosition, FinalPosition, progress);
                    Entity.Position = lerpPosition;
                }
            }
        }

        void UpdateHitbox()
        {
            if (_isPreparing || _isSimulation)
            {
                _hitbox.SetEnabled(false);
            }
            else
            {
                if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                {
                    _hitbox.SetEnabled(true);
                }
                else
                {
                    _hitbox.SetEnabled(false);
                }
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            foreach (var component in _componentsList)
            {
                Entity.RemoveComponent(component);
            }
        }
    }
}