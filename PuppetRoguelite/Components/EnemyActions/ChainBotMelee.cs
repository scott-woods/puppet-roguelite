using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Sprites;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite.Components.EnemyActions
{
    public class ChainBotMelee : EnemyAction
    {
        ChainBot _chainBot;

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

        public ChainBotMelee(ChainBot chainBot)
        {
            _chainBot = chainBot;
        }

        protected override IEnumerator HandleExecution()
        {
            yield return TransitionToCharge();
            ChargeAttack();
            yield return Coroutine.WaitForSeconds(.25f);
            yield return AttackPlayer();

            _isExecutionCompleted = true;
            _isExecuting = false;
        }

        IEnumerator TransitionToCharge()
        {
            var transitionAnimation = _chainBot.VelocityComponent.Direction.X >= 0 ? "TransitionRight" : "TransitionLeft";
            _chainBot.Animator.Play(transitionAnimation, SpriteAnimator.LoopMode.Once);
            while (_chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                yield return null;
            }
        }

        void ChargeAttack()
        {
            var chargeAnimation = _chainBot.VelocityComponent.Direction.X >= 0 ? "ChargeRight" : "ChargeLeft";
            _chainBot.Animator.Play(chargeAnimation);
        }

        IEnumerator AttackPlayer()
        {
            //rotate melee shape
            var shape = new Vector2[_rightMeleeShape.Length];
            Array.Copy(_rightMeleeShape, shape, _rightMeleeShape.Length);
            var offset = new Vector2(_offset.X, _offset.Y);
            if (_chainBot.VelocityComponent.Direction.X < 0)
            {
                for (int i = 0; i < shape.Length; i++)
                {
                    shape[i] *= -1;
                }
                offset *= -1;
            }

            //add hitbox
            _hitboxCollider = _chainBot.Entity.AddComponent(new PolygonCollider(shape));
            _hitboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _hitboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _hitboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _hitboxCollider.LocalOffset += offset;
            _hitbox = _chainBot.Entity.AddComponent(new Hitbox(_hitboxCollider, 3));

            //play animation
            var attackAnimation = _chainBot.VelocityComponent.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
            _chainBot.Animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);

            //yield until animation is completed
            while (_chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_chainBot.Animator.CurrentFrame))
                {
                    _hitbox.Enable();
                }
                else _hitbox.Disable();

                yield return null;
            }

            //remove hitbox components
            _chainBot.Entity.RemoveComponent(_hitbox);
            _chainBot.Entity.RemoveComponent(_hitboxCollider);
        }
    }
}
