﻿using Microsoft.Xna.Framework;
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
using Serilog;
using System;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.Enemies.Spitter
{
    public class Spitter : Enemy<Spitter>
    {
        //stats
        int _maxHp = 12;
        float _fastMoveSpeed = 75f;
        float _moveSpeed = 50f;
        float _slowMoveSpeed = 25f;
        float _cooldown = 3f;

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
        public Healthbar NewHealthbar;
        public KnockbackComponent KnockbackComponent;
        public SpriteFlipper SpriteFlipper;
        public OriginComponent OriginComponent;
        public DollahDropper DollahDropper;
        public DeathComponent DeathComponent;

        //actions
        public SpitAttack SpitAttack;

        //misc
        float _cooldownTimer;

        public Spitter(Entity mapEntity) : base(mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            AddAnimations();
            AddActions();

            _cooldownTimer = _cooldown;
        }

        void AddComponents()
        {
            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(11, 20));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 0, Content.Audio.Sounds.Chain_bot_damaged));

            //health
            HealthComponent = Entity.AddComponent(new HealthComponent(_maxHp));

            //dollah dropper
            DollahDropper = Entity.AddComponent(new DollahDropper(3, 1));

            //velocity
            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _moveSpeed));

            //collider
            Collider = Entity.AddComponent(new BoxCollider(-4, 8, 6, 6));
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Collider.CollidesWithLayers = 0;
            Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.Environment);
            //Flags.SetFlag(ref Collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);

            OriginComponent = Entity.AddComponent(new OriginComponent(Collider));

            //pathfinding
            Pathfinder = Entity.AddComponent(new PathfindingComponent(VelocityComponent, OriginComponent, MapEntity));

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());
            Animator.SetLocalOffset(new Vector2(2, -6));

            //y sorter
            YSorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new Healthbar(HealthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(200f, .25f, VelocityComponent, Hurtbox, "Hit"));

            //sturdy
            Entity.AddComponent(new SturdyComponent(6, 2f));

            //sprite flipper
            SpriteFlipper = Entity.AddComponent(new SpriteFlipper(Animator, VelocityComponent));

            //death
            DeathComponent = Entity.AddComponent(new DeathComponent(Content.Audio.Sounds.Enemy_death_1, Animator, "Die", "Hit"));

            //shadow
            Entity.AddComponent(new Shadow(Animator, new Vector2(-1, 13), Vector2.One));
        }

        void AddAnimations()
        {
            var texture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.Spitter.Spitter_sheet);
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

        public override BehaviorTree<Spitter> CreateSubTree()
        {
            var tree = BehaviorTreeBuilder<Spitter>.Begin(this)
                .Selector(AbortTypes.Self)
                    .ConditionalDecorator(s => !s.HasLineOfSight() || !s.CanAttack(), false)
                        .Sequence()
                            .Action(s => s.Move())
                            .ParallelSelector()
                                .WaitAction(1f)
                                .Action(s => s.Idle())
                            .EndComposite()
                            .Action(s => s.ExecuteAction(SpitAttack))
                            .Action(s => s.ResetTimer())
                        .EndComposite()
                .EndComposite()
                .Build();

            tree.UpdatePeriod = 0;
            return tree;
        }

        bool HasLineOfSight()
        {
            var cast = Physics.Linecast(Entity.Position, GetTargetPosition(), 1 << (int)PhysicsLayers.Environment);
            return cast.Collider == null;
        }

        bool CanAttack()
        {
            return _cooldownTimer <= 0;
        }

        #region TASKS

        TaskStatus ResetTimer()
        {
            _cooldownTimer = _cooldown;
            return TaskStatus.Success;
        }

        public override TaskStatus Idle()
        {
            if (!Animator.IsAnimationActive("Idle"))
            {
                Animator.Play("Idle");
            }

            return TaskStatus.Running;
        }

        TaskStatus Move()
        {
            Log.Debug("Spitter starting Move Task");

            if (CanAttack())
            {
                return TaskStatus.Success;
            }

            //handle cooldown timer
            if (HasLineOfSight())
            {
                Log.Debug("Spitter has Line of Sight, decrementing cooldown timer");
                _cooldownTimer -= Time.DeltaTime;
            }
            else
            {
                var targetPos = GetTargetPosition();
                Log.Debug("Spitter does not have Line of Sight.");
                Log.Debug("Spitter following Path towards " + targetPos);
                Pathfinder.FollowPath(targetPos);
                Log.Debug("Spitter moving at " + _fastMoveSpeed + " to get Line of Sight");
                VelocityComponent.Move(_fastMoveSpeed);
                if (!Animator.IsAnimationActive("Move"))
                    Animator.Play("Move");
                return TaskStatus.Running;
            }

            //get dist to player
            if (PlayerController.Instance.Entity == null) return TaskStatus.Failure;
            var dist = Math.Abs(Vector2.Distance(GetTargetPosition(), Entity.Position));

            //determine where to move and how fast
            float speed = 0;
            Vector2 direction;
            Log.Debug("Spitter distance to Player: " + dist);
            if (dist <= 64) //player too close
            {
                Log.Debug("Player too close to Spitter");
                speed = _fastMoveSpeed;
                direction = Entity.Position - GetTargetPosition();
            }
            else if (dist >= 96 && dist <= 128)
            {
                Log.Debug("Player slight far from Spitter, moving slowly towards Player");
                speed = _slowMoveSpeed;
                direction = GetTargetPosition() - Entity.Position;
            }
            else if (dist >= 192)
            {
                Log.Debug("Player too far from Spitter");
                speed = _moveSpeed;
                direction = GetTargetPosition() - Entity.Position;
            }
            else
            {
                Log.Debug("Player at right distance from Spitter, Spitter not moving.");
                speed = 0;
                direction = GetTargetPosition() - Entity.Position;
            }

            //normalize and set direction
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                VelocityComponent.SetDirection(direction);
            }

            //play idle or move animation
            var anim = speed == 0 ? "Idle" : "Move";
            if (Animator.CurrentAnimationName != anim)
            {
                Animator.Play(anim);
            }

            //move
            if (speed != 0)
            {
                Log.Debug("Spitter moving at speed " + speed);
                VelocityComponent.Move(speed);
            }

            return TaskStatus.Running;
        }

        #endregion
    }
}
