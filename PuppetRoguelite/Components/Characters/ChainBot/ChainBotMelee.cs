using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.PhysicsShapes;
using Nez.Sprites;
using Nez.Systems;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite.Components.Characters.ChainBot
{
    public class ChainBotMelee : EnemyAction<ChainBot>
    {
        const int _damage = 3;
        Vector2 _offset = new Vector2(24, 0);

        //components
        PolygonHitbox _leftHitbox, _rightHitbox;

        public ChainBotMelee(ChainBot chainBot) : base(chainBot) { }

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
            _rightHitbox.SetEnabled(false);
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
            _leftHitbox.SetEnabled(false);
            _components.Add(_leftHitbox);
        }

        protected override IEnumerator StartAction()
        {
            yield return TransitionToCharge();
            ChargeAttack();
            yield return Coroutine.WaitForSeconds(.25f);
            yield return AttackPlayer();

            _state = EnemyActionState.Succeeded;
        }

        IEnumerator TransitionToCharge()
        {
            var transitionAnimation = _enemy.VelocityComponent.Direction.X >= 0 ? "TransitionRight" : "TransitionLeft";
            _enemy.Animator.Play(transitionAnimation, SpriteAnimator.LoopMode.Once);
            while (_enemy.Animator.IsAnimationActive(transitionAnimation) && _enemy.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                yield return null;
            }
        }

        void ChargeAttack()
        {
            var chargeAnimation = _enemy.VelocityComponent.Direction.X >= 0 ? "ChargeRight" : "ChargeLeft";
            _enemy.Animator.Play(chargeAnimation);
        }

        IEnumerator AttackPlayer()
        {
            //select hitbox
            var hitbox = _enemy.VelocityComponent.Direction.X >= 0 ? _rightHitbox : _leftHitbox;
            hitbox.Direction = _enemy.VelocityComponent.Direction;

            //play animation
            var attackAnimation = _enemy.VelocityComponent.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
            _enemy.Animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);

            var soundCounter = 0;

            //yield until animation is completed
            while (_enemy.Animator.IsAnimationActive(attackAnimation) && _enemy.Animator.AnimationState != SpriteAnimator.State.Completed)
            {
                if (new[] { 0, 4 }.Contains(_enemy.Animator.CurrentFrame))
                {
                    if (_enemy.Animator.CurrentFrame == 0 && soundCounter == 0)
                    {
                        soundCounter += 1;
                        Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._81_Whip_woosh_1, .5f);
                    }
                    else if (_enemy.Animator.CurrentFrame == 4 && soundCounter == 1)
                    {
                        soundCounter += 1;
                        Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._81_Whip_woosh_1, .5f);
                    }
                    
                    hitbox.SetEnabled(true);
                }
                else hitbox.SetEnabled(false);

                yield return null;
            }
        }

        public override void Abort()
        {
            base.Abort();

            _leftHitbox.SetEnabled(false);
            _rightHitbox.SetEnabled(false);
        }
    }
}
