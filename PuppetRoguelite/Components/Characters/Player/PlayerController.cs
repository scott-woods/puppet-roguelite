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
using PuppetRoguelite.Models.Items;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class PlayerController : Component, IUpdatable
    {
        public static PlayerController Instance { get; private set; } = new PlayerController();

        //state machine
        public StateMachine<PlayerController> StateMachine;

        //stats
        public float RaycastDistance = 10f;
        public float MoveSpeed = PlayerData.Instance.MovementSpeed * 5f;

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
        //public MeleeAttack MeleeAttack;
        public MeleeAttack MeleeAttack;
        public Dash Dash;
        public SpriteTrail _spriteTrail;
        public KnockbackComponent KnockbackComponent;
        public OriginComponent OriginComponent;
        public DollahInventory DollahInventory;
        public DeathComponent DeathComponent;
        public ActionsManager ActionsManager;
        public SpriteFlipper SpriteFlipper;
        public StatusComponent StatusComponent;

        //misc
        public Vector2 Direction = new Vector2(1, 0);
        public Vector2 LastNonZeroDirection = new Vector2(1, 0);
        public bool WaitingForSceneTransition = false;
        public Vector2 SpriteOffset = new Vector2(13, -2);
        Vector2 _collisionOffset = new Vector2(-4, 4);
        public bool IsMeleeEnabled = true;

        //default status
        Status _defaultStatus = new Status(Status.StatusType.Normal, (int)StatusPriority.Normal);

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

            Entity.SetTag((int)EntityTags.EnemyTarget);

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
            SpriteAnimator.SetLocalOffset(SpriteOffset);
            AddAnimations();

            //add mover
            _mover = Entity.AddComponent(new Mover());

            //Add hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(9, 16));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.PlayerHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.EnemyHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1.25f));
            //Hurtbox.SetEnabled(false);

            //add collision box
            Collider = Entity.AddComponent(new BoxCollider(_collisionOffset.X, _collisionOffset.Y, 8, 5));
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.PlayerCollider);
            Collider.CollidesWithLayers = 0;
            //Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);
            Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            //Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Trigger);
            //Collider.SetEnabled(false);

            //origin
            OriginComponent = Entity.AddComponent(new OriginComponent(Collider));

            //Add health component
            HealthComponent = Entity.AddComponent(new HealthComponent(PlayerUpgradeData.Instance.MaxHpUpgrade.GetCurrentValue()));

            //action points
            ActionPointComponent = Entity.AddComponent(new ActionPointComponent(PlayerUpgradeData.Instance.MaxApUpgrade.GetCurrentValue(), HealthComponent));

            //inventory
            _inventory = Entity.AddComponent(new Inventory());
            //_inventory.AddItem(new CerealBox("feef"));
            //_inventory.AddItem(new CerealBox("feef"));

            //dollahs
            DollahInventory = Entity.AddComponent(new DollahInventory(PlayerData.Instance.Dollahs));

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Nez.Content.Audio.Sounds._69_Die_02, SpriteAnimator, "Die", "Idle", false));

            //ySort
            _ySorter = Entity.AddComponent(new YSorter(SpriteAnimator, OriginComponent));

            //velocity component
            VelocityComponent = Entity.AddComponent(new VelocityComponent(_mover, PlayerData.Instance.MovementSpeed));

            //MeleeAttack = Entity.AddComponent(new MeleeAttack(SpriteAnimator, VelocityComponent));
            MeleeAttack = Entity.AddComponent(new MeleeAttack(SpriteAnimator, VelocityComponent));

            Dash = Entity.AddComponent(new Dash(PlayerData.Instance.MaxDashes));

            _spriteTrail = Entity.AddComponent(new SpriteTrail(SpriteAnimator));
            _spriteTrail.DisableSpriteTrail();

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(110f, .5f, VelocityComponent, Hurtbox, "Idle"));

            //actions manager
            ActionsManager = Entity.AddComponent(new ActionsManager(PlayerData.Instance.AttackActions, PlayerData.Instance.UtilityActions,
                PlayerData.Instance.SupportActions, PlayerUpgradeData.Instance.AttackSlotsUpgrade.GetCurrentValue(),
                PlayerUpgradeData.Instance.UtilitySlotsUpgrade.GetCurrentValue(),
                PlayerUpgradeData.Instance.SupportSlotsUpgrade.GetCurrentValue()));

            var shadow = Entity.AddComponent(new SpriteMime(SpriteAnimator));
            shadow.SetLocalOffset(SpriteOffset);
            shadow.Color = new Color(10, 10, 10, 80);
            shadow.Material = Material.StencilRead();
            shadow.RenderLayer = int.MinValue;

            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(
                new List<SpriteRenderer>() { SpriteAnimator },
                VelocityComponent,
                otherRenderableComponents: new List<RenderableComponent> { shadow }));

            Entity.AddComponent(new Shadow(SpriteAnimator, new Vector2(0, 6), Vector2.One));

            StatusComponent = Entity.AddComponent(new StatusComponent(_defaultStatus));
        }

        void AddAnimations()
        {
            var texture = Game1.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Sci_fi_player_with_sword);
            var noSwordTexture = Game1.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Sci_fi_player_no_sword);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 65);
            var noSwordSprites = Sprite.SpritesFromAtlas(noSwordTexture, 64, 65);

            var totalCols = 13;
            var slashFps = 15;
            var thrustFps = 15;
            var rollFps = 20;

            //down
            SpriteAnimator.AddAnimation($"IdleDown", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 0, 12, totalCols));
            SpriteAnimator.AddAnimation($"IdleDownNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 0, 12, totalCols));
            SpriteAnimator.AddAnimation($"WalkDown", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 1, 8, totalCols));
            SpriteAnimator.AddAnimation($"WalkDownNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 1, 8, totalCols));
            SpriteAnimator.AddAnimation($"RunDown", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 2, 8, totalCols));
            SpriteAnimator.AddAnimation($"RunDownNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 2, 8, totalCols));
            SpriteAnimator.AddAnimation($"ThrustDown", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 3, 4, totalCols), thrustFps);
            //SpriteAnimator.AddAnimation($"SlashDown", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 4, 5, totalCols), slashFps);
            SpriteAnimator.AddAnimation($"SlashDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 54, 58), slashFps);
            SpriteAnimator.AddAnimation($"RollDown", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 5, 8, totalCols), rollFps);
            SpriteAnimator.AddAnimation($"RollDownNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 3, 8, totalCols), rollFps);

            //side
            SpriteAnimator.AddAnimation($"Idle", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 6, 12, totalCols));
            SpriteAnimator.AddAnimation($"IdleNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 4, 12, totalCols));
            SpriteAnimator.AddAnimation($"Walk", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 7, 8, totalCols));
            SpriteAnimator.AddAnimation($"WalkNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 5, 8, totalCols));
            SpriteAnimator.AddAnimation($"Run", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 8, 8, totalCols));
            SpriteAnimator.AddAnimation($"RunNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 6, 8, totalCols));
            SpriteAnimator.AddAnimation($"Roll", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 9, 6, totalCols), rollFps);
            SpriteAnimator.AddAnimation($"RollNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 7, 6, totalCols), rollFps);
            SpriteAnimator.AddAnimation($"Thrust", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 10, 4, totalCols), thrustFps);
            SpriteAnimator.AddAnimation($"Slash", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 11, 5, totalCols), slashFps);

            //up
            SpriteAnimator.AddAnimation($"IdleUp", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 12, 12, totalCols));
            SpriteAnimator.AddAnimation($"IdleUpNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 8, 12, totalCols));
            SpriteAnimator.AddAnimation($"WalkUp", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 13, 8, totalCols));
            SpriteAnimator.AddAnimation($"WalkUpNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 9, 8, totalCols));
            SpriteAnimator.AddAnimation($"RunUp", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 14, 8, totalCols));
            SpriteAnimator.AddAnimation($"RunUpNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 10, 8, totalCols));
            SpriteAnimator.AddAnimation($"ThrustUp", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 15, 4, totalCols), thrustFps);
            //SpriteAnimator.AddAnimation($"SlashUp", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 16, 5, totalCols), slashFps);
            SpriteAnimator.AddAnimation($"SlashUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 210, 214), slashFps);
            SpriteAnimator.AddAnimation($"RollUp", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 17, 8, totalCols), rollFps);
            SpriteAnimator.AddAnimation($"RollUpNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 11, 8, totalCols), rollFps);

            //death
            SpriteAnimator.AddAnimation($"Die", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 18, 13, totalCols));
            SpriteAnimator.AddAnimation($"DieNoSword", AnimatedSpriteHelper.GetSpriteArrayByRow(noSwordSprites, 12, 13, totalCols));
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

            Debug.Log(Entity.Position);
        }

        public void Idle()
        {
            //handle animation
            var animation = "IdleDown";
            if (Math.Abs(VelocityComponent.Direction.X) >= Math.Abs(VelocityComponent.Direction.Y))
            {
                animation = "Idle";
            }
            else
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

        /// <summary>
        /// if using keyboard/mouse, returns direction from player to mouse
        /// if using gamepad, returns direction left joystick is facing
        /// </summary>
        /// <returns></returns>
        public Vector2 GetFacingDirection()
        {
            if (Game1.InputStateManager.IsUsingGamepad)
            {
                var dir = new Vector2(Controls.Instance.XAxisIntegerInput.Value, Controls.Instance.YAxisIntegerInput.Value);
                if (dir == Vector2.Zero)
                {
                    dir = VelocityComponent.Direction;
                }

                return dir;
            }
            else
                return Entity.Scene.Camera.MouseToWorldPoint() - Entity.Position;
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
            //Idle();
            StateMachine.ChangeState<CutsceneState>();
        }

        void OnSceneTransitionEnded()
        {
            StateMachine.ChangeState<IdleState>();
        }

        #endregion
    }
}
