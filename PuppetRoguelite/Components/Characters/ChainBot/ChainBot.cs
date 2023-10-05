using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.ChainBot
{
    public class ChainBot : Component, IUpdatable
    {
        //behavior tree
        BehaviorTree<ChainBot> BehaviorTree;

        Entity _meleeEntity;

        //components
        Mover _mover;
        SpriteAnimator _animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        PathfindingComponent _pathfinder;
        Collider _collider;

        Hitbox _melee;

        //properties
        float _moveSpeed = 75f;
        bool _isHurt = false;

        //misc
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();
        Vector2 _direction = Vector2.One;

        #region SETUP

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            SetupBehaviorTree();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _healthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
        }

        void AddComponents()
        {
            //Mover
            _mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(0, -9, 8, 18));
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(10, 10));

            //pathfinding
            _pathfinder = Entity.AddComponent(new PathfindingComponent());

            //collider
            _collider = Entity.AddComponent(new BoxCollider(0, 5, 8, 5));
            _collider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;

            //animator
            _animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();
        }

        void AddAnimations()
        {
            //Idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 126, 39);
            _animator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArray(idleSprites, new List<int> { 1, 3, 5, 7, 9 }));
            _animator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArray(idleSprites, new List<int> { 0, 2, 4, 6, 8 }));

            //Run
            var runTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 126, 39);
            var leftSprites = runSprites.Where((sprite, index) => index % 2 != 0);
            var rightSprites = runSprites.Where((sprite, index) => index % 2 == 0);
            _animator.AddAnimation("RunLeft", leftSprites.ToArray());
            _animator.AddAnimation("RunRight", rightSprites.ToArray());

            //Attack
            var attackTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Attack);
            var attackSprites = Sprite.SpritesFromAtlas(attackTexture, 126, 39);
            _animator.AddAnimation("AttackLeft", attackSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            _animator.AddAnimation("AttackRight", attackSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //transition to charge
            var transitionTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Transitiontocharge);
            var transitionSprites = Sprite.SpritesFromAtlas(transitionTexture, 126, 39);
            _animator.AddAnimation("TransitionLeft", transitionSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            _animator.AddAnimation("TransitionRight", transitionSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //charge
            var chargeTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Charge);
            var chargeSprites = Sprite.SpritesFromAtlas(chargeTexture, 126, 39);
            _animator.AddAnimation("ChargeLeft", chargeSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            _animator.AddAnimation("ChargeRight", chargeSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //hit
            var hitTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Hit);
            var hitSprites = Sprite.SpritesFromAtlas(hitTexture, 126, 39);
            _animator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 1, 3 }));
            _animator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 0, 2 }));
        }

        public void SetupBehaviorTree()
        {
            BehaviorTree = BehaviorTreeBuilder<ChainBot>.Begin(this)
                .Selector()
                    .Sequence()
                        .Action(enemy => enemy.ChasePlayer())
                        .Action(enemy => enemy.TransitionToCharge())
                        .Action(enemy => enemy.ChargeAttack())
                        .WaitAction(.25f)
                        .Action(enemy => enemy.AttackPlayer())
                        .Action(enemy => enemy.Idle())
                        .WaitAction(.5f)
                    .EndComposite()
                .EndComposite()
                .Build();
            BehaviorTree.UpdatePeriod = 0;
        }

        #endregion

        public void Update()
        {
            if (!_isHurt)
            {
                BehaviorTree.Tick();
            }
        }

        public TaskStatus ChasePlayer()
        {
            var player = Entity.Scene.FindComponentOfType<PlayerController>();
            if (player != null)
            {
                //check if finished
                if (_pathfinder.IsNavigationFinished())
                {
                    return TaskStatus.Success;
                }

                //if close enough to player, return success
                var distanceToPlayer = Vector2.Distance(Entity.Position, player.Entity.Position);
                if (distanceToPlayer < 20)
                {
                    return TaskStatus.Success;
                }

                //get next path position
                _pathfinder.SetTarget(player.Entity.Position);
                var target = _pathfinder.GetNextPosition();

                //determine direction
                _direction = target - Entity.Position;
                _direction.Normalize();

                //handle animation
                var animation = _direction.X >= 0 ? "RunRight" : "RunLeft";
                if (!_animator.IsAnimationActive(animation))
                {
                    _animator.Play(animation);
                }

                //move
                var movement = _direction * _moveSpeed * Time.DeltaTime;
                _mover.CalculateMovement(ref movement, out var result);
                _subPixelV2.Update(ref movement);
                _mover.ApplyMovement(movement);

                return TaskStatus.Running;
            }

            return TaskStatus.Failure;
        }

        public TaskStatus TransitionToCharge()
        {
            if (_animator.CurrentAnimationName != "TransitionRight" && _animator.CurrentAnimationName != "TransitionLeft")
            {
                var transitionAnimation = _direction.X >= 0 ? "TransitionRight" : "TransitionLeft";
                _animator.Play(transitionAnimation, SpriteAnimator.LoopMode.Once);
                return TaskStatus.Running;
            }
            if (_animator.AnimationState != SpriteAnimator.State.Completed)
            {
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        public TaskStatus ChargeAttack()
        {
            var chargeAnimation = _direction.X >= 0 ? "ChargeRight" : "ChargeLeft";
            _animator.Play(chargeAnimation);
            return TaskStatus.Success;
        }

        public TaskStatus AttackPlayer()
        {
            if (_animator.CurrentAnimationName != "AttackRight" && _animator.CurrentAnimationName != "AttackLeft")
            {
                var attackAnimation = _direction.X >= 0 ? "AttackRight" : "AttackLeft";
                _animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);
                var meleeShape = new[]
                {
                    new Vector2(10, -4),
                    new Vector2(45, -4),
                    new Vector2(50, 0),
                    new Vector2(45, 7),
                    new Vector2(10, 7)
                };

                _meleeEntity = Entity.Scene.CreateEntity("chain-bot-melee", Entity.Position);
                var meleeCollider = _meleeEntity.AddComponent(new PolygonCollider(meleeShape));
                meleeCollider.IsTrigger = true;
                meleeCollider.PhysicsLayer = (int)PhysicsLayers.Damage;
                _melee = _meleeEntity.AddComponent(new Hitbox(meleeCollider, 3));
                if (_direction.X < 0)
                {
                    _meleeEntity.RotationDegrees = _direction.X >= 0 ? 0 : 180;
                    _meleeEntity.Position = new Vector2(_meleeEntity.Position.X, _meleeEntity.Position.Y + 7);
                }
                return TaskStatus.Running;
            }
            if (_animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_animator.CurrentFrame))
                {
                    _melee.Enable();
                }
                else _melee.Disable();
                return TaskStatus.Running;
            }

            _meleeEntity.Destroy();
            return TaskStatus.Success;
        }

        public TaskStatus Idle()
        {
            var animation = _direction.X >= 0 ? "IdleRight" : "IdleLeft";
            _animator.Play(animation);
            return TaskStatus.Success;
        }

        void OnDamageTaken(HealthComponent healthComponent)
        {
            var hurtAnimation = _direction.X >= 0 ? "HurtRight" : "HurtLeft";
            _animator.Speed = _animator.Speed / 2;
            int plays = 0;
            _animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                plays += 1;
                if (plays >= 3)
                {
                    _animator.Stop();
                    _animator.OnAnimationCompletedEvent -= handler;
                    _animator.Speed = _animator.Speed * 2;
                    _isHurt = false;
                }
                else
                {
                    _animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
                }
            }
            _animator.OnAnimationCompletedEvent += handler;
            _isHurt = true;
        }

        void OnHealthDepleted(HealthComponent healthComponent)
        {

        }
    }
}
