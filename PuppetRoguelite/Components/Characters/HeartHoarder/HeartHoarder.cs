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
        public CombatComponent CombatComponent;

        //misc
        float _normalMoveSpeed = 50f;
        float _attackingMoveSpeed = 80f;

        bool _isActive = false;
        bool _isInCombat = false;

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

            HealthComponent = Entity.AddComponent(new HealthComponent(10, 10));
            HealthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);

            Collider = Entity.AddComponent(new BoxCollider(-13, 35, 26, 16));
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Flags.SetFlagExclusive(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Environment);

            var hurtboxCollider = Entity.AddComponent(new BoxCollider());
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, .5f, 1));

            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _normalMoveSpeed, new Vector2(1, 0)));

            PathfindingComponent = Entity.AddComponent(new PathfindingComponent(VelocityComponent, MapEntity));
            PathfindingComponent.Offset = new Vector2(0, Collider.LocalOffset.Y + (Collider.Height / 2));

            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            CombatComponent = Entity.AddComponent(new CombatComponent());
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

            Animator.AddAnimation("HitRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 11, 2, totalColumns));
            Animator.AddAnimation("HitLeft", AnimatedSpriteHelper.GetSpriteArrayByRow(leftSprites, 11, 2, totalColumns));

            Animator.AddAnimation("Death", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 12, 36, totalColumns));
        }

        void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<HeartHoarder>
                .Begin(this)
                    .Selector(AbortTypes.Self)
                        .ConditionalDecorator(h => !h._isInCombat)
                        .Action(h => h.PlayAnimationLoop("Idle"))
                        .ConditionalDecorator(h => !h.Hurtbox.IsStunned, true)
                        .RandomSelector()
                            .Sequence()
                                .ParallelSelector()
                                    .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _normalMoveSpeed))
                                    .Action(h => h.WaitForAnimation("StartMovingRight"))
                                .EndComposite()
                                .ParallelSelector() //move towards player for a few seconds
                                    .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _normalMoveSpeed))
                                    .Action(h => h.PlayAnimationLoop("MoveRight"))
                                    .Conditional(h => Math.Abs(Vector2.Distance(h.Entity.Position, PlayerController.Instance.Entity.Position)) < 32)
                                    .WaitAction(3f)
                                .EndComposite()
                                .Action(h => h.WaitForAnimation("MoveAttackPrepRight", .5f))
                                .ParallelSelector()
                                    .Action(h => h.MoveTowardsPosition(PlayerController.Instance.Entity.Position, _attackingMoveSpeed))
                                    .Action(h => h.PlayAnimationLoop("MoveAttackRight"))
                                    .WaitAction(6f)
                                .EndComposite()
                                .Action(h => h.WaitForAnimation("StopMovingAfterAttackRight"))
                                .ParallelSelector()
                                    .Action(h => h.PlayAnimationLoop("Idle"))
                                    .WaitAction(1f)
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
            if (_isActive)
            {
                _tree.Tick();
            }

            var dir = VelocityComponent.Direction;
            var flip = dir.X < 0;
            Animator.FlipX = flip;
        }

        public IEnumerator PlayAppearanceAnimation()
        {
            Animator.Play("Appear", SpriteAnimator.LoopMode.Once);
            while (Animator.IsAnimationActive("Appear") && Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                yield return null;
            }
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void StartCombat()
        {
            _isInCombat = true;
        }

        #region TASKS

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

        //TaskStatus ChargeMovingAttack()
        //{
        //    if (!Animator.IsAnimationActive("MoveAttackPrepRight"))
        //    {
        //        Animator.Play("MoveAttackPrepRight", SpriteAnimator.LoopMode.Once);
        //        return TaskStatus.Running;
        //    }
        //    else if (Animator.AnimationState != SpriteAnimator.State.Completed)
        //    {
        //        return TaskStatus.Running;
        //    }

        //    return TaskStatus.Success;
        //}

        #endregion

        #region Observers

        void OnDamageTaken(HealthComponent healthComponent)
        {
            if (!Animator.IsAnimationActive("HitRight"))
            {
                Animator.Play("HitRight");
            }
        }

        #endregion
    }
}
