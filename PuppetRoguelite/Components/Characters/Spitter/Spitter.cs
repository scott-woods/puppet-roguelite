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

namespace PuppetRoguelite.Components.Characters.Spitter
{
    public class Spitter : Enemy, IUpdatable
    {
        //stats
        int _maxHp = 10;
        float _fastMoveSpeed = 75f;
        float _moveSpeed = 50f;
        float _slowMoveSpeed = 25f;

        //behavior tree
        BehaviorTree<Spitter> _tree;

        //components
        public Mover Mover;
        public SpriteAnimator Animator;
        public Hurtbox Hurtbox;
        public HealthComponent HealthComponent;
        public PathfindingComponent Pathfinder;
        public BoxCollider Collider;
        public YSorter YSorter;
        public VelocityComponent VelocityComponent;
        public Healthbar Healthbar;
        public NewHealthbar NewHealthbar;
        public KnockbackComponent KnockbackComponent;
        public SpriteFlipper SpriteFlipper;
        public OriginComponent OriginComponent;
        public DollahDropper DollahDropper;
        public DeathComponent DeathComponent;
        public StatusComponent StatusComponent;

        //actions
        public SpitAttack SpitAttack;
        public EnemyAction<Spitter> ActiveAction;

        public Spitter(Entity mapEntity) : base(mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            AddAnimations();
            AddActions();
            SetupBehaviorTree();
        }

        void AddComponents()
        {
            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(-5, -3, 11, 20));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 0, Nez.Content.Audio.Sounds.Chain_bot_damaged));

            //status
            StatusComponent = Entity.AddComponent(new StatusComponent(new Status(Status.StatusType.Normal, (int)StatusPriority.Normal)));

            //health
            HealthComponent = Entity.AddComponent(new HealthComponent(_maxHp));

            //dollah dropper
            DollahDropper = Entity.AddComponent(new DollahDropper(3, 1));

            //velocity
            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _moveSpeed));

            //collider
            Collider = Entity.AddComponent(new BoxCollider(-5, 13, 9, 6));
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Flags.UnsetFlag(ref Collider.CollidesWithLayers, -1);
            Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);

            OriginComponent = Entity.AddComponent(new OriginComponent(Collider));

            //pathfinding
            Pathfinder = Entity.AddComponent(new PathfindingComponent(VelocityComponent, OriginComponent, MapEntity));

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());

            //y sorter
            YSorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new NewHealthbar(HealthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(200f, .25f, 6, 2f, VelocityComponent, Hurtbox));

            //sprite flipper
            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(Animator, VelocityComponent));

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Nez.Content.Audio.Sounds.Enemy_death_1, Animator, "Die", "Hit"));
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            SpriteFlipper.Emitter.AddObserver(SpriteFlipperEvents.Flipped, OnSpriteFlipped);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            SpriteFlipper.Emitter.RemoveObserver(SpriteFlipperEvents.Flipped, OnSpriteFlipped);
        }

        void AddAnimations()
        {
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Spitter.Spitter_sheet);
            var sprites = Sprite.SpritesFromAtlas(texture, 77, 39);

            Animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 0, 6, 9));
            Animator.AddAnimation("Move", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 1, 7, 9));
            Animator.AddAnimation("Attack", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 2, 8, 9));
            Animator.AddAnimation("Hit", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 3, 3, 9));
            Animator.AddAnimation("Die", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 4, 9, 9));
        }

        void AddActions()
        {
            SpitAttack = Entity.AddComponent(new SpitAttack(this));
        }

        void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<Spitter>.Begin(this)
                .Selector(AbortTypes.Self)
                    .ConditionalDecorator(s => s.StatusComponent.CurrentStatus.Type == Status.StatusType.Death, true)
                        .Action(s => s.AbortActions())
                    .ConditionalDecorator(s => s.StatusComponent.CurrentStatus.Type == Status.StatusType.Stunned, true)
                        .Action(s => s.AbortActions())
                    .ConditionalDecorator(s =>
                    {
                        var gameStateManager = Game1.GameStateManager;
                        return gameStateManager.GameState != GameState.Combat;
                    })
                        .Sequence()
                            .Action(s => s.AbortActions())
                            .Action(s => s.Idle())
                        .EndComposite()
                    .ConditionalDecorator(s => s.StatusComponent.CurrentStatus.Type == Status.StatusType.Normal) //main sequence
                        .Sequence()
                            .ParallelSelector()
                                .WaitAction(Nez.Random.Range(3f, 5f)) //after certain time, fire no matter where we are
                                .Action(s =>
                                {
                                    //get dist to player
                                    var dist = Math.Abs(Vector2.Distance(PlayerController.Instance.Entity.Position, Entity.Position));

                                    //determine where to move and how fast
                                    float speed = 0;
                                    Vector2 direction;
                                    if (dist <= 64) //player too close
                                    {
                                        speed = _fastMoveSpeed;
                                        direction = Entity.Position - PlayerController.Instance.Entity.Position;
                                    }
                                    else if (dist >= 96 && dist <= 128)
                                    {
                                        speed = _slowMoveSpeed;
                                        direction = PlayerController.Instance.Entity.Position - Entity.Position;
                                    }
                                    else if (dist >= 192)
                                    {
                                        speed = _moveSpeed;
                                        direction = PlayerController.Instance.Entity.Position - Entity.Position;
                                    }
                                    else
                                    {
                                        speed = 0;
                                        direction = PlayerController.Instance.Entity.Position - Entity.Position;
                                    }

                                    //normalize and set direction
                                    direction.Normalize();
                                    VelocityComponent.SetDirection(direction);

                                    //play idle or move animation
                                    var anim = speed == 0 ? "Idle" : "Move";
                                    if (Animator.CurrentAnimationName != anim)
                                    {
                                        Animator.Play(anim);
                                    }

                                    //move
                                    if (speed != 0)
                                    {
                                        VelocityComponent.Move(speed);
                                    }

                                    return TaskStatus.Running;
                                })
                            .EndComposite()
                            .ParallelSelector()
                                .WaitAction(1f)
                                .Action(s =>
                                {
                                    var dir = PlayerController.Instance.Entity.Position - Entity.Position;
                                    VelocityComponent.SetDirection(dir);
                                    if (Animator.CurrentAnimationName != "Idle")
                                    {
                                        Animator.Play("Idle");
                                    }
                                    return TaskStatus.Running;
                                })
                            .EndComposite()
                            //execute attack
                            .Action(s => s.SpitAttack.Execute())
                        .EndComposite()
                .EndComposite()
                .Build();

            _tree.UpdatePeriod = 0;
        }

        public void Update()
        {
            _tree.Tick();
        }

        TaskStatus AbortActions()
        {
            ActiveAction?.Abort();
            ActiveAction = null;
            return TaskStatus.Success;
        }

        TaskStatus Idle()
        {
            if (!Animator.IsAnimationActive("Idle"))
            {
                Animator.Play("Idle");
            }

            return TaskStatus.Running;
        }

        void OnSpriteFlipped(bool flipped)
        {
            var newOffsetX = Collider.LocalOffset.X * -1;
            var newOffset = new Vector2(newOffsetX, Collider.LocalOffset.Y);
            Collider.SetLocalOffset(newOffset);

            var newHurtboxOffsetX = Hurtbox.Collider.LocalOffset.X * -1;
            var newHurtboxOffset = new Vector2(newHurtboxOffsetX, Hurtbox.Collider.LocalOffset.Y);
            Hurtbox.Collider.SetLocalOffset(newHurtboxOffset);
        }
    }
}
