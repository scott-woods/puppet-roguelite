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

namespace PuppetRoguelite.Components.Characters.Enemies.ChainBot
{
    public class ChainBot : Enemy<ChainBot>
    {
        public Guid Id = Guid.NewGuid();

        //stats
        float _moveSpeed = 75f;
        int _maxHp = 15;

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
        public Healthbar NewHealthbar { get; set; }
        public KnockbackComponent KnockbackComponent;
        public OriginComponent OriginComponent;
        public DeathComponent DeathComponent;

        #region SETUP

        public ChainBot(Entity mapEntity) : base(mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
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
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 0, Content.Audio.Sounds.Chain_bot_damaged));

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
            DeathComponent = Entity.AddComponent(new DeathComponent(Content.Audio.Sounds.Enemy_death_1, Animator, "Die", "Hit"));

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //actions
            _chainBotMelee = Entity.AddComponent(new ChainBotMelee(this));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new Healthbar(_healthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(150f, .5f, 4, 2f, VelocityComponent, _hurtbox));

            //shadow
            Entity.AddComponent(new Shadow(Animator, new Vector2(4, 9), Vector2.One));
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
            var dieTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Death);
            var dieSprites = Sprite.SpritesFromAtlas(dieTexture, 126, 39);
            Animator.AddAnimation("Die", AnimatedSpriteHelper.GetSpriteArrayFromRange(dieSprites, 0, 4));
        }

        #endregion

        public override BehaviorTree<ChainBot> CreateSubTree()
        {
            var tree = BehaviorTreeBuilder<ChainBot>.Begin(this)
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
            .Build();

            tree.UpdatePeriod = 0;
            return tree;
        }

        bool IsInAttackRange()
        {
            var xDist = Math.Abs(OriginComponent.Origin.X - PlayerController.Instance.OriginComponent.Origin.X);
            var yDist = Math.Abs(OriginComponent.Origin.Y - PlayerController.Instance.OriginComponent.Origin.Y);
            if (xDist <= 16 && yDist <= 8)
            {
                return true;
            }
            return false;
            //if (Vector2.Distance(Entity.Position, PlayerController.Instance.Entity.Position) <= 10)
            //{
            //    return true;
            //}
            //else return false;
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
            var gridGraphManager = MapEntity.GetComponent<GridGraphManager>();

            var leftTarget = PlayerController.Instance.OriginComponent.Origin + new Vector2(-10, 0);
            var rightTarget = PlayerController.Instance.OriginComponent.Origin + new Vector2(10, 0);
            var leftDistance = Vector2.Distance(OriginComponent.Origin, leftTarget);
            var rightDistance = Vector2.Distance(OriginComponent.Origin, rightTarget);

            var target = leftDistance > rightDistance ? rightTarget : leftTarget;
            var altTarget = target == rightTarget ? leftTarget : rightTarget;

            Vector2 finalTarget = PlayerController.Instance.OriginComponent.Origin;

            if (!gridGraphManager.IsPositionInWall(target))
            {
                finalTarget = target;
            }
            else if (!gridGraphManager.IsPositionInWall(altTarget))
            {
                finalTarget = altTarget;
            }

            Pathfinder.FollowPath(finalTarget, true);
            VelocityComponent.Move();

            return TaskStatus.Running;
        }

        public override TaskStatus AbortActions()
        {
            if (_activeAction != null)
            {
                Debug.Log($"Aborting action for enemy with Id: {Id}. Reason: {StatusComponent.CurrentStatus.Type}");
                _activeAction.Abort();
                _activeAction = null;
            }

            return TaskStatus.Success;
        }

        public override TaskStatus Idle()
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
