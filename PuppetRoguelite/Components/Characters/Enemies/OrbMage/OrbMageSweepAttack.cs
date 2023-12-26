using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Entities;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies.OrbMage
{
    public class OrbMageSweepAttack : EnemyAction<OrbMage>, IUpdatable
    {
        //constants
        const int _pickDirectionFrame = 4;
        const int _startFrame = 8;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _sweepAttackCoroutine;

        //entities
        Entity _vfxEntity;

        public OrbMageSweepAttack(OrbMage enemy) : base(enemy)
        {
        }

        public void Update()
        {
            _coroutineManager.Update();
        }

        protected override void StartAction()
        {
            _sweepAttackCoroutine = _coroutineManager.StartCoroutine(SweepAttackCoroutine());
        }

        IEnumerator SweepAttackCoroutine()
        {
            //enemy plays sweep animation
            _enemy.Animator.Play("SweepAttack", Nez.Sprites.SpriteAnimator.LoopMode.Once);
            _enemy.Animator.OnAnimationCompletedEvent += OnAnimationCompleted;

            //wait for pick direction frame
            while (_enemy.Animator.CurrentFrame < _pickDirectionFrame)
                yield return null;

            //get direction to player
            var dir = _enemy.GetTargetPosition() - Entity.Position;
            dir.Normalize();

            //wait for start frame
            while (_enemy.Animator.CurrentFrame < _startFrame)
                yield return null;

            //create vfx entity
            _vfxEntity = Entity.Scene.AddEntity(new PausableEntity("sweep-attack-vfx"));
            _vfxEntity.AddComponent(new OrbMageSweepAttackVfx(dir));
            _vfxEntity.SetPosition(Entity.Position);

            //wait for attack vfx to be finished
            while (!_vfxEntity.IsDestroyed)
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
            Log.Debug("Resetting OrbMageSweepAttack");

            //coroutines
            _sweepAttackCoroutine?.Stop();
            _sweepAttackCoroutine = null;

            //animator
            _enemy.Animator.OnAnimationCompletedEvent -= OnAnimationCompleted;

            //clear entity
            _vfxEntity?.Destroy();
        }
    }
}
