using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Enemies.Ghoul;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.Enemies.OrbMage
{
    public class OrbMage : Enemy<OrbMage>
    {
        //constants
        const int _maxHp = 12;
        const float _fastMoveSpeed = 40f;
        const float _normalMoveSpeed = 25f;
        const int _attackRange = 192;
        const int _sweepAttackRange = 64;
        const int _minDistanceToPlayer = 48;
        const int _preferredDistanceToPlayer = 80;
        const float _attackPrepTime = 2.5f;
        Vector2 _spriteOffset = new Vector2(38, -5);

        //misc
        float _attackPrepTimer = 0;

        //actions
        EnemyAction<OrbMage> _currentAction;
        OrbMageAttack _orbMageAttack;
        OrbMageSweepAttack _sweepAttack;

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
        public SpriteFlipper SpriteFlipper;

        public OrbMage(Entity mapEntity) : base(mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            //actions
            _orbMageAttack = Entity.AddComponent(new OrbMageAttack(this));
            _sweepAttack = Entity.AddComponent(new OrbMageSweepAttack(this));

            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(9, 24));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 0, Content.Audio.Sounds.Chain_bot_damaged));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(_maxHp));

            //dollah dropper
            DollahDropper = Entity.AddComponent(new DollahDropper(3, 1));

            //velocity
            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _fastMoveSpeed));

            //collider
            _collider = Entity.AddComponent(new BoxCollider(-5, 5, 10, 7));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            _collider.CollidesWithLayers = 0;
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            Flags.SetFlag(ref _collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);

            //origin
            OriginComponent = Entity.AddComponent(new OriginComponent(_collider));

            //pathfinding
            Pathfinder = Entity.AddComponent(new PathfindingComponent(VelocityComponent, OriginComponent, MapEntity));

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());
            Animator.SetLocalOffset(_spriteOffset);
            AddAnimations();

            //sprite flipper
            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(Animator, VelocityComponent));

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Content.Audio.Sounds.Enemy_death_1, Animator, "Death", "Hit"));

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new Healthbar(_healthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(150f, .5f, VelocityComponent, _hurtbox, "Hit"));

            //sturdy
            Entity.AddComponent(new SturdyComponent(6, 2f));

            //shadow
            Entity.AddComponent(new Shadow(Animator, new Vector2(0, 10), Vector2.One));
        }

        void AddAnimations()
        {
            var idleTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 119, 34);
            Animator.AddAnimation("Idle", idleSprites.ToArray());

            var moveTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Move);
            var moveSprites = Sprite.SpritesFromAtlas(moveTexture, 119, 34);
            Animator.AddAnimation("Move", moveSprites.ToArray());

            var attackTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Attack);
            var attackSprites = Sprite.SpritesFromAtlas(attackTexture, 119, 34);
            Animator.AddAnimation("Attack", attackSprites.ToArray());

            var sweepAttackTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Sweepattack);
            var sweepAttackSprites = Sprite.SpritesFromAtlas(sweepAttackTexture, 119, 34);
            Animator.AddAnimation("SweepAttack", sweepAttackSprites.ToArray());

            var hitTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Hit);
            var hitSprites = Sprite.SpritesFromAtlas(hitTexture, 119, 34);
            Animator.AddAnimation("Hit", hitSprites.ToArray());

            var deathTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.OrbMage.Death);
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 119, 34);
            Animator.AddAnimation("Death", deathSprites.ToArray());
        }

        bool IsInAttackRange()
        {
            var distToPlayer = Vector2.Distance(OriginComponent.Origin, PlayerController.Instance.OriginComponent.Origin);
            return distToPlayer <= _attackRange;
        }

        bool IsInSweepAttackRange()
        {
            var distToPlayer = Vector2.Distance(OriginComponent.Origin, PlayerController.Instance.OriginComponent.Origin);
            return distToPlayer <= _sweepAttackRange;
        }

        bool IsTooCloseToPlayer()
        {
            var distToPlayer = Vector2.Distance(OriginComponent.Origin, PlayerController.Instance.OriginComponent.Origin);
            return distToPlayer < _minDistanceToPlayer;
        }

        void MoveToTarget(Vector2 target, float speed)
        {
            //follow path
            Pathfinder.FollowPath(target);
            VelocityComponent.Move(speed);

            //animation
            if (!Animator.IsAnimationActive("Move"))
                Animator.Play("Move");
        }

        void MoveInDirection(Vector2 direction, float speed)
        {
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                VelocityComponent.SetDirection(direction);
            }
            VelocityComponent.Move(speed);

            //animation
            if (!Animator.IsAnimationActive("Move"))
                Animator.Play("Move");
        }

        public override TaskStatus AbortActions()
        {
            _currentAction?.Abort();
            _currentAction = null;

            return TaskStatus.Success;
        }

        public override BehaviorTree<OrbMage> CreateSubTree()
        {
            var tree = BehaviorTreeBuilder<OrbMage>.Begin(this)
                .Sequence()
                    .Action(o => o.StartAttackTimer())
                    .Selector(AbortTypes.Self)
                        .ConditionalDecorator(o => !o.IsInAttackRange(), true)
                            .Sequence()
                                .Action(o => o.MoveTowardsAttackRange())
                            .EndComposite()
                        .ConditionalDecorator(o => o.IsTooCloseToPlayer(), true)
                            .Sequence()
                                .Action(o => o.MoveAwayFromPlayer())
                            .EndComposite()
                        .ConditionalDecorator(o => o.IsInAttackRange(), true)
                            .Sequence()
                                .Action(o => o.WaitToAttack())
                            .EndComposite()
                    .EndComposite()
                    .Selector()
                        .Sequence()
                            .Conditional(o => o.IsInSweepAttackRange())
                            .Action(o => o.ExecuteAction(_sweepAttack))
                        .EndComposite()
                        .Sequence()
                            .Action(o => o.ExecuteAction(_orbMageAttack))
                        .EndComposite()
                    .EndComposite()
                    .Sequence()
                        .ParallelSelector()
                            .Action(o => o.Idle())
                            .WaitAction(1f)
                        .EndComposite()
                    .EndComposite()
                .EndComposite()
                .Build();

            tree.UpdatePeriod = 0;
            return tree;
        }

        public override TaskStatus Idle()
        {
            if (!Animator.IsAnimationActive("Idle"))
                Animator.Play("Idle");

            return TaskStatus.Running;
        }

        TaskStatus MoveAwayFromPlayer()
        {
            //if timer finished, reset timer and return success
            if (_attackPrepTimer >= _attackPrepTime)
            {
                _attackPrepTimer = 0;
                return TaskStatus.Success;
            }

            //increment timer
            _attackPrepTimer += Time.DeltaTime;

            //get direction
            var dir = PlayerController.Instance.OriginComponent.Origin - OriginComponent.Origin;

            //move away from player
            MoveInDirection(dir * -1, _fastMoveSpeed);

            return TaskStatus.Running;
        }

        TaskStatus MoveTowardsAttackRange()
        {
            //move towards player
            MoveToTarget(PlayerController.Instance.OriginComponent.Origin, _normalMoveSpeed);

            return TaskStatus.Running;
        }

        TaskStatus StartAttackTimer()
        {
            _attackPrepTimer = 0;
            return TaskStatus.Success;
        }

        TaskStatus WaitToAttack()
        {
            //return failure for fail conditions
            if (!IsInAttackRange() || IsTooCloseToPlayer())
                return TaskStatus.Failure;

            //increment timer
            _attackPrepTimer += Time.DeltaTime;

            //if timer finished, reset timer and return success
            if (_attackPrepTimer >= _attackPrepTime)
            {
                _attackPrepTimer = 0;
                return TaskStatus.Success;
            }

            //get distance and direction to player
            var distToPlayer = Vector2.Distance(OriginComponent.Origin, PlayerController.Instance.OriginComponent.Origin);
            var dir = PlayerController.Instance.OriginComponent.Origin - OriginComponent.Origin;

            if (dir != Vector2.Zero)
            {
                dir.Normalize();
                VelocityComponent.SetDirection(dir);
            }

            //slowly move towards player if not at preferred distance
            if (distToPlayer > _preferredDistanceToPlayer)
            {
                MoveToTarget(PlayerController.Instance.OriginComponent.Origin, _normalMoveSpeed);
            }
            else
            {
                return Idle();
            }

            return TaskStatus.Running;
        }

        TaskStatus ExecuteAction(EnemyAction<OrbMage> action)
        {
            _currentAction = action;
            return _currentAction.Execute();
        }
    }
}
