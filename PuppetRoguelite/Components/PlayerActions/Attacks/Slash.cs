﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.PlayerActions;
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
    [PlayerActionInfo("Slash", 1)]
    public class Slash : PlayerAction, IUpdatable
    {
        int _offset = 12;
        Vector2 _hitboxHorizontalSize = new Vector2(16, 32);
        Vector2 _hitboxVerticalSize = new Vector2(32, 16);
        int _damage = 4;

        Vector2 _direction = new Vector2(1, 0);
        int _currentIndex;

        //input
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        VirtualButton _confirmButton;

        //components
        SpriteAnimator _animator;
        CircleHitbox _hitbox;
        YSorter _ySorter;

        public override void Initialize()
        {
            base.Initialize();

            SetupInput();
            AddComponents();
        }

        public override void Execute(bool isSimulation = false)
        {
            base.Execute(isSimulation);

            PlayAnimation();
        }

        public override void Update()
        {
            base.Update();

            HandleInput();

            if (_isPreparing) //check after input in case confirm was pressed
            {
                PlayAnimation();
            }
            if (_animator.IsRunning)
            {
                _currentIndex = _animator.CurrentFrame;
            }

            if (!_isPreparing)
            {
                _hitbox.SetEnabled(_animator.CurrentFrame == 1);
            }
        }

        void PlayAnimation()
        {
            //get animation by direction
            var animation = "SlashRight";
            if (_direction.X != 0)
            {
                animation = _direction.X >= 0 ? "SlashRight" : "SlashLeft";
            }
            else if (_direction.Y != 0)
            {
                animation = _direction.Y >= 0 ? "SlashDown" : "SlashUp";
            }

            //if in prep or sim state, set animator to slightly transparent
            if (_isPreparing || _isSimulation)
            {
                _animator.SetColor(new Color(Color.White, 128));
            }
            else
            {
                _animator.SetColor(new Color(Color.White, 255));
            }

            if (_isPreparing)
            {
                if (!_animator.IsAnimationActive(animation))
                {
                    _animator.Play(animation);
                    _animator.CurrentFrame = _currentIndex;
                }
            }
            else
            {
                _animator.Play(animation, SpriteAnimator.LoopMode.Once);
                _animator.OnAnimationCompletedEvent += _animator_OnAnimationCompletedEvent;

                _hitbox.SetLocalOffset(_direction * _offset);

                //play sound
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._26_Pierce_03);
            }
        }

        private void _animator_OnAnimationCompletedEvent(string obj)
        {
            _animator.OnAnimationCompletedEvent -= _animator_OnAnimationCompletedEvent;

            if (_isPreparing)
            {
                PlayAnimation();
            }
            else
            {
                var type = _isSimulation ? PlayerActionEvents.SimActionFinishedExecuting : PlayerActionEvents.ActionFinishedExecuting;
                Emitters.PlayerActionEmitter.Emit(type, this);
            }
        }

        void SetupInput()
        {
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _xAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));

            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _yAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down));

            _confirmButton = new VirtualButton();
            _confirmButton.AddKeyboardKey(Keys.Z);
        }

        void AddComponents()
        {
            _animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            _ySorter = Entity.AddComponent(new YSorter(_animator, 12));

            //add hitbox
            _hitbox = Entity.AddComponent(new CircleHitbox(_damage, 10));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.SetEnabled(false);
        }

        void AddAnimations()
        {
            //slash
            var slashTexture = Core.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_attack);
            var slashSprites = Sprite.SpritesFromAtlas(slashTexture, 64, 64);
            _animator.AddAnimation("SlashDown", AnimatedSpriteHelper.GetSpriteArray(slashSprites, new List<int>() { 18, 19, 20, 21, 26 }));
            _animator.AddAnimation("SlashRight", AnimatedSpriteHelper.GetSpriteArray(slashSprites, new List<int>() { 0, 1, 2, 3, 8 }));
            _animator.AddAnimation("SlashUp", AnimatedSpriteHelper.GetSpriteArray(slashSprites, new List<int>() { 27, 28, 29, 30, 35 }));
            _animator.AddAnimation("SlashLeft", AnimatedSpriteHelper.GetSpriteArray(slashSprites, new List<int>() { 9, 10, 11, 12, 17 }));
        }

        void HandleInput()
        {
            if (_isPreparing)
            {
                //check for confirm
                if (_confirmButton.IsReleased)
                {
                    _isPreparing = false;
                    _animator.Stop();
                    //SetEnabled(false);
                    Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this);
                    return;
                }

                if (_xAxisInput.Value > 0)
                {
                    _direction = new Vector2(1, 0);
                }
                else if (_xAxisInput.Value < 0)
                {
                    _direction = new Vector2(-1, 0);
                }
                else if (_yAxisInput.Value > 0)
                {
                    _direction = new Vector2(0, 1);
                }
                else if (_yAxisInput.Value < 0)
                {
                    _direction = new Vector2(0, -1);
                }
            }
        }

        public override void OnRemovedFromEntity()
        {
            if (Entity != null)
            {
                if (_animator != null) Entity.RemoveComponent(_animator);
                if (_hitbox != null) Entity.RemoveComponent(_hitbox);
                if (_ySorter != null) Entity.RemoveComponent(_ySorter);
            }

            base.OnRemovedFromEntity();
        }
    }
}
