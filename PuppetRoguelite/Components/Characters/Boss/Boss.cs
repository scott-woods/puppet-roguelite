﻿using Nez.Sprites;
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

namespace PuppetRoguelite.Components.Characters.Boss
{
    public class Boss : Enemy
    {
        //behavior tree
        BehaviorTree<Enemy> BehaviorTree;

        //actions
        List<IEnemyAction> _actions = new List<IEnemyAction>();
        IEnemyAction _nextAction;
        DoubleMeleeAttack _doubleMeleeAttack;

        Entity _meleeEntity;

        //components
        public Mover Mover;
        public SpriteAnimator Animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        public PathfindingComponent Pathfinder;
        Collider _collider;
        YSorter _ySorter;

        Hitbox _melee;

        //properties
        public float MoveSpeed = 75f;
        bool _isHurt = false;
        bool _isActive = true;

        //misc
        public SubpixelVector2 SubPixelV2 = new SubpixelVector2();
        public Vector2 Direction = Vector2.One;

        #region SETUP

        public Boss(string mapId) : base(mapId)
        {

        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            AddComponents();
            SetupBehaviorTree();

            _healthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
        }

        void AddComponents()
        {
            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(0, -9, 8, 18));
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1, new int[] { (int)PhysicsLayers.PlayerDamage }));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(1, 1));

            //pathfinding
            Pathfinder = Entity.AddComponent(new PathfindingComponent(MapId));

            //collider
            _collider = Entity.AddComponent(new BoxCollider(0, 5, 8, 5));

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //actions
            //_doubleMeleeAttack = Entity.AddComponent(new DoubleMeleeAttack(this));
            _actions.Add(_doubleMeleeAttack);

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, 12));
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
            Animator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 0, 2 }));
        }

        public void SetupBehaviorTree()
        {
            BehaviorTree = BehaviorTreeBuilder<Enemy>.Begin(this)
                .Selector()
                    .Sequence(AbortTypes.LowerPriority)
                        .Conditional(e => _isHurt)
                        .Action(e => CancelAction())
                    .EndComposite()
                    .Sequence(AbortTypes.LowerPriority)
                        .Conditional(e => !_isHurt)
                        .Action(enemy => SelectNextAction())
                        .Action(enemy => ExecuteAction())
                        //.SubTree(_nextAction.GetBehaviorTree(this))
                        .Action(enemy => Idle())
                        .WaitAction(.5f)
                    .EndComposite()
                .EndComposite()
                .Build();
            BehaviorTree.UpdatePeriod = 0;
        }

        #endregion

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void Update()
        {
            if (_isActive)
            {
                BehaviorTree.Tick();
            }
            else Idle();
        }

        public TaskStatus SelectNextAction()
        {
            _nextAction = _actions.RandomItem();
            _nextAction.Start();
            return TaskStatus.Success;
        }

        public TaskStatus ExecuteAction()
        {
            if (_nextAction.IsCompleted)
            {
                _nextAction.Reset();
                return TaskStatus.Success;
            }
            else
            {
                var tree = _nextAction.GetBehaviorTree();
                tree.Tick();
                return TaskStatus.Running;
            }
        }

        public TaskStatus CancelAction()
        {
            if (_nextAction != null)
            {
                _nextAction.Reset();
            }

            return TaskStatus.Success;
        }

        public TaskStatus Idle()
        {
            var animation = Direction.X >= 0 ? "IdleRight" : "IdleLeft";
            Animator.Play(animation);
            return TaskStatus.Success;
        }

        void OnDamageTaken(HealthComponent healthComponent)
        {
            var hurtAnimation = Direction.X >= 0 ? "HurtRight" : "HurtLeft";
            //Animator.Speed = Animator.Speed / 2;
            int plays = 0;
            Animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                plays += 1;
                if (plays >= 4)
                {
                    Animator.Stop();
                    Animator.OnAnimationCompletedEvent -= handler;
                    //Animator.Speed = Animator.Speed * 2;
                    _isHurt = false;
                }
                else
                {
                    Animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
                }
            }
            Animator.OnAnimationCompletedEvent += handler;
            _isHurt = true;
        }

        void OnHealthDepleted(HealthComponent healthComponent)
        {
            Entity.Destroy();
        }
    }
}
