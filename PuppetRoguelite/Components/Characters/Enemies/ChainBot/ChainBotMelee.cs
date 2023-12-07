﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.PhysicsShapes;
using Nez.Sprites;
using Nez.Systems;
using Nez.UI;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite.Components.Characters.Enemies.ChainBot
{
    public class ChainBotMelee : EnemyAction<ChainBot>, IUpdatable
    {
        const int _damage = 3;
        Vector2 _offset = new Vector2(24, 0);
        List<int> _hitboxActiveFrames = new List<int> { 0, 4 };

        //components
        PolygonHitbox _leftHitbox, _rightHitbox, _activeHitbox;
        SpriteAnimator _animator;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _executionCoroutine;
        ICoroutine _transitionToCharge;
        ICoroutine _attackPlayer;

        //misc
        int _soundCounter = 0;
        bool _isAttacking = false;
        bool _transitioningToCharge = false;

        public ChainBotMelee(ChainBot chainBot) : base(chainBot) { }

        public override void Initialize()
        {
            base.Initialize();

            //animator
            _animator = Entity.GetComponent<SpriteAnimator>();

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
        }

        public void Update()
        {
            _coroutineManager.Update();

            if (_isAttacking)
            {
                if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                {
                    if (_animator.CurrentFrame == _hitboxActiveFrames[0] && _soundCounter == 0)
                    {
                        _soundCounter += 1;
                        Game1.AudioManager.PlaySound(Content.Audio.Sounds._81_Whip_woosh_1);
                    }
                    else if (_animator.CurrentFrame == _hitboxActiveFrames[1] && _soundCounter == 1)
                    {
                        _soundCounter += 1;
                        Game1.AudioManager.PlaySound(Content.Audio.Sounds._81_Whip_woosh_1);
                    }

                    _activeHitbox?.SetEnabled(true);
                }
                else _activeHitbox?.SetEnabled(false);
            }
        }

        protected override void StartAction()
        {
            _executionCoroutine = _coroutineManager.StartCoroutine(ChainBotMeleeExecutionCoroutine());
        }

        IEnumerator ChainBotMeleeExecutionCoroutine()
        {
            var id = _enemy.Id;
            //Debug.Log($"{id}: transitioning to charge");
            _transitionToCharge = _coroutineManager.StartCoroutine(TransitionToCharge());
            yield return _transitionToCharge;
            _transitionToCharge = null;
            //Debug.Log($"{id}: finished transition to charge");
            //Debug.Log($"{id}: starting charge");
            ChargeAttack();
            yield return Coroutine.WaitForSeconds(.2f);
            //Debug.Log($"{id}: finished charge");
            //Debug.Log($"{id}: starting attack player");
            _attackPlayer = _coroutineManager.StartCoroutine(AttackPlayer());
            yield return _attackPlayer;
            _attackPlayer = null;
            //Debug.Log($"{id}: finished attack player");

            HandleActionFinished();
            Reset();
        }

        IEnumerator TransitionToCharge()
        {
            _transitioningToCharge = true;

            var transitionAnimation = _enemy.VelocityComponent.Direction.X >= 0 ? "TransitionRight" : "TransitionLeft";
            _animator.Play(transitionAnimation, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnTransitionToChargeCompleted;

            while (_transitioningToCharge)
            {
                yield return null;
            }
        }

        void OnTransitionToChargeCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnTransitionToChargeCompleted;
            _transitioningToCharge = false;
        }

        void ChargeAttack()
        {
            var chargeAnimation = _enemy.VelocityComponent.Direction.X >= 0 ? "ChargeRight" : "ChargeLeft";
            _animator.Play(chargeAnimation);
        }

        IEnumerator AttackPlayer()
        {
            _isAttacking = true;

            //select hitbox
            _activeHitbox = _enemy.VelocityComponent.Direction.X >= 0 ? _rightHitbox : _leftHitbox;
            _activeHitbox.Direction = _enemy.VelocityComponent.Direction;

            //play animation
            var attackAnimation = _enemy.VelocityComponent.Direction.X >= 0 ? "AttackRight" : "AttackLeft";
            _animator.Play(attackAnimation, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAttackAnimationCompleted;

            //yield until animation is completed
            while (_isAttacking)
            {
                yield return null;
            }
        }

        void OnAttackAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;
            _isAttacking = false;
        }

        void Reset()
        {
            //handle coroutines
            _executionCoroutine?.Stop();
            _executionCoroutine = null;
            _transitionToCharge?.Stop();
            _transitionToCharge = null;
            _attackPlayer?.Stop();
            _attackPlayer = null;

            //disable hitboxes
            _leftHitbox.SetEnabled(false);
            _rightHitbox.SetEnabled(false);

            //reset fields
            _transitioningToCharge = false;
            _isAttacking = false;
            _soundCounter = 0;
            _activeHitbox = null;

            //remove animation handlers
            _animator.OnAnimationCompletedEvent -= OnTransitionToChargeCompleted;
            _animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;
        }

        public override void Abort()
        {
            base.Abort();

            Debug.Log($"{_enemy.Id}: aborting");

            Reset();
        }
    }
}
