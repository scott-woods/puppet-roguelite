using Nez.Sprites;
using Nez;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PuppetRoguelite.Enums;
using Nez.AI.BehaviorTrees;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Tools;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Components.Characters.Player;
using System.Collections;
using PuppetRoguelite.GlobalManagers;

namespace PuppetRoguelite.Components.Characters.HeartHoarder
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
        public NewHealthbar NewHealthbar;
        public KnockbackComponent KnockbackComponent;
        public YSorter YSorter;
        public OriginComponent OriginComponent;
        public DollahDropper DollahDropper;
        public DeathComponent DeathComponent;

        //misc
        float _normalMoveSpeed = 100f;
        float _attackingMoveSpeed = 115f;
        int _maxHp = 1;

        bool _isBehaviorTreeEnabled = false;

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
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, .1f, Nez.Content.Audio.Sounds.Chain_bot_damaged));

            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _normalMoveSpeed));

            PathfindingComponent = Entity.AddComponent(new PathfindingComponent(VelocityComponent, OriginComponent, MapEntity));

            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            NewHealthbar = Entity.AddComponent(new NewHealthbar(HealthComponent, 48));
            NewHealthbar.SetLocalOffset(new Vector2(0, -24));

            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(65f, .5f, 3, 5f, VelocityComponent, Hurtbox));

            YSorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            DollahDropper = Entity.AddComponent(new DollahDropper(75, 0));

            DeathComponent = Entity.AddComponent(new DeathComponent(Nez.Content.Audio.Sounds.Boss_death, Animator, "Die", "Hit"));

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
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.HeartHoarder.Heart_hoarder);
            var leftTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.HeartHoarder.Heart_hoarder_left);
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
                        //if not in combat, idle
                        .ConditionalDecorator(h =>
                        {
                            var gameStateManager = Game1.GameStateManager;
                            return gameStateManager.GameState != GameState.Combat;
                        }) 
                            .Action(h => h.PlayAnimationLoop("Idle"))
                        .ConditionalDecorator(h => h.KnockbackComponent.IsStunned, true)
                            .Action(h => h.AbortActions())
                        .ConditionalDecorator(h => !h.KnockbackComponent.IsStunned, true) //if not stunned, select something to do
                            .Sequence() //main sequence
                                .ParallelSelector() //move towards player for time or until within certain distance
                                    .Conditional(h => Math.Abs(Vector2.Distance(h.OriginComponent.Origin, PlayerController.Instance.Entity.Position)) < 24)
                                    .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _normalMoveSpeed))
                                    .Sequence()
                                        .Action(h => h.WaitForAnimation("StartMovingRight"))
                                        .Action(h => h.PlayAnimationLoop("MoveRight"))
                                    .EndComposite()
                                    .WaitAction(2f)
                                .EndComposite()
                                .Selector() //select an action
                                    .ConditionalDecorator(h =>
                                    {
                                        var xDist = Math.Abs(h.Entity.Position.X - PlayerController.Instance.Entity.Position.X);
                                        var yDist = Math.Abs(h.Entity.Position.Y - PlayerController.Instance.Entity.Position.Y);
                                        if (yDist <= 32 && xDist <= 64) return true;
                                        else return false;
                                    }, false)
                                        .Sequence() //stationary attack
                                            .ParallelSelector()
                                                .Conditional(h => Math.Abs(Vector2.Distance(h.OriginComponent.Origin, PlayerController.Instance.Entity.Position)) < 32)
                                                .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _normalMoveSpeed))
                                                .Sequence()
                                                    .Action(h => h.WaitForAnimation("StartMovingRight"))
                                                    .Action(h => h.PlayAnimationLoop("MoveRight"))
                                                .EndComposite()
                                            .EndComposite()
                                            .ParallelSelector()
                                                .Action(h => h.PlayAnimationLoop("Idle"))
                                                .WaitAction(.2f)
                                            .EndComposite()
                                            .Action(h => h.StationaryAttack())
                                            .Action(h => h.WaitForAnimation("PostStationaryAttack"))
                                        .EndComposite()
                                    .RandomSelector()
                                        .Sequence() //vanish attack
                                            .Action(h => h.Vanish())
                                            .ParallelSelector()
                                                .Conditional(h =>
                                                {
                                                    var xDist = Math.Abs(h.OriginComponent.Origin.X - PlayerController.Instance.Entity.Position.X);
                                                    var yDist = Math.Abs(h.OriginComponent.Origin.Y - PlayerController.Instance.Entity.Position.Y);
                                                    if (xDist <= 24 && yDist <= 24)
                                                    {
                                                        return true;
                                                    }
                                                    else return false;
                                                })
                                                .Action(h =>
                                                {
                                                    var xDist = PlayerController.Instance.Entity.Position.X - h.OriginComponent.Origin.X;
                                                    Vector2 target;
                                                    if (xDist > 0)
                                                    {
                                                        target = PlayerController.Instance.Entity.Position + new Vector2(32, 0);
                                                    }
                                                    else target = PlayerController.Instance.Entity.Position - new Vector2(32, 0);
                                                    return h.MoveTowardsPosition(target, _attackingMoveSpeed * 1.5f);
                                                })
                                                .Sequence()
                                                    .Action(h => h.WaitForAnimation("StartMovingRight"))
                                                    .Action(h => h.PlayAnimationLoop("MoveRight"))
                                                .EndComposite()
                                                .WaitAction(5f)
                                            .EndComposite()
                                            .Action(h => h.Emerge())
                                            .Action(h => h.StationaryAttack())
                                            .Action(h => h.WaitForAnimation("PostStationaryAttack"))
                                        .EndComposite()
                                        .Sequence() //moving attack sequence
                                            .Action(h => h.WaitForAnimation("MoveAttackPrepRight", 1f))
                                            .ParallelSelector()
                                                .Action(h => h.MovingAttack())
                                                .WaitAction(6f)
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
                                .ParallelSelector() //idle after action
                                    .Action(h => h.PlayAnimationLoop("Idle"))
                                    .WaitAction(1f)
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
            if (_isBehaviorTreeEnabled)
            {
                _tree.Tick();
            }

            var dir = VelocityComponent.Direction;
            var flip = dir.X < 0;
            Animator.FlipX = flip;
        }

        public void SetBehaviorTreeEnabled(bool enabled)
        {
            _isBehaviorTreeEnabled = enabled;
        }

        public IEnumerator PlayAppearanceAnimation()
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Hh_appear);
            Animator.Speed = .75f;
            Animator.Play("Appear", SpriteAnimator.LoopMode.Once);
            while (Animator.IsAnimationActive("Appear") && Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                yield return null;
            }
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
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Hh_appear);
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
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Hh_vanish);
                Animator.Play("Vanish", SpriteAnimator.LoopMode.Once);
                Hurtbox.SetEnabled(false);
                NewHealthbar.SetEnabled(false);
            }

            if (Animator.IsAnimationActive("Vanish") && Animator.AnimationState == SpriteAnimator.State.Completed)
            {
                Animator.SetColor(Color.Transparent);
                return TaskStatus.Success;
            }
            else return TaskStatus.Running;
        }

        TaskStatus StationaryAttack()
        {
            if (!Animator.IsAnimationActive("StationaryAttack"))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Hh_stationary_attack);
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
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Hh_slash);
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
            _isBehaviorTreeEnabled = false;
        }

        void OnDeathFinished(Entity entity)
        {
            DollahDropper.DropDollahs();
        }

        #endregion
    }
}
