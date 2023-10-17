using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.PhysicsShapes;
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

        //components
        Hitbox _hitbox;
        Collider _hitboxCollider;

        Shape _rightMeleeShape = new Polygon(new Vector2[]
        {
            new Vector2(0, -4),
            new Vector2(35, -4),
            new Vector2(40, 0),
            new Vector2(35, 7),
            new Vector2(0, 7)
        });

        Shape _leftMeleeShape = new Polygon(new Vector2[]
        {
            new Vector2(0, -4),
            new Vector2(0, 7),
            new Vector2(-35, 7),
            new Vector2(-40, 0),
            new Vector2(-35, -4)
        });

        Vector2 _offset = new Vector2(8, 0);

        public ChainBotMelee(ChainBot chainBot)
        {
            _chainBot = chainBot;
        }

        public override void Initialize()
        {
            base.Initialize();

            _hitboxCollider = Entity.AddComponent(new PolygonCollider());
            _hitboxCollider.Shape = _rightMeleeShape;
            _hitboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _hitboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _hitboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _hitboxCollider.SetLocalOffset(_offset);
            _components.Add(_hitboxCollider);

            _hitbox = Entity.AddComponent(new Hitbox(_hitboxCollider, 3));
            _components.Add(_hitbox);
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
            while (_chainBot.Animator.IsAnimationActive(transitionAnimation) && _chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
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
            //set direction
            if (_chainBot.VelocityComponent.Direction.X >= 0)
            {
                _hitboxCollider.Shape = _rightMeleeShape;
                _hitboxCollider.SetLocalOffset(_offset);
            }
            else
            {
                _hitboxCollider.Shape = _leftMeleeShape;
                _hitboxCollider.SetLocalOffset(-_offset);
            }

            //play animation
            var attackAnimation = _chainBot.VelocityComponent.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
            _chainBot.Animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);

            //yield until animation is completed
            while (_chainBot.Animator.IsAnimationActive(attackAnimation) && _chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_chainBot.Animator.CurrentFrame))
                {
                    _hitbox.Enable();
                }
                else _hitbox.Disable();

                yield return null;
            }
        }
    }
}
