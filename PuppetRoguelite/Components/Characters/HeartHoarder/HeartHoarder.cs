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

        #region SETUP

        public HeartHoarder(string mapId) : base(mapId)
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

            Collider = Entity.AddComponent(new BoxCollider());
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);

            var hurtboxCollider = Entity.AddComponent(new BoxCollider());
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, .5f, 1));

            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, 75f, new Vector2(1, 0)));

            PathfindingComponent = Entity.AddComponent(new PathfindingComponent(VelocityComponent, MapId));

            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            CombatComponent = Entity.AddComponent(new CombatComponent());
        }

        void AddAnimations()
        {
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.HeartHoarder.Heart_hoarder);
            var sprites = Sprite.SpritesFromAtlas(texture, 222, 119);
            int totalColumns = 36;

            Animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 0, 9, totalColumns));

            Animator.AddAnimation("StationaryAttack", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 1, 9, totalColumns));

            Animator.AddAnimation("StartMovingRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 2, 2, totalColumns));

            Animator.AddAnimation("MoveRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 3, 8, totalColumns));

            Animator.AddAnimation("MoveAttackPrepRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 4, 8, totalColumns));

            Animator.AddAnimation("MoveAttackRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 5, 16, totalColumns));

            Animator.AddAnimation("StopMovingAfterAttackRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 6, 4, totalColumns));

            Animator.AddAnimation("StopMovingRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 7, 4, totalColumns));

            Animator.AddAnimation("Vanish", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 8, 10, totalColumns));

            Animator.AddAnimation("Appear", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 9, 9, totalColumns));

            Animator.AddAnimation("HitRight", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 11, 2, totalColumns));

            Animator.AddAnimation("Death", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 12, 36, totalColumns));
        }

        void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<HeartHoarder>
                .Begin(this)
                    .Selector(AbortTypes.Self)
                        .ConditionalDecorator(h => !h.Hurtbox.IsStunned)
                        .Sequence()
                            .Action(h => h.Idle())
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

        #region TASKS

        TaskStatus Idle()
        {
            if (!Animator.IsAnimationActive("Idle"))
            {
                Animator.Play("Idle");
            }

            return TaskStatus.Success;
        }

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
