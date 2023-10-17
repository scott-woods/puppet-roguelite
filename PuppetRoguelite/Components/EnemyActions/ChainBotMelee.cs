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
        ChainBot _chainBot;

        //components
        Hitbox _leftHitbox, _rightHitbox;
        Collider _leftCollider, _rightCollider;

        Vector2 _offset = new Vector2(8, 0);

        public ChainBotMelee(ChainBot chainBot)
        {
            _chainBot = chainBot;
        }

        public override void Initialize()
        {
            base.Initialize();

            //right facing hitbox
            _rightCollider = Entity.AddComponent(new PolygonCollider(new Vector2[]
            {
                new Vector2(0, -4),
                new Vector2(35, -4),
                new Vector2(40, 0),
                new Vector2(35, 7),
                new Vector2(0, 7)
            }));
            _rightCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _rightCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _rightCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _rightCollider.SetLocalOffset(_offset);
            _components.Add(_rightCollider);
            _rightHitbox = Entity.AddComponent(new Hitbox(_rightCollider, 3));
            _components.Add(_rightHitbox);

            //left facing hitbox
            _leftCollider = Entity.AddComponent(new PolygonCollider(new Vector2[]
            {
                new Vector2(0, -4),
                new Vector2(0, 7),
                new Vector2(-35, 7),
                new Vector2(-40, 0),
                new Vector2(-35, -4)
            }));
            _leftCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _leftCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHitbox);
            Flags.SetFlagExclusive(ref _leftCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHurtbox);
            _leftCollider.SetLocalOffset(-_offset);
            _components.Add(_leftCollider);
            _leftHitbox = Entity.AddComponent(new Hitbox(this._leftCollider, 3));
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

            //play animation
            var attackAnimation = _chainBot.VelocityComponent.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
            _chainBot.Animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);

            //yield until animation is completed
            while (_chainBot.Animator.IsAnimationActive(attackAnimation) && _chainBot.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_chainBot.Animator.CurrentFrame))
                {
                    hitbox.Enable();
                }
                else hitbox.Disable();

                yield return null;
            }
        }
    }
}
