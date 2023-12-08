using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies.OrbMage
{
    public class OrbMageAttack : EnemyAction<OrbMage>, IUpdatable
    {
        //constants
        const int _showVfxFrame = 6;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _orbMageAttackExecutionCoroutine;

        //entities
        Entity _attackVfxEntity;

        public OrbMageAttack(OrbMage enemy) : base(enemy)
        {
        }

        public void Update()
        {
            _coroutineManager.Update();
        }

        protected override void StartAction()
        {
            _orbMageAttackExecutionCoroutine = _coroutineManager.StartCoroutine(OrbMageAttackExecutionCoroutine());
        }

        IEnumerator OrbMageAttackExecutionCoroutine()
        {
            //play attack animation
            _enemy.Animator.Play("Attack", Nez.Sprites.SpriteAnimator.LoopMode.Once);
            _enemy.Animator.OnAnimationCompletedEvent += OnAnimationCompleted;

            //wait until show vfx frame
            while (_enemy.Animator.CurrentFrame < _showVfxFrame)
                yield return null;

            //create vfx entity
            _attackVfxEntity = Entity.Scene.AddEntity(new PausableEntity("orb-mage-attack-vfx"));
            _attackVfxEntity.AddComponent(new OrbMageAttackVfx());
            var targetPos = PlayerController.Instance.OriginComponent.Origin + new Vector2(0, -17);
            _attackVfxEntity.SetPosition(targetPos);

            //wait for attack vfx to be finished
            while (!_attackVfxEntity.IsDestroyed)
                yield return null;

            HandleActionFinished();
            Reset();
        }

        void OnAnimationCompleted(string animationName)
        {
            _enemy.Animator.OnAnimationCompletedEvent -= OnAnimationCompleted;

            _enemy.Animator.SetSprite(_enemy.Animator.CurrentAnimation.Sprites.Last());
        }

        public override void Abort()
        {
            base.Abort();

            Reset();
        }

        void Reset()
        {
            //coroutines
            _orbMageAttackExecutionCoroutine?.Stop();
            _orbMageAttackExecutionCoroutine = null;

            //animator
            _enemy.Animator.OnAnimationCompletedEvent -= OnAnimationCompleted;
        }
    }
}
