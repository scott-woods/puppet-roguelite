﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.PlayerActions.Attacks;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Items;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class PlayerController : Component, IUpdatable
    {
        public static PlayerController Instance { get; private set; } = new PlayerController();

        //state machine
        internal PlayerStateMachine StateMachine;

        //input
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        VirtualButton _actionInput;
        VirtualButton _checkInput;

        //stats
        float _moveSpeed = 150f;
        float _raycastDistance = 10f;

        //components
        Mover _mover;
        SpriteAnimator _spriteAnimator;
        Collider _collider;
        HealthComponent _healthComponent;
        Hurtbox _hurtbox;
        public ActionPointComponent ActionPointComponent;
        public AttacksList AttacksList;
        Inventory _inventory;
        YSorter _ySorter;

        //misc
        public Vector2 Direction = new Vector2(1, 0);
        Vector2 _lastNonZeroDirection = new Vector2(1, 0);
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();

        #region SETUP

        public PlayerController()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            AddObservers();
            SetupInput();

            StateMachine = new PlayerStateMachine(this, new PlayerDefault());
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
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.PlayerHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.EnemyHitbox);
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            //add collision box
            _collider = Entity.AddComponent(new BoxCollider(-5, 4, 10, 8));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.PlayerCollider);
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            //Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Trigger);

            //Add health component
            _healthComponent = Entity.AddComponent(new HealthComponent(10, 10));
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);

            //action points
            ActionPointComponent = Entity.AddComponent(new ActionPointComponent(5, 10));

            //attacks list
            AttacksList = Entity.AddComponent(new AttacksList(new List<Type>() { typeof(Slash), typeof(Dash) }));

            //inventory
            _inventory = Entity.AddComponent(new Inventory());
            _inventory.AddItem(new CerealBox("Reese's Puffs"));

            //ySort
            _ySorter = Entity.AddComponent(new YSorter(_spriteAnimator, 12));
        }

        void AddObservers()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);

            Emitters.InteractableEmitter.AddObserver(InteractableEvents.Interacted, OnInteractionStarted);
            Emitters.InteractableEmitter.AddObserver(InteractableEvents.InteractionFinished, OnInteractionFinished);

            Emitters.CutsceneEmitter.AddObserver(CutsceneEvents.CutsceneStarted, () => StateMachine.ChangeState<PlayerIdle>());
            Emitters.CutsceneEmitter.AddObserver(CutsceneEvents.CutsceneEnded, () => StateMachine.ChangeState<PlayerDefault>());
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

            _checkInput = new VirtualButton();
            _checkInput.AddKeyboardKey(Keys.Z);
        }

        void AddAnimations()
        {
            //idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            _spriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            _spriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            _spriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            _spriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));

            //run
            var runTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            _spriteAnimator.AddAnimation("RunRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 0, 9));
            _spriteAnimator.AddAnimation("RunLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 10, 19));
            _spriteAnimator.AddAnimation("RunDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 20, 27));
            _spriteAnimator.AddAnimation("RunUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 30, 37));

            //hurt
            var hurtTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_hurt);
            var hurtSprites = Sprite.SpritesFromAtlas(hurtTexture, 64, 64);
            _spriteAnimator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 0, 4));
            _spriteAnimator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 5, 9));

            //death
            var deathTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_death);
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 64, 64);
            _spriteAnimator.AddAnimation("DeathRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 0, 9));
            _spriteAnimator.AddAnimation("DeathLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 10, 19));
        }

        #endregion

        public void Update()
        {
            StateMachine.Update(Time.DeltaTime);
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
            if (Direction.X != 0)
            {
                animation = Direction.X >= 0 ? "RunRight" : "RunLeft";
            }
            else if (Direction.Y != 0)
            {
                animation = Direction.Y >= 0 ? "RunDown" : "RunUp";
            }
            if (!_spriteAnimator.IsAnimationActive(animation))
            {
                _spriteAnimator.Play(animation);
            }

            //move
            var movement = Direction * _moveSpeed * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out var result);
            _subPixelV2.Update(ref movement);
            _mover.ApplyMovement(movement);
        }

        public bool IsActionInputPressed()
        {
            return _actionInput.IsPressed;
        }

        public void HandleCheck()
        {
            if (_checkInput.IsPressed)
            {
                var raycastHit = Physics.Linecast(Entity.Position, Entity.Position + _lastNonZeroDirection * _raycastDistance);

                if (raycastHit.Collider != null)
                {
                    if (raycastHit.Collider.Entity.TryGetComponent<Interactable>(out var interactable))
                    {
                        interactable.Interact();
                    }
                }
                else
                {
                    var overlap = Physics.OverlapRectangle(new RectangleF(Entity.Position, new Vector2(16, 16)));
                    if (overlap != null)
                    {
                        if (overlap.Entity.TryGetComponent<Interactable>(out var interactable))
                        {
                            interactable.Interact();
                        }
                    }
                }
            }
        }

        public void HandleMovementInput()
        {
            //get movement inputs
            var newDirection = new Vector2(_xAxisInput.Value, _yAxisInput.Value);

            //if no movement, switch to idle and return
            if (newDirection == Vector2.Zero)
            {
                Idle();
            }
            else
            {
                Direction = newDirection;
                Direction.Normalize();
                _lastNonZeroDirection = Direction;
                Move();
            }
        }

        public void OnDamageTaken(HealthComponent healthComponent)
        {
            var hurtAnimation = Direction.X >= 0 ? "HurtRight" : "HurtLeft";
            _spriteAnimator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                _spriteAnimator.OnAnimationCompletedEvent -= handler;
                StateMachine.ChangeState<PlayerInCombat>();
            }
            _spriteAnimator.OnAnimationCompletedEvent += handler;
            StateMachine.ChangeState<PlayerHurt>();
        }

        public void OnHealthDepleted(HealthComponent healthComponent)
        {
            var deathAnimation = Direction.X >= 0 ? "DeathRight" : "DeathLeft";
            _spriteAnimator.Play(deathAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                _spriteAnimator.OnAnimationCompletedEvent -= handler;
                Core.Exit();
            }
            _spriteAnimator.OnAnimationCompletedEvent += handler;
            StateMachine.ChangeState<PlayerDying>();
        }

        void OnTurnPhaseTriggered()
        {
            StateMachine.ChangeState<PlayerInTurn>();
        }

        void OnDodgePhaseStarted()
        {
            StateMachine.ChangeState<PlayerInCombat>();
        }

        void OnEncounterStarted()
        {
            StateMachine.ChangeState<PlayerInCombat>();
        }

        void OnEncounterEnded()
        {
            StateMachine.ChangeState<PlayerDefault>();
        }

        void OnInteractionStarted()
        {
            StateMachine.ChangeState<PlayerIdle>();
        }

        void OnInteractionFinished()
        {
            Core.Schedule(.1f, timer => StateMachine.ChangeState<PlayerDefault>());
        }
    }

    #region STATE MACHINE

    public class PlayerStateMachine : StateMachine<PlayerController>
    {
        public PlayerStateMachine(PlayerController context, State<PlayerController> initialState) : base(context, initialState)
        {
            AddState(new PlayerIdle());
            AddState(new PlayerRunning());
            AddState(new PlayerHurt());
            AddState(new PlayerDying());
            AddState(new PlayerInCombat());
            AddState(new PlayerInTurn());
        }
    }

    public class PlayerInCombat : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            if (_context.IsActionInputPressed() && _context.ActionPointComponent.ActionPoints > 0)
            {
                Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseTriggered);
                return;
            }

            _context.HandleMovementInput();
        }
    }

    public class PlayerDefault : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            _context.HandleCheck();
            _context.HandleMovementInput();
        }
    }

    public class PlayerIdle : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            _context.Idle();
        }
    }

    public class PlayerRunning : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            _context.Move();
        }
    }

    public class PlayerHurt : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {

        }
    }

    public class PlayerDying : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {

        }
    }

    public class PlayerInTurn : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            _context.Idle();
        }
    }

    #endregion
}