using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Linq;

namespace PuppetRoguelite.Components.Characters.Enemies.HeartHoarder
{
    public class HeartHoarder : Enemy, IUpdatable
    {
        //behavior tree
        BehaviorTree<HeartHoarder> _tree;

        //components
        public Mover Mover;
        public HealthComponent HealthComponent;
        public BoxCollider Collider;
        public SpriteAnimator Animator;
        public Hurtbox Hurtbox;
        public VelocityComponent VelocityComponent;
        public PathfindingComponent PathfindingComponent;
        public BoxHitbox MovingAttackHitbox;
        public BoxHitbox StationaryAttackHitboxTopLeft, StationaryAttackHitboxBottomLeft, StationaryAttackHitboxTopRight, StationaryAttackHitboxBottomRight;
        public Healthbar NewHealthbar;
        public KnockbackComponent KnockbackComponent;
        public YSorter YSorter;
        public OriginComponent OriginComponent;
        public DollahDropper DollahDropper;
        public DeathComponent DeathComponent;
        public StatusComponent StatusComponent;

        //misc
        float _normalMoveSpeed = 100f;
        float _attackingMoveSpeed = 115f;
        float _vanishedMoveSpeed = 200f;
        int _maxHp = 50;
        bool _inAttackSequence = false;
        float _timeInAttackSequence = 0f;

        int _prevAnimatorFrame = 0;

        #region SETUP

        public HeartHoarder(Entity mapEntity) : base(mapEntity)
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
            StatusComponent = Entity.AddComponent(new StatusComponent(new Status(Status.StatusType.Normal, (int)StatusPriority.Normal)));

            Mover = Entity.AddComponent(new Mover());

            HealthComponent = Entity.AddComponent(new HealthComponent(_maxHp));

            Collider = Entity.AddComponent(new BoxCollider(-13, 35, 26, 16));
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Flags.SetFlagExclusive(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Environment);

            OriginComponent = Entity.AddComponent(new OriginComponent(Collider));

            var hurtboxCollider = Entity.AddComponent(new BoxCollider(-10, 18, 18, 37));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, .1f, Content.Audio.Sounds.Chain_bot_damaged));

            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _normalMoveSpeed));

            PathfindingComponent = Entity.AddComponent(new PathfindingComponent(VelocityComponent, OriginComponent, MapEntity));

            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            NewHealthbar = Entity.AddComponent(new Healthbar(HealthComponent, 48));
            NewHealthbar.SetLocalOffset(new Vector2(0, -24));

            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(65f, .5f, 3, 5f, VelocityComponent, Hurtbox));

            YSorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            DollahDropper = Entity.AddComponent(new DollahDropper(75, 0));

            DeathComponent = Entity.AddComponent(new DeathComponent(Content.Audio.Sounds.Boss_death, Animator, "Die", "Hit"));

            MovingAttackHitbox = Entity.AddComponent(new BoxHitbox(2, new Rectangle(-35, 35, 71, 18)));
            Flags.SetFlagExclusive(ref MovingAttackHitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref MovingAttackHitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            MovingAttackHitbox.SetEnabled(false);

            StationaryAttackHitboxTopLeft = Entity.AddComponent(new BoxHitbox(4, new Rectangle(-110, 27, 81, 12)));
            Flags.SetFlagExclusive(ref StationaryAttackHitboxTopLeft.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref StationaryAttackHitboxTopLeft.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            StationaryAttackHitboxTopLeft.SetEnabled(false);

            StationaryAttackHitboxBottomLeft = Entity.AddComponent(new BoxHitbox(4, new Rectangle(-80, 47, 56, 12)));
            Flags.SetFlagExclusive(ref StationaryAttackHitboxBottomLeft.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref StationaryAttackHitboxBottomLeft.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            StationaryAttackHitboxBottomLeft.SetEnabled(false);

            StationaryAttackHitboxTopRight = Entity.AddComponent(new BoxHitbox(4, new Rectangle(30, 28, 79, 11)));
            Flags.SetFlagExclusive(ref StationaryAttackHitboxTopRight.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref StationaryAttackHitboxTopRight.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            StationaryAttackHitboxTopRight.SetEnabled(false);

            StationaryAttackHitboxBottomRight = Entity.AddComponent(new BoxHitbox(4, new Rectangle(29, 50, 58, 10)));
            Flags.SetFlagExclusive(ref StationaryAttackHitboxBottomRight.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref StationaryAttackHitboxBottomRight.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            StationaryAttackHitboxBottomRight.SetEnabled(false);
        }

        void AddAnimations()
        {
            var texture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.HeartHoarder.Heart_hoarder);
            var leftTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.HeartHoarder.Heart_hoarder_left);
            var sprites = Sprite.SpritesFromAtlas(texture, 222, 119);
            var leftSprites = Sprite.SpritesFromAtlas(texture, 222, 119);
            int totalColumns = 36;

            Animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 0, 9, totalColumns));

            Animator.AddAnimation("StationaryAttack", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 1, 9, totalColumns));

            Animator.AddAnimation("StartMovingRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 2, 2, totalColumns));
            Animator.AddAnimation("StartMovingLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 2, 2, totalColumns));

            Animator.AddAnimation("MoveRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 3, 8, totalColumns));
            Animator.AddAnimation("MoveLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 3, 8, totalColumns));

            Animator.AddAnimation("MoveAttackPrepRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 4, 8, totalColumns));
            Animator.AddAnimation("MoveAttackPrepLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 4, 8, totalColumns));

            Animator.AddAnimation("MoveAttackRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 5, 16, totalColumns));
            Animator.AddAnimation("MoveAttackLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 5, 16, totalColumns));

            Animator.AddAnimation("StopMovingAfterAttackRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 6, 4, totalColumns));
            Animator.AddAnimation("StopMovingAfterAttackLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 6, 4, totalColumns));

            Animator.AddAnimation("StopMovingRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 7, 4, totalColumns));
            Animator.AddAnimation("StopMovingLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 7, 4, totalColumns));

            Animator.AddAnimation("Vanish", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 8, 10, totalColumns));

            Animator.AddAnimation("Appear", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 9, 9, totalColumns));

            Animator.AddAnimation("PostStationaryAttack", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 10, 9, totalColumns));

            Animator.AddAnimation("Hit", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 11, 2, totalColumns));
            Animator.AddAnimation("HitLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 11, 2, totalColumns));

            Animator.AddAnimation("Die", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 12, 36, totalColumns), 7);
        }

        void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<HeartHoarder>
                .Begin(this)
                    .Selector(AbortTypes.Self)
                        .ConditionalDecorator(h => h.StatusComponent.CurrentStatus.Type != Status.StatusType.Normal, true)
                            .Action(h => h.AbortActions())
                        //if not in combat, idle
                        .ConditionalDecorator(h =>
                        {
                            var gameStateManager = Game1.GameStateManager;
                            return gameStateManager.GameState != GameState.Combat;
                        }, true)
                            .Sequence()
                                .Action(h => h.AbortActions())
                                .Action(h => h.Idle())
                            .EndComposite()
                        .ConditionalDecorator(h => h.StatusComponent.CurrentStatus.Type == Status.StatusType.Normal) //if not stunned, select something to do
                            .Sequence() //main sequence
                                .Action(h => h.StartNewAttackSequence())
                                .Selector() //select an attack or move towards player
                                    .Sequence(AbortTypes.LowerPriority) //try to attack
                                        .Conditional(h => h.CanPerformAction())
                                        .Selector()
                                            .Sequence() //try melee attacks
                                                .Conditional(h => h.IsInMeleeRange())
                                                .RandomSelector()
                                                    .Sequence()
                                                        .ParallelSelector()
                                                            .Action(h => h.Idle())
                                                            .WaitAction(.5f)
                                                        .EndComposite()
                                                        .Action(h => h.StationaryAttack())
                                                        .Action(h => h.WaitForAnimation("PostStationaryAttack"))
                                                    .EndComposite()
                                                    .Sequence()
                                                        .Action(h => h.WaitForAnimation("MoveAttackPrepRight"))
                                                        .ParallelSelector()
                                                            .Action(h => h.MovingAttack())
                                                            .WaitAction(5f)
                                                        .EndComposite()
                                                        .Action(h =>
                                                        {
                                                            h.Animator.Speed = 1f;
                                                            return TaskStatus.Success;
                                                        })
                                                        .Action(h => h.WaitForAnimation("StopMovingAfterAttackRight"))
                                                    .EndComposite()
                                                .EndComposite()
                                            .EndComposite()
                                            .Sequence() //try vanish attack
                                                .Conditional(h => h.ShouldPerformVanish())
                                                .Action(h => h.Vanish())
                                                .WaitAction(2f)
                                                .ParallelSelector()
                                                    .WaitAction(4f)
                                                    .Action(h => h.IsInMeleeRange())
                                                    .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _vanishedMoveSpeed))
                                                .EndComposite()
                                                .Action(h => h.Emerge())
                                                .Action(h => h.StationaryAttack())
                                                .Action(h => h.WaitForAnimation("PostStationaryAttack"))
                                            .EndComposite()
                                        .EndComposite()
                                    .EndComposite()
                                    .Sequence(AbortTypes.LowerPriority) //if neither attack possible, move towards player
                                        .Action(h => h.WaitForAnimation("StartMovingRight"))
                                        .ParallelSelector()
                                            .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _normalMoveSpeed))
                                            .Action(h => h.PlayAnimationLoop("MoveRight"))
                                        .EndComposite()
                                    .EndComposite()
                                .EndComposite()
                                .ParallelSelector() //idle for a moment after attack sequence
                                    .Action(h => h.Idle())
                                    .WaitAction(1.5f)
                                .EndComposite()
                            .EndComposite()
                    .EndComposite()
                .Build();

            _tree.UpdatePeriod = 0;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            HealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            DeathComponent.OnDeathFinished += OnDeathFinished;
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            HealthComponent.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            DeathComponent.OnDeathFinished -= OnDeathFinished;
        }

        #endregion

        public void Update()
        {
            _tree.Tick();

            if (_inAttackSequence)
            {
                _timeInAttackSequence += Time.DeltaTime;
            }

            var dir = VelocityComponent.Direction;
            var flip = dir.X < 0;
            Animator.FlipX = flip;
        }

        public TaskStatus Idle()
        {
            if (!Animator.IsAnimationActive("Idle"))
            {
                Animator.Play("Idle");
            }

            return TaskStatus.Running;
        }

        bool CanPerformAction()
        {
            return IsInMeleeRange() || ShouldPerformVanish();
        }

        TaskStatus StartNewAttackSequence()
        {
            _inAttackSequence = true;
            _timeInAttackSequence = 0f;
            return TaskStatus.Success;
        }

        bool IsInMeleeRange()
        {
            var xDist = Math.Abs(Entity.Position.X - PlayerController.Instance.Entity.Position.X);
            var yDist = Math.Abs(Entity.Position.Y - PlayerController.Instance.Entity.Position.Y);
            if (yDist <= 32 && xDist <= 48) return true;
            else return false;
        }

        bool ShouldPerformVanish()
        {
            if (_timeInAttackSequence > 3f)
            {
                return true;
            }
            return false;
        }

        public IEnumerator PlayAppearanceAnimation()
        {
            var status = new Status(Status.StatusType.Frozen, 99);
            StatusComponent.PushStatus(status);
            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Hh_appear);
            Animator.Speed = .75f;
            Animator.Play("Appear", SpriteAnimator.LoopMode.Once);
            while (Animator.IsAnimationActive("Appear") && Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                yield return null;
            }
            StatusComponent.PopStatus(status);
            Animator.Speed = 1f;
        }

        #region TASKS

        TaskStatus AbortActions()
        {
            MovingAttackHitbox.SetEnabled(false);
            StationaryAttackHitboxTopLeft.SetEnabled(false);
            StationaryAttackHitboxBottomLeft.SetEnabled(false);
            StationaryAttackHitboxTopRight.SetEnabled(false);
            StationaryAttackHitboxBottomRight.SetEnabled(false);

            return TaskStatus.Success;
        }

        TaskStatus Emerge()
        {
            if (!Animator.IsAnimationActive("Appear"))
            {
                Game1.AudioManager.PlaySound(Content.Audio.Sounds.Hh_appear);
                Animator.SetColor(Color.White);
                Animator.Speed = 2f;
                Animator.Play("Appear", SpriteAnimator.LoopMode.Once);
                return TaskStatus.Running;
            }

            if (Animator.IsAnimationActive("Appear") && Animator.AnimationState == SpriteAnimator.State.Completed)
            {
                Hurtbox.SetEnabled(true);
                NewHealthbar.SetEnabled(true);
                Animator.Speed = 1f;
                return TaskStatus.Success;
            }
            else return TaskStatus.Running;
        }

        TaskStatus Vanish()
        {
            if (!Animator.IsAnimationActive("Vanish"))
            {
                Game1.AudioManager.PlaySound(Content.Audio.Sounds.Hh_vanish);
                Animator.Play("Vanish", SpriteAnimator.LoopMode.Once);
                Animator.OnAnimationCompletedEvent += OnVanishAnimationCompleted;
                Hurtbox.SetEnabled(false);
                NewHealthbar.SetEnabled(false);
            }

            if (Animator.IsAnimationActive("Vanish") && Animator.AnimationState == SpriteAnimator.State.Completed)
            {
                return TaskStatus.Success;
            }
            else return TaskStatus.Running;
        }

        void OnVanishAnimationCompleted(string animationName)
        {
            Animator.OnAnimationCompletedEvent -= OnVanishAnimationCompleted;
            Animator.SetColor(Color.Transparent);
        }

        TaskStatus StationaryAttack()
        {
            if (!Animator.IsAnimationActive("StationaryAttack"))
            {
                Game1.AudioManager.PlaySound(Content.Audio.Sounds.Hh_stationary_attack);
                Animator.Play("StationaryAttack", SpriteAnimator.LoopMode.Once);
            }

            var activeFramesBottom = new int[] { 1 };
            if (activeFramesBottom.Contains(Animator.CurrentFrame))
            {
                StationaryAttackHitboxBottomLeft.SetEnabled(true);
                StationaryAttackHitboxBottomRight.SetEnabled(true);
            }
            else
            {
                StationaryAttackHitboxBottomLeft.SetEnabled(false);
                StationaryAttackHitboxBottomRight.SetEnabled(false);
            }

            var activeFramesTop = new int[] { 3 };
            if (activeFramesTop.Contains(Animator.CurrentFrame))
            {
                StationaryAttackHitboxTopLeft.SetEnabled(true);
                StationaryAttackHitboxTopRight.SetEnabled(true);
            }
            else
            {
                StationaryAttackHitboxTopLeft.SetEnabled(false);
                StationaryAttackHitboxTopRight.SetEnabled(false);
            }

            if (Animator.IsAnimationActive("StationaryAttack") && Animator.AnimationState == SpriteAnimator.State.Completed)
            {
                return TaskStatus.Success;
            }
            else return TaskStatus.Running;
        }

        TaskStatus MovingAttack()
        {
            MoveTowardsPosition(PlayerController.Instance.Entity.Position, _attackingMoveSpeed);

            if (!Animator.IsAnimationActive("MoveAttackRight"))
            {
                Animator.Play("MoveAttackRight");
                return TaskStatus.Running;
            }

            Animator.Speed += .15f * Time.DeltaTime;

            var activeFrames = new int[] { 2, 9 };
            if (activeFrames.Contains(Animator.CurrentFrame))
            {
                MovingAttackHitbox.SetEnabled(true);
                if (_prevAnimatorFrame != Animator.CurrentFrame)
                {
                    Game1.AudioManager.PlaySound(Content.Audio.Sounds.Hh_slash);
                }
            }
            else MovingAttackHitbox.SetEnabled(false);

            _prevAnimatorFrame = Animator.CurrentFrame;

            return TaskStatus.Running;
        }

        TaskStatus WaitForAnimation(string animationName, float animationSpeed = 1)
        {
            Animator.Speed = animationSpeed;

            if (!Animator.IsAnimationActive(animationName))
            {
                Animator.Play(animationName, SpriteAnimator.LoopMode.Once);
                return TaskStatus.Running;
            }
            else if (Animator.AnimationState != SpriteAnimator.State.Completed) //still running
            {
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        TaskStatus MoveTowardsPosition(Vector2 target, float speed)
        {
            PathfindingComponent.FollowPath(target, true);
            VelocityComponent.Move(speed);
            return TaskStatus.Running;
        }

        TaskStatus PlayAnimationLoop(string animationName, float speed = 1)
        {
            Animator.Speed = speed;

            if (!Animator.IsAnimationActive(animationName))
            {
                Animator.Play(animationName);
            }

            return TaskStatus.Running;
        }

        #endregion

        #region Observers

        void OnHealthDepleted(HealthComponent hc)
        {
            AbortActions();
            NewHealthbar.SetEnabled(false);
        }

        void OnDeathFinished(Entity entity)
        {
            DollahDropper.DropDollahs();
        }

        #endregion
    }
}
