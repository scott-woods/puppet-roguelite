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
using System;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.Enemies.Spitter
{
    public class Spitter : Enemy<Spitter>
    {
        //stats
        int _maxHp = 10;
        float _fastMoveSpeed = 75f;
        float _moveSpeed = 50f;
        float _slowMoveSpeed = 25f;

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
        }

        void AddComponents()
        {
            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(1, 0, 11, 20));
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
            Animator.SetLocalOffset(new Vector2(3, -5));

            //y sorter
            YSorter = Entity.AddComponent(new YSorter(Animator, OriginComponent));

            //new healthbar
            NewHealthbar = Entity.AddComponent(new Healthbar(HealthComponent));
            NewHealthbar.SetLocalOffset(new Vector2(0, -25));

            //knockback
            KnockbackComponent = Entity.AddComponent(new KnockbackComponent(200f, .25f, 6, 2f, VelocityComponent, Hurtbox));

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

        public override TaskStatus AbortActions()
        {
            ActiveAction?.Abort();
            ActiveAction = null;
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

        public override BehaviorTree<Spitter> CreateSubTree()
        {
            var tree = BehaviorTreeBuilder<Spitter>.Begin(this)
                .Sequence()
                    .ParallelSelector()
                        .WaitAction(Nez.Random.Range(3f, 5f)) //after certain time, fire no matter where we are
                        .Action(s =>
                        {
                            //get dist to player
                            if (PlayerController.Instance.Entity == null) return TaskStatus.Failure;
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
                    .Action(s => s.ExecuteAction(SpitAttack))
                .EndComposite()
            .Build();

            tree.UpdatePeriod = 0;
            return tree;
        }

        TaskStatus ExecuteAction(EnemyAction<Spitter> action)
        {
            ActiveAction = action;
            return action.Execute();
        }
    }
}
