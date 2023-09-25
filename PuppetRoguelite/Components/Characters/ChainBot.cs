using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters
{
    public class ChainBot : Component, IUpdatable
    {
        //behavior tree
        BehaviorTree<ChainBot> BehaviorTree;

        //components
        Mover _mover;
        SpriteAnimator _animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        PathfindingComponent _pathfinder;
        Collider _collider;
        Hitbox _melee;

        Entity _meleeEntity;

        //properties
        float _moveSpeed = 75f;

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

        public void AddComponents()
        {
            //Mover
            _mover = Entity.AddComponent(new Mover());

            //hitbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(0, -9, 8, 18));
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(10));

            //pathfinding
            _pathfinder = Entity.AddComponent(new PathfindingComponent());

            //collider
            _collider = Entity.AddComponent(new BoxCollider(0, 5, 8, 5));
            _collider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;

            //animator
            _animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //melee
            //var meleeShape = new[]
            //{
            //    new Vector2(10, -5),
            //    new Vector2(45, -5),
            //    new Vector2(50, 0),
            //    new Vector2(45, 5),
            //    new Vector2(10, 5)
            //};
            //_melee = Entity.AddComponent(new Melee(new PolygonCollider(meleeShape), 1));
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

        public void AddAnimations()
        {
            //Idle
            var idleTexture = Entity.Scene.Content.LoadTexture("Content/Characters/ChainBot/idle.png");
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 126, 39);
            _animator.AddAnimation("IdleLeft", new[]
            {
                idleSprites[1],
                idleSprites[3],
                idleSprites[5],
                idleSprites[7],
                idleSprites[9]
            });
            _animator.AddAnimation("IdleRight", new[]
            {
                idleSprites[0],
                idleSprites[2],
                idleSprites[4],
                idleSprites[6],
                idleSprites[8]
            });

            //Run
            var runTexture = Entity.Scene.Content.LoadTexture("Content/Characters/ChainBot/run.png");
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 126, 39);
            var leftSprites = runSprites.Where((sprite, index) => index % 2 != 0);
            var rightSprites = runSprites.Where((sprite, index) => index % 2 == 0);
            _animator.AddAnimation("RunLeft", leftSprites.ToArray());
            _animator.AddAnimation("RunRight", rightSprites.ToArray());

            //Attack
            var attackTexture = Entity.Scene.Content.LoadTexture("Content/Characters/ChainBot/attack.png");
            var attackSprites = Sprite.SpritesFromAtlas(attackTexture, 126, 39);
            _animator.AddAnimation("AttackLeft", attackSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            _animator.AddAnimation("AttackRight", attackSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //transition to charge
            var transitionTexture = Entity.Scene.Content.LoadTexture("Content/Characters/ChainBot/transition to charge.png");
            var transitionSprites = Sprite.SpritesFromAtlas(transitionTexture, 126, 39);
            _animator.AddAnimation("TransitionLeft", transitionSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            _animator.AddAnimation("TransitionRight", transitionSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //charge
            var chargeTexture = Entity.Scene.Content.LoadTexture("Content/Characters/ChainBot/charge.png");
            var chargeSprites = Sprite.SpritesFromAtlas(chargeTexture, 126, 39);
            _animator.AddAnimation("ChargeLeft", chargeSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            _animator.AddAnimation("ChargeRight", chargeSprites.Where((sprite, index) => index % 2 == 0).ToArray());
        }

        #endregion

        public void Update()
        {
            BehaviorTree.Tick();
        }

        public TaskStatus ChasePlayer()
        {
            var player = Entity.Scene.FindComponentOfType<Player>();
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
    }
}
