using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Player.PlayerComponents;
using PuppetRoguelite.Components.Characters.Player.States;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.Tools;
using System;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class PlayerController : Component, IUpdatable
    {
        public static PlayerController Instance { get; private set; } = new PlayerController();

        //state machine
        public StateMachine<PlayerController> StateMachine;

        //input
        public VirtualIntegerAxis XAxisInput;
        public VirtualIntegerAxis YAxisInput;
        public VirtualButton ActionInput;
        public VirtualButton CheckInput;
        public VirtualButton DashInput;

        //stats
        public float RaycastDistance = 10f;
        public float MoveSpeed = PlayerData.Instance.MovementSpeed;

        //components
        Mover _mover;
        public SpriteAnimator SpriteAnimator;
        public Collider Collider;
        public HealthComponent HealthComponent;
        public Hurtbox Hurtbox;
        public ActionPointComponent ActionPointComponent;
        Inventory _inventory;
        YSorter _ySorter;
        public VelocityComponent VelocityComponent;
        public MeleeAttack MeleeAttack;
        public Dash Dash;
        public SpriteTrail _spriteTrail;
        public KnockbackComponent KnockbackComponent;
        public OriginComponent OriginComponent;
        public DollahInventory DollahInventory;
        public DeathComponent DeathComponent;
        public ActionsManager ActionsManager;

        //misc
        public Vector2 Direction = new Vector2(1, 0);
        public Vector2 LastNonZeroDirection = new Vector2(1, 0);
        public bool WaitingForSceneTransition = false;

        //data
        //public PlayerData PlayerData;
        //public PlayerUpgradeData PlayerUpgradeData;

        #region SETUP

        public PlayerController()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            SetupInput();

            StateMachine = new StateMachine<PlayerController>(this, new IdleState());
            StateMachine.AddState(new AttackState());
            StateMachine.AddState(new DashState());
            StateMachine.AddState(new DyingState());
            StateMachine.AddState(new HurtState());
            StateMachine.AddState(new MoveState());
            StateMachine.AddState(new CutsceneState());
            StateMachine.AddState(new TurnState());
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.CutsceneEmitter.AddObserver(CutsceneEvents.CutsceneStarted, OnCutsceneStarted);
            Emitters.CutsceneEmitter.AddObserver(CutsceneEvents.CutsceneEnded, OnCutsceneEnded);

            Game1.SceneManager.Emitter.AddObserver(GlobalManagers.SceneEvents.TransitionStarted, OnSceneTransitionStarted);
            Game1.SceneManager.Emitter.AddObserver(GlobalManagers.SceneEvents.TransitionEnded, OnSceneTransitionEnded);

            HealthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);

            DeathComponent.OnDeathStarted += OnDeathStarted;
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.CutsceneEmitter.RemoveObserver(CutsceneEvents.CutsceneStarted, OnCutsceneStarted);
            Emitters.CutsceneEmitter.RemoveObserver(CutsceneEvents.CutsceneEnded, OnCutsceneEnded);

            Game1.SceneManager.Emitter.RemoveObserver(GlobalManagers.SceneEvents.TransitionStarted, OnSceneTransitionStarted);
            Game1.SceneManager.Emitter.RemoveObserver(GlobalManagers.SceneEvents.TransitionEnded, OnSceneTransitionEnded);

            HealthComponent.Emitter.RemoveObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);

            DeathComponent.OnDeathStarted -= OnDeathStarted;
        }

        void AddComponents()
        {
            //add sprites
            SpriteAnimator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //add mover
            _mover = Entity.AddComponent(new Mover());

            //Add hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(10, 20));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.PlayerHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.EnemyHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 2f));
            //Hurtbox.SetEnabled(false);

            //add collision box
            Collider = Entity.AddComponent(new BoxCollider(-5, 6, 10, 8));
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.PlayerCollider);
            Flags.UnsetFlag(ref Collider.CollidesWithLayers, -1);
            Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Trigger);
            //Collider.SetEnabled(false);

            //origin
            OriginComponent = Entity.AddComponent(new OriginComponent(Collider));

            //Add health component
            HealthComponent = Entity.AddComponent(new HealthComponent(PlayerUpgradeData.Instance.MaxHpUpgrade.GetCurrentMaxHp()));

            //action points
            ActionPointComponent = Entity.AddComponent(new ActionPointComponent(PlayerUpgradeData.Instance.MaxApUpgrade.GetCurrentMaxAp(), HealthComponent));

            //inventory
            _inventory = Entity.AddComponent(new Inventory());
            //_inventory.AddItem(new CerealBox("feef"));
            //_inventory.AddItem(new CerealBox("feef"));

            //dollahs
            DollahInventory = Entity.AddComponent(new DollahInventory(PlayerData.Instance.Dollahs));

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Nez.Content.Audio.Sounds._69_Die_02, SpriteAnimator, "DeathRight", "HurtRight"));

            //ySort
            _ySorter = Entity.AddComponent(new YSorter(SpriteAnimator, OriginComponent));

            //velocity component
            VelocityComponent = Entity.AddComponent(new VelocityComponent(_mover, PlayerData.Instance.MovementSpeed));

            MeleeAttack = Entity.AddComponent(new MeleeAttack(SpriteAnimator, VelocityComponent));

            Dash = Entity.AddComponent(new Dash(PlayerData.Instance.MaxDashes));

            _spriteTrail = Entity.AddComponent(new SpriteTrail(SpriteAnimator));
            _spriteTrail.DisableSpriteTrail();

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(110f, .75f, VelocityComponent, Hurtbox));

            //actions manager
            ActionsManager = Entity.AddComponent(new ActionsManager(PlayerData.Instance.AttackActions, PlayerData.Instance.UtilityActions,
                PlayerData.Instance.SupportActions, PlayerUpgradeData.Instance.AttackSlotsUpgrade.GetCurrentValue(),
                PlayerUpgradeData.Instance.UtilitySlotsUpgrade.GetCurrentValue(),
                PlayerUpgradeData.Instance.SupportSlotsUpgrade.GetCurrentValue()));

            var shadow = Entity.AddComponent(new SpriteMime(Entity.GetComponent<SpriteRenderer>()));
            shadow.Color = new Color(10, 10, 10, 80);
            shadow.Material = Material.StencilRead();
            shadow.RenderLayer = int.MinValue;
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
            CheckInput.AddKeyboardKey(Keys.E);

            DashInput = new VirtualButton();
            DashInput.AddKeyboardKey(Keys.Space);
        }

        void AddAnimations()
        {
            //idle
            var idleTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            SpriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            SpriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            SpriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            SpriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));

            //run
            var runTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            var runFps = 13;
            SpriteAnimator.AddAnimation("RunRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 0, 9), runFps);
            SpriteAnimator.AddAnimation("RunLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 10, 19), runFps);
            SpriteAnimator.AddAnimation("RunDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 20, 27), runFps);
            SpriteAnimator.AddAnimation("RunUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 30, 37), runFps);

            //hurt
            var hurtTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_hurt);
            var hurtSprites = Sprite.SpritesFromAtlas(hurtTexture, 64, 64);
            SpriteAnimator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 0, 4));
            SpriteAnimator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 5, 9));

            //death
            var deathTexture = Game1.Content.LoadTexture(Content.Textures.Characters.Player.Hooded_knight_death);
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 64, 64);
            SpriteAnimator.AddAnimation("DeathRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 0, 9));
            SpriteAnimator.AddAnimation("DeathLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 10, 19));

            //dash
            var dashTexture = Game1.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var dashSprites = Sprite.SpritesFromAtlas(dashTexture, 64, 64);
            SpriteAnimator.AddAnimation("DashRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 4, 4));
            SpriteAnimator.AddAnimation("DashLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 13, 13));
            SpriteAnimator.AddAnimation("DashDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 26, 26));
            SpriteAnimator.AddAnimation("DashUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(dashSprites, 35, 35));
        }

        #endregion

        public void Update()
        {
            if (WaitingForSceneTransition)
            {
                Idle();
            }
            else
            {
                StateMachine.Update(Time.DeltaTime);
            }
        }

        public void Idle()
        {
            //handle animation
            var animation = "IdleDown";
            if (VelocityComponent.Direction.X != 0)
            {
                animation = VelocityComponent.Direction.X >= 0 ? "IdleRight" : "IdleLeft";
            }
            else if (VelocityComponent.Direction.Y != 0)
            {
                animation = VelocityComponent.Direction.Y >= 0 ? "IdleDown" : "IdleUp";
            }
            if (!SpriteAnimator.IsAnimationActive(animation))
            {
                SpriteAnimator.Play(animation);
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

        #region OBSERVERS

        void OnTurnPhaseTriggered()
        {
            Hurtbox.SetEnabled(false);
            StateMachine.ChangeState<TurnState>();
        }

        void OnTurnPhaseCompleted()
        {
            Hurtbox.SetEnabled(true);
            StateMachine.ChangeState<IdleState>();
        }

        void OnDamageTaken(HealthComponent hc)
        {
            if (StateMachine.CurrentState.GetType() != typeof(DyingState))
            {
                StateMachine.ChangeState<HurtState>();
            }
        }

        void OnDeathStarted(Entity entity)
        {
            StateMachine.ChangeState<DyingState>();
        }

        void OnCutsceneStarted()
        {
            StateMachine.ChangeState<CutsceneState>();
        }

        void OnCutsceneEnded()
        {
            StateMachine.ChangeState<IdleState>();
        }

        void OnSceneTransitionStarted()
        {
            StateMachine.ChangeState<CutsceneState>();
        }

        void OnSceneTransitionEnded()
        {
            StateMachine.ChangeState<IdleState>();
        }

        #endregion
    }
}
