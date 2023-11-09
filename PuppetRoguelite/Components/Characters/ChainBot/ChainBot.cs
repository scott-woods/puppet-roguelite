using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.ChainBot
{
    public class ChainBot : Enemy, IUpdatable
    {
        public Guid Id = Guid.NewGuid();

        //stats
        float _moveSpeed = 75f;
        int _maxHp = 12;

        //behavior tree
        BehaviorTree<ChainBot> _tree;

        //actions
        ChainBotMelee _chainBotMelee;
        EnemyAction<ChainBot> _activeAction;

        //components
        public Mover Mover;
        public SpriteAnimator Animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        public PathfindingComponent Pathfinder;
        BoxCollider _collider;
        YSorter _ySorter;
        public VelocityComponent VelocityComponent;
        public DollahDropper DollahDropper;
        public NewHealthbar NewHealthbar { get; set; }
        public KnockbackComponent KnockbackComponent;
        public OriginComponent OriginComponent;
        public DeathComponent DeathComponent;
        public StatusComponent StatusComponent;

        #region SETUP

        public ChainBot(Entity mapEntity) : base(mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            SetupBehaviorTree();
        }

        void AddComponents()
        {
            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(8, 18));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 0, Nez.Content.Audio.Sounds.Chain_bot_damaged));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(_maxHp));

            //dollah dropper
            DollahDropper = Entity.AddComponent(new DollahDropper(3, 1));

            //velocity
            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _moveSpeed));

            //collider
            _collider = Entity.AddComponent(new BoxCollider(10, 5));
            _collider.SetLocalOffset(new Vector2(-1, 7));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);

            //origin
            OriginComponent = Entity.AddComponent(new OriginComponent(_collider));

            //pathfinding
            Pathfinder = Entity.AddComponent(new PathfindingComponent(VelocityComponent, OriginComponent, MapEntity));

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Nez.Content.Audio.Sounds.Enemy_death_1, Animator, "Die", "Hit"));

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //actions
            _chainBotMelee = Entity.AddComponent(new ChainBotMelee(this));

            //status
            StatusComponent = Entity.AddComponent(new StatusComponent(new Status(Status.StatusType.Normal, (int)StatusPriority.Normal)));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new NewHealthbar(_healthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(150f, .5f, 4, 2f, VelocityComponent, _hurtbox));
        }

        void AddAnimations()
        {
            //Idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 126, 39);
            Animator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArray(idleSprites, new List<int> { 1, 3, 5, 7, 9 }));
            Animator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArray(idleSprites, new List<int> { 0, 2, 4, 6, 8 }));

            //Run
            var runTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 126, 39);
            var leftSprites = runSprites.Where((sprite, index) => index % 2 != 0);
            var rightSprites = runSprites.Where((sprite, index) => index % 2 == 0);
            Animator.AddAnimation("RunLeft", leftSprites.ToArray());
            Animator.AddAnimation("RunRight", rightSprites.ToArray());

            //Attack
            var attackTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Attack);
            var attackSprites = Sprite.SpritesFromAtlas(attackTexture, 126, 39);
            Animator.AddAnimation("AttackLeft", attackSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            Animator.AddAnimation("AttackRight", attackSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //transition to charge
            var transitionTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Transitiontocharge);
            var transitionSprites = Sprite.SpritesFromAtlas(transitionTexture, 126, 39);
            Animator.AddAnimation("TransitionLeft", transitionSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            Animator.AddAnimation("TransitionRight", transitionSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //charge
            var chargeTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Charge);
            var chargeSprites = Sprite.SpritesFromAtlas(chargeTexture, 126, 39);
            Animator.AddAnimation("ChargeLeft", chargeSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            Animator.AddAnimation("ChargeRight", chargeSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //hit
            var hitTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Hit);
            var hitSprites = Sprite.SpritesFromAtlas(hitTexture, 126, 39);
            Animator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 1, 3 }));
            Animator.AddAnimation("Hit", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 0, 2 }));

            //die
            var dieTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.ChainBot.Death);
            var dieSprites = Sprite.SpritesFromAtlas(dieTexture, 126, 39);
            Animator.AddAnimation("Die", AnimatedSpriteHelper.GetSpriteArrayFromRange(dieSprites, 0, 4));
        }

        public void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<ChainBot>.Begin(this)
                .Selector(AbortTypes.Self)
                    .ConditionalDecorator(c => c.StatusComponent.CurrentStatus.Type == Status.StatusType.Death, true)
                        .Action(c => c.AbortActions())
                    .ConditionalDecorator(c => c.StatusComponent.CurrentStatus.Type == Status.StatusType.Stunned, true)
                        .Action(c => c.AbortActions())
                    .ConditionalDecorator(c =>
                    {
                        var gameStateManager = Game1.GameStateManager;
                        return gameStateManager.GameState != GameState.Combat;
                    }, true)
                        .Sequence()
                            .Action(c => c.AbortActions())
                            .Action(c => c.Idle())
                        .EndComposite()
                    .ConditionalDecorator(c => c.StatusComponent.CurrentStatus.Type == Status.StatusType.Normal)
                        .Sequence() //combat sequence
                            .Selector() //move or attack selector
                                .Sequence(AbortTypes.LowerPriority)
                                    .Conditional(c => c.IsInAttackRange())
                                    .Action(c => c.ExecuteAction(_chainBotMelee))
                                    .Action(c => c.ClearAction())
                                .EndComposite()
                                .Sequence(AbortTypes.LowerPriority)
                                    .Action(c => c.MoveTowardsPlayer())
                                .EndComposite()
                            .EndComposite()
                        .EndComposite()
                .EndComposite()
                .Build();

            _tree.UpdatePeriod = 0;
        }

        #endregion

        public void Update()
        {
            _tree.Tick();
        }

        bool IsInAttackRange()
        {
            if (Vector2.Distance(Entity.Position, PlayerController.Instance.Entity.Position) <= 32)
            {
                return true;
            }
            else return false;
        }

        #region TASKS

        TaskStatus ExecuteAction(EnemyAction<ChainBot> action)
        {
            _activeAction = action;

            return action.Execute();
        }

        TaskStatus ClearAction()
        {
            _activeAction = null;
            return TaskStatus.Success;
        }

        TaskStatus MoveTowardsPlayer()
        {
            //handle animation
            var animation = VelocityComponent.Direction.X >= 0 ? "RunRight" : "RunLeft";
            if (!Animator.IsAnimationActive(animation))
            {
                Animator.Play(animation);
            }

            //follow path
            Pathfinder.FollowPath(PlayerController.Instance.Entity.Position, true);
            VelocityComponent.Move();

            return TaskStatus.Running;
        }

        TaskStatus AbortActions()
        {
            if (_activeAction != null)
            {
                Debug.Log($"Aborting action for enemy with Id: {Id}. Reason: {StatusComponent.CurrentStatus.Type}");
                _activeAction.Abort();
                _activeAction = null;
            }

            return TaskStatus.Success;
        }

        TaskStatus Idle()
        {
            var animation = VelocityComponent.Direction.X >= 0 ? "IdleRight" : "IdleLeft";

            if (!Animator.IsAnimationActive(animation))
            {
                Animator.Play(animation);
            }

            return TaskStatus.Running;
        }

        #endregion
    }
}
