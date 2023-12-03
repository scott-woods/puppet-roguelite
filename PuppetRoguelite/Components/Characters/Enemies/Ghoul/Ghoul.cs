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
using PuppetRoguelite.Tools;
using System;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.Enemies.Ghoul
{
    public class Ghoul : Enemy<Ghoul>
    {
        //stats
        float _moveSpeed = 115f;
        int _maxHp = 9;

        //actions
        EnemyAction<Ghoul> _activeAction;
        GhoulAttack _ghoulAttack;

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

        //misc spawn status
        Status _spawnStatus = new Status(Status.StatusType.Frozen, 99);

        #region SETUP

        public Ghoul(Entity mapEntity) : base(mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
        }

        void AddComponents()
        {
            //actions
            _ghoulAttack = Entity.AddComponent(new GhoulAttack(this));

            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(10, 17));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 0, Content.Audio.Sounds.Chain_bot_damaged));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(_maxHp));

            //dollah dropper
            DollahDropper = Entity.AddComponent(new DollahDropper(1, 0));

            //velocity
            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _moveSpeed));

            //collider
            _collider = Entity.AddComponent(new BoxCollider(9, 6));
            _collider.SetLocalOffset(new Vector2(0, 9));
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

            //sprite flipper
            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(Animator, VelocityComponent));

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Content.Audio.Sounds.Enemy_death_1, Animator, "Die", "Hit"));

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new Healthbar(_healthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(150f, .5f, 4, 2f, VelocityComponent, _hurtbox));

            //shadow
            Entity.AddComponent(new Shadow(Animator, new Vector2(0, 15), new Vector2(.5f, .5f)));
        }

        void AddAnimations()
        {
            var texture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Ghoul.Ghoul_sprites);
            var sprites = Sprite.SpritesFromAtlas(texture, 62, 33);
            Animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 0, 4, 11));
            Animator.AddAnimation("Walk", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 1, 9, 11));
            Animator.AddAnimation("Attack", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 2, 7, 11));
            Animator.AddAnimation("Hit", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 3, 2, 11));
            Animator.AddAnimation("Die", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 4, 8, 11));
            Animator.AddAnimation("Spawn", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 5, 11, 11));
        }

        #endregion

        #region Enemy overrides

        public override TaskStatus Idle()
        {
            if (!Animator.IsAnimationActive("Idle"))
            {
                Animator.Play("Idle");
            }

            return TaskStatus.Running;
        }

        public override BehaviorTree<Ghoul> CreateSubTree()
        {
            var tree = BehaviorTreeBuilder<Ghoul>.Begin(this)
                .Sequence() //combat sequence
                    .Selector() //move or attack selector
                        .Sequence(AbortTypes.LowerPriority)
                            .Conditional(c => c.IsInAttackRange())
                            .Action(c => c.ExecuteAction(_ghoulAttack))
                            .Action(c => c.ClearAction())
                        .EndComposite()
                        .Sequence(AbortTypes.LowerPriority)
                            .Action(c => c.MoveTowardsPlayer())
                        .EndComposite()
                    .EndComposite()
                .EndComposite()
                .Build();

            tree.UpdatePeriod = 0f;

            return tree;
        }

        public override TaskStatus AbortActions()
        {
            if (_activeAction != null)
            {
                _activeAction.Abort();
                _activeAction = null;
            }

            return TaskStatus.Success;
        }

        #endregion

        public override void OnEnabled()
        {
            base.OnEnabled();

            _hurtbox.SetEnabled(false);
            StatusComponent.PushStatus(_spawnStatus);
            Animator.Play("Spawn", SpriteAnimator.LoopMode.Once);
            Animator.OnAnimationCompletedEvent += OnSpawnCompleted;
        }

        bool IsInAttackRange()
        {
            var distance = PlayerController.Instance.Entity.Position - Entity.Position;
            if (Math.Abs(distance.X) <= 16 && Math.Abs(distance.Y) <= 8)
            {
                return true;
            }
            else return false;
        }

        #region TASKS

        TaskStatus ExecuteAction(EnemyAction<Ghoul> action)
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
            if (!Animator.IsAnimationActive("Walk"))
            {
                Animator.Play("Walk");
            }

            Vector2 target = PlayerController.Instance.Entity.Position;
            if (Entity.Position.X - PlayerController.Instance.Entity.Position.X >= 0)
            {
                target.X += 8;
            }
            else target.X -= 8;

            //follow path
            Pathfinder.FollowPath(target, true);
            VelocityComponent.Move();

            return TaskStatus.Running;
        }

        #endregion

        void OnSpawnCompleted(string animationName)
        {
            Animator.OnAnimationCompletedEvent -= OnSpawnCompleted;
            _hurtbox.SetEnabled(true);
            StatusComponent.PopStatus(_spawnStatus);
        }
    }
}
