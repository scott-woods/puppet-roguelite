using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player.Superstates;
using PuppetRoguelite.Components.PlayerActions.Attacks;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
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
        //internal PlayerStateMachine StateMachine;
        public StateMachine<PlayerController> StateMachine;

        //input
        public VirtualIntegerAxis XAxisInput;
        public VirtualIntegerAxis YAxisInput;
        public VirtualButton ActionInput;
        public VirtualButton CheckInput;
        public VirtualButton DashInput;

        //stats
        public float MoveSpeed = 115f;
        float _raycastDistance = 10f;

        //components
        Mover _mover;
        public SpriteAnimator SpriteAnimator;
        Collider _collider;
        public HealthComponent HealthComponent;
        Hurtbox _hurtbox;
        public ActionPointComponent ActionPointComponent;
        public AttacksList AttacksList;
        Inventory _inventory;
        YSorter _ySorter;
        public VelocityComponent VelocityComponent;
        public MeleeAttack MeleeAttack;
        public Dash Dash;
        public SpriteTrail _spriteTrail;

        //misc
        public Vector2 Direction = new Vector2(1, 0);
        public Vector2 LastNonZeroDirection = new Vector2(1, 0);

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

            StateMachine = new StateMachine<PlayerController>(this, new ExploreState());
            StateMachine.AddState(new CombatState());
            StateMachine.AddState(new CutsceneState());
            StateMachine.AddState(new TurnState());
        }

        void AddComponents()
        {
            //add sprites
            SpriteAnimator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //add mover
            _mover = Entity.AddComponent(new Mover());

            //Add hurtbox
            //var hurtboxCollider = Entity.AddComponent(new BoxCollider(10, 20));
            //hurtboxCollider.IsTrigger = true;
            //Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.PlayerHurtbox);
            //Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.EnemyHitbox);
            //_hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, .5f, 1));

            //add collision box
            _collider = Entity.AddComponent(new BoxCollider(-5, 4, 10, 8));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.PlayerCollider);
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Trigger);

            //Add health component
            HealthComponent = Entity.AddComponent(new HealthComponent(10, 10));

            //action points
            ActionPointComponent = Entity.AddComponent(new ActionPointComponent(5, 10));

            //attacks list
            AttacksList = Entity.AddComponent(new AttacksList(new List<Type>() { typeof(Slash), typeof(DashAttack) }));

            //inventory
            _inventory = Entity.AddComponent(new Inventory());
            _inventory.AddItem(new CerealBox("Reese's Puffs"));

            //ySort
            _ySorter = Entity.AddComponent(new YSorter(SpriteAnimator, 12));

            //velocity component
            VelocityComponent = Entity.AddComponent(new VelocityComponent(_mover, MoveSpeed));

            MeleeAttack = Entity.AddComponent(new MeleeAttack(SpriteAnimator, VelocityComponent));

            Dash = Entity.AddComponent(new Dash());

            _spriteTrail = Entity.AddComponent(new SpriteTrail(SpriteAnimator));
            _spriteTrail.DisableSpriteTrail();
        }

        void AddObservers()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);

            Emitters.InteractableEmitter.AddObserver(InteractableEvents.Interacted, OnInteractionStarted);
            Emitters.InteractableEmitter.AddObserver(InteractableEvents.InteractionFinished, OnInteractionFinished);
        }

        void SetupInput()
        {
            XAxisInput = new VirtualIntegerAxis();
            XAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            XAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            XAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D));

            YAxisInput = new VirtualIntegerAxis();
            YAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            YAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            YAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.W, Keys.S));

            ActionInput = new VirtualButton();
            ActionInput.AddKeyboardKey(Keys.E);

            CheckInput = new VirtualButton();
            CheckInput.AddKeyboardKey(Keys.Z);

            DashInput = new VirtualButton();
            DashInput.AddKeyboardKey(Keys.Space);
        }

        void AddAnimations()
        {
            //idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            SpriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            SpriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            SpriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            SpriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));

            //run
            var runTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            SpriteAnimator.AddAnimation("RunRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 0, 9));
            SpriteAnimator.AddAnimation("RunLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 10, 19));
            SpriteAnimator.AddAnimation("RunDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 20, 27));
            SpriteAnimator.AddAnimation("RunUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 30, 37));

            //hurt
            var hurtTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_hurt);
            var hurtSprites = Sprite.SpritesFromAtlas(hurtTexture, 64, 64);
            SpriteAnimator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 0, 4));
            SpriteAnimator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 5, 9));

            //death
            var deathTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_death);
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 64, 64);
            SpriteAnimator.AddAnimation("DeathRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 0, 9));
            SpriteAnimator.AddAnimation("DeathLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 10, 19));

            //dash
            var dashTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var dashSprites = Sprite.SpritesFromAtlas(dashTexture, 64, 64);
            SpriteAnimator.AddAnimation("DashRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 4, 4));
            SpriteAnimator.AddAnimation("DashLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 13, 13));
            SpriteAnimator.AddAnimation("DashDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 26, 26));
            SpriteAnimator.AddAnimation("DashUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 35, 35));
        }

        #endregion

        public void Update()
        {
            StateMachine.Update(Time.DeltaTime);
        }

        public bool CanMove()
        {
            var invalidTypes = new List<Type> { typeof(CutsceneState), typeof(TurnState) };
            return !invalidTypes.Contains(StateMachine.CurrentState.GetType());
        }

        public bool CanTriggerTurn()
        {
            return StateMachine.CurrentState.GetType() == typeof(CombatState);
        }

        public bool CanMelee()
        {
            return !MeleeAttack.IsOnCooldown;
        }

        public bool CanDash()
        {
            return !Dash.IsOnCooldown;
        }

        public void TryTriggerTurn()
        {
            if (StateMachine.CurrentState.GetType() == typeof(CombatState))
            {
                if (ActionInput.IsPressed)
                {
                    Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseTriggered);
                }
            }
        }

        public void TryCheck()
        {
            if (StateMachine.CurrentState.GetType() == typeof(ExploreState))
            {
                if (CheckInput.IsPressed)
                {
                    var raycastHit = Physics.Linecast(Entity.Position, Entity.Position + LastNonZeroDirection * _raycastDistance);

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
        }

        public void ExecuteMeleeAttack(Action attackCompleteCallback)
        {
            MeleeAttack.ExecuteAttack(attackCompleteCallback);
        }

        public void ExecuteDash(Action dashCompleteCallback)
        {
            Dash.ExecuteDash(dashCompleteCallback, SpriteAnimator, VelocityComponent, _spriteTrail);
        }

        #region COMBAT EVENTS OBSERVERS

        void OnTurnPhaseTriggered()
        {
            StateMachine.ChangeState<TurnState>();
        }

        void OnDodgePhaseStarted()
        {
            StateMachine.ChangeState<CombatState>();
        }

        void OnEncounterEnded()
        {
            StateMachine.ChangeState<ExploreState>();
        }

        #endregion

        #region INTERACTION OBSERVERS

        void OnInteractionStarted()
        {
            StateMachine.ChangeState<CutsceneState>();
        }

        void OnInteractionFinished()
        {
            Core.Schedule(.1f, timer => StateMachine.ChangeState<ExploreState>());
        }

        #endregion
    }

    #region STATE MACHINE

    //public class PlayerStateMachine : StateMachine<PlayerController>
    //{
    //    public PlayerStateMachine(PlayerController context, State<PlayerController> initialState) : base(context, initialState)
    //    {
    //        AddState(new PlayerHurt());
    //        AddState(new PlayerDying());
    //        AddState(new PlayerInTurn());
    //        AddState(new PlayerMove());
    //    }
    //}

    //public class PlayerMove : State<PlayerController>
    //{
    //    public override void Update(float deltaTime)
    //    {
    //        //get movement inputs
    //        var newDirection = new Vector2(_context.XAxisInput.Value, _context.YAxisInput.Value);

    //        //if no movement, switch to idle and return
    //        if (newDirection == Vector2.Zero)
    //        {
    //            _context.PlayIdleAnimation();
    //        }
    //        else
    //        {
    //            newDirection.Normalize();
    //            _context.VelocityComponent.SetDirection(newDirection);
    //            _context.Move();
    //        }

    //        var combatManager = _context.Entity.Scene.GetSceneComponent<CombatManager>();
    //        if (combatManager != null)
    //        {
    //            if (combatManager.GameState == GameState.Combat)
    //            {
    //                if (_context.ActionInput.IsPressed && _context.ActionPointComponent.ActionPoints > 0)
    //                {
    //                    Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseTriggered);
    //                }
    //                //TODO: ATTACK
    //            }
    //            else if (combatManager.GameState == GameState.Exploration)
    //            {
    //                _context.HandleCheck();
    //            }
    //        }
    //    }
    //}

    //public class PlayerHurt : State<PlayerController>
    //{
    //    public override void Update(float deltaTime)
    //    {

    //    }
    //}

    //public class PlayerDying : State<PlayerController>
    //{
    //    public override void Update(float deltaTime)
    //    {

    //    }
    //}

    //public class PlayerInTurn : State<PlayerController>
    //{
    //    public override void Update(float deltaTime)
    //    {
    //        _context.PlayIdleAnimation();
    //    }
    //}

    #endregion
}
