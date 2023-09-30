﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters
{
    public class Player : Component, IUpdatable
    {
        public static Player Instance { get; private set; } = new Player();

        //state machine
        internal PlayerStateMachine StateMachine;

        //input
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        VirtualButton _actionInput;

        //stats
        float _moveSpeed = 100f;

        //components
        Mover _mover;
        SpriteAnimator _spriteAnimator;
        Collider _collider;
        HealthComponent _healthComponent;
        Hurtbox _hurtbox;
        HitHandler _hitHandler;
        ActionPointComponent _actionPointComponent;

        //misc
        Vector2 _direction = new Vector2(1, 0);
        Vector2 _lastNonZeroDirection = new Vector2(1, 0);
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();

        #region SETUP

        public Player()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            AddObservers();
            SetupInput();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            StateMachine = new PlayerStateMachine(this, new PlayerInCombat());
        }

        void AddComponents()
        {
            //add sprites
            _spriteAnimator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //add mover
            _mover = Entity.AddComponent(new Mover());

            //Add hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(10, 20));
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.PlayerHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            //add collision box
            _collider = Entity.AddComponent(new BoxCollider(-5, 5, 10, 8));

            //Add health component
            _healthComponent = Entity.AddComponent(new HealthComponent(10, 10));

            //hit handler
            _hitHandler = Entity.AddComponent(new HitHandler());

            //action points
            _actionPointComponent = Entity.AddComponent(new ActionPointComponent(5, 10));
        }

        void AddObservers()
        {
            //_hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHitboxHit);
            _hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, _hitHandler.OnHurtboxHit);

            _hitHandler.Emitter.AddObserver(HitHandlerEventType.Hurt, OnHurt);
            _hitHandler.Emitter.AddObserver(HitHandlerEventType.Killed, OnKilled);

            Game1.GameEventsEmitter.AddObserver(GameEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
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

            _actionInput = new VirtualButton();
            _actionInput.AddKeyboardKey(Keys.Space);
        }

        void AddAnimations()
        {
            //idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            _spriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            _spriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            _spriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            _spriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));

            //run
            var runTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            _spriteAnimator.AddAnimation("RunRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 0, 9));
            _spriteAnimator.AddAnimation("RunLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 10, 19));
            _spriteAnimator.AddAnimation("RunDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 20, 27));
            _spriteAnimator.AddAnimation("RunUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 30, 37));

            //hurt
            var hurtTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_hurt);
            var hurtSprites = Sprite.SpritesFromAtlas(hurtTexture, 64, 64);
            _spriteAnimator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 0, 4));
            _spriteAnimator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 5, 9));

            //death
            var deathTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_death);
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 64, 64);
            _spriteAnimator.AddAnimation("DeathRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 0, 9));
            _spriteAnimator.AddAnimation("DeathLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 10, 19));
        }

        #endregion

        public void Update()
        {
            StateMachine.Update(Time.DeltaTime);
        }

        public void OnHurt()
        {
            var hurtAnimation = _direction.X >= 0 ? "HurtRight" : "HurtLeft";
            _spriteAnimator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                _spriteAnimator.OnAnimationCompletedEvent -= handler;
                StateMachine.ChangeState<PlayerInCombat>();
            }
            _spriteAnimator.OnAnimationCompletedEvent += handler;
            StateMachine.ChangeState<PlayerHurt>();
        }

        public void OnKilled()
        {
            var deathAnimation = _direction.X >= 0 ? "DeathRight" : "DeathLeft";
            _spriteAnimator.Play(deathAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                _spriteAnimator.OnAnimationCompletedEvent -= handler;
                Game1.Exit();
            }
            _spriteAnimator.OnAnimationCompletedEvent += handler;
            StateMachine.ChangeState<PlayerDying>();
        }

        public void Idle()
        {
            //handle animation
            var animation = "IdleDown";
            if (_lastNonZeroDirection.X != 0)
            {
                animation = _lastNonZeroDirection.X >= 0 ? "IdleRight" : "IdleLeft";
            }
            else if (_lastNonZeroDirection.Y != 0)
            {
                animation = _lastNonZeroDirection.Y >= 0 ? "IdleDown" : "IdleUp";
            }
            if (!_spriteAnimator.IsAnimationActive(animation))
            {
                _spriteAnimator.Play(animation);
            }
        }

        public void Move()
        {
            //animation
            var animation = "RunDown";
            if (_direction.X != 0)
            {
                animation = _direction.X >= 0 ? "RunRight" : "RunLeft";
            }
            else if (_direction.Y != 0)
            {
                animation = _direction.Y >= 0 ? "RunDown" : "RunUp";
            }
            if (!_spriteAnimator.IsAnimationActive(animation))
            {
                _spriteAnimator.Play(animation);
            }

            //move
            var movement = _direction * _moveSpeed * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out var result);
            _subPixelV2.Update(ref movement);
            _mover.ApplyMovement(movement);
        }

        public void HandleInput()
        {
            //first, check for action button
            if (_actionInput.IsPressed)
            {
                if (_actionPointComponent.ActionPoints > 0)
                {
                    Game1.GameEventsEmitter.Emit(GameEvents.TurnPhaseTriggered);
                }
            }

            //get movement inputs
            var newDirection = new Vector2(_xAxisInput.Value, _yAxisInput.Value);

            //if no movement, switch to idle and return
            if (newDirection == Vector2.Zero)
            {
                //_lastNonZeroDirection = _direction;
                //_direction = newDirection;
                //_direction.Normalize();
                Idle();
            }
            else
            {
                _direction = newDirection;
                _direction.Normalize();
                _lastNonZeroDirection = _direction;
                Move();
            }
        }

        public void ChargeActionPoints()
        {
            _actionPointComponent.Charge();
        }

        public void OnTurnPhaseTriggered()
        {
            _actionPointComponent.StopCharging();
        }
    }

    #region STATE MACHINE

    public class PlayerStateMachine : StateMachine<Player>
    {
        public PlayerStateMachine(Player context, State<Player> initialState) : base(context, initialState)
        {
            AddState(new PlayerIdle());
            AddState(new PlayerRunning());
            AddState(new PlayerHurt());
            AddState(new PlayerDying());
            AddState(new PlayerInCombat());
        }
    }

    public class PlayerInCombat : State<Player>
    {
        public override void Begin()
        {
            base.Begin();

            _context.ChargeActionPoints();
        }
        public override void Update(float deltaTime)
        {
            _context.HandleInput();
        }
    }

    public class PlayerIdle : State<Player>
    {
        public override void Update(float deltaTime)
        {
            _context.Idle();
        }
    }

    public class PlayerRunning : State<Player>
    {
        public override void Update(float deltaTime)
        {
            _context.Move();
        }
    }

    public class PlayerHurt : State<Player>
    {
        public override void Update(float deltaTime)
        {

        }
    }

    public class PlayerDying : State<Player>
    {
        public override void Update(float deltaTime)
        {

        }
    }

    #endregion
}
