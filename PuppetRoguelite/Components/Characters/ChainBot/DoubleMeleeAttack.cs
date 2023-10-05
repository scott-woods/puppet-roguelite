using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.PhysicsShapes;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite.Components.Characters.ChainBot
{
    public class DoubleMeleeAttack : Component, IEnemyAction
    {
        bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
        }

        BehaviorTree<Enemy> _behaviorTree;

        Hitbox _hitbox;
        Collider _hitboxCollider;

        Vector2[] _rightMeleeShape = new[]
        {
            new Vector2(0, -4),
            new Vector2(35, -4),
            new Vector2(40, 0),
            new Vector2(35, 7),
            new Vector2(0, 7)
        };

        Vector2 _offset = new Vector2(8, 0);

        ChainBot _chainBot;

        public DoubleMeleeAttack(ChainBot chainBot)
        {
            _chainBot = chainBot;
        }

        public void Start()
        {
            _isCompleted = false;

            _behaviorTree = BehaviorTreeBuilder<Enemy>.Begin(_chainBot)
                .Sequence()
                    .Action(enemy => ChasePlayer())
                    .Action(enemy => TransitionToCharge())
                    .Action(enemy => ChargeAttack())
                    .WaitAction(.25f)
                    .Action(enemy => AttackPlayer())
                    .Action(enemy => _isCompleted = true)
                .EndComposite()
                .Build();
            _behaviorTree.UpdatePeriod = 0;
        }

        public void Reset()
        {
            _isCompleted = false;
            if (_hitbox != null) Entity.RemoveComponent(_hitbox);
            if (_hitboxCollider != null) Entity.RemoveComponent(_hitboxCollider);
        }

        public BehaviorTree<Enemy> GetBehaviorTree()
        {
            return _behaviorTree;
        }

        public TaskStatus ChasePlayer()
        {
            var player = PlayerController.Instance;
            if (player != null)
            {
                //check if finished
                if (_chainBot.Pathfinder.IsNavigationFinished())
                {
                    return TaskStatus.Success;
                }

                //if close enough to player, return success
                var distanceToPlayer = Vector2.Distance(_chainBot.Entity.Position, player.Entity.Position);
                if (distanceToPlayer < 20)
                {
                    return TaskStatus.Success;
                }

                //get next path position
                _chainBot.Pathfinder.SetTarget(player.Entity.Position);
                var target = _chainBot.Pathfinder.GetNextPosition();

                //determine direction
                _chainBot.Direction = target - _chainBot.Entity.Position;
                _chainBot.Direction.Normalize();

                //handle animation
                var animation = _chainBot.Direction.X >= 0 ? "RunRight" : "RunLeft";
                if (!_chainBot.Animator.IsAnimationActive(animation))
                {
                    _chainBot.Animator.Play(animation);
                }

                //move
                var movement = _chainBot.Direction * _chainBot.MoveSpeed * Time.DeltaTime;
                _chainBot.Mover.CalculateMovement(ref movement, out var result);
                _chainBot.SubPixelV2.Update(ref movement);
                _chainBot.Mover.ApplyMovement(movement);

                return TaskStatus.Running;
            }

            return TaskStatus.Failure;
        }

        public TaskStatus TransitionToCharge()
        {
            if (_chainBot.Animator.CurrentAnimationName != "TransitionRight" && _chainBot.Animator.CurrentAnimationName != "TransitionLeft")
            {
                var transitionAnimation = _chainBot.Direction.X >= 0 ? "TransitionRight" : "TransitionLeft";
                _chainBot.Animator.Play(transitionAnimation, SpriteAnimator.LoopMode.Once);
                return TaskStatus.Running;
            }
            if (_chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        public TaskStatus ChargeAttack()
        {
            var chargeAnimation = _chainBot.Direction.X >= 0 ? "ChargeRight" : "ChargeLeft";
            _chainBot.Animator.Play(chargeAnimation);
            return TaskStatus.Success;
        }

        public TaskStatus AttackPlayer()
        {
            if (_chainBot.Animator.CurrentAnimationName != "AttackRight" && _chainBot.Animator.CurrentAnimationName != "AttackLeft")
            {
                var attackAnimation = _chainBot.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
                _chainBot.Animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);

                //rotate melee shape
                var shape = new Vector2[_rightMeleeShape.Length];
                Array.Copy(_rightMeleeShape, shape, _rightMeleeShape.Length);
                var offset = new Vector2(_offset.X, _offset.Y);
                if (_chainBot.Direction.X < 0)
                {
                    for (int i = 0; i < shape.Length; i++)
                    {
                        //shape[i] = Mathf.RotateAround(shape[i], Vector2.Zero, 180);
                        shape[i] *= -1;
                    }
                    offset *= -1;
                }
                _hitboxCollider = Entity.AddComponent(new PolygonCollider(shape));
                _hitboxCollider.IsTrigger = true;
                _hitboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyDamage;
                _hitboxCollider.LocalOffset += offset;
                _hitbox = Entity.AddComponent(new Hitbox(_hitboxCollider, 3, new int[] { (int)PhysicsLayers.PlayerHurtbox }));

                return TaskStatus.Running;
            }
            if (_chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_chainBot.Animator.CurrentFrame))
                {
                    _hitbox.Enable();
                }
                else _hitbox.Disable();
                return TaskStatus.Running;
            }

            Entity.RemoveComponent(_hitbox);
            Entity.RemoveComponent(_hitboxCollider);
            return TaskStatus.Success;
        }
    }
}
