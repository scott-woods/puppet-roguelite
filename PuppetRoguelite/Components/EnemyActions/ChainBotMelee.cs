using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.PhysicsShapes;
using Nez.Sprites;
using Nez.Systems;
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
        const int _damage = 3;

        ChainBot _chainBot;

        //components
        PolygonHitbox _leftHitbox, _rightHitbox;
        Collider _leftCollider, _rightCollider;

        Vector2 _offset = new Vector2(24, 0);

        public ChainBotMelee(ChainBot chainBot)
        {
            _chainBot = chainBot;
        }

        public override void Initialize()
        {
            base.Initialize();

            //right facing hitbox
            _rightHitbox = Entity.AddComponent(new PolygonHitbox(_damage, new Vector2[]
            {
                new Vector2(0, -4),
                new Vector2(35, -4),
                new Vector2(40, 0),
                new Vector2(35, 7),
                new Vector2(0, 7)
            }));
            Flags.SetFlagExclusive(ref _rightHitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _rightHitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _rightHitbox.SetLocalOffset(_offset);
            _components.Add(_rightHitbox);

            //left facing hitbox
            _leftHitbox = Entity.AddComponent(new PolygonHitbox(_damage, new Vector2[]
            {
                new Vector2(0, -4),
                new Vector2(0, 7),
                new Vector2(-35, 7),
                new Vector2(-40, 0),
                new Vector2(-35, -4)
            }));
            Flags.SetFlagExclusive(ref _leftHitbox.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _leftHitbox.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _leftHitbox.SetLocalOffset(-_offset);
            _components.Add(_leftHitbox);
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
            //select hitbox
            var hitbox = _chainBot.VelocityComponent.Direction.X >= 0 ? _rightHitbox : _leftHitbox;
            hitbox.Direction = _chainBot.VelocityComponent.Direction;

            //play animation
            var attackAnimation = _chainBot.VelocityComponent.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
            _chainBot.Animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);

            //yield until animation is completed
            while (_chainBot.Animator.IsAnimationActive(attackAnimation) && _chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_chainBot.Animator.CurrentFrame))
                {
                    hitbox.SetEnabled(true);
                }
                else hitbox.SetEnabled(false);

                yield return null;
            }
        }
    }
}
