using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Spitter
{
    public class SpitAttack : EnemyAction<Spitter>, IUpdatable
    {
        bool _hasLaunchedProjectile = false;

        protected override IEnumerator StartAction()
        {
            _hasLaunchedProjectile = false;
            _enemy.Animator.Play("Attack", Nez.Sprites.SpriteAnimator.LoopMode.Once);
            _enemy.Animator.OnAnimationCompletedEvent += OnAttackAnimationCompleted;
            yield break;
        }

        public void Update()
        {
            if (_state == EnemyActionState.Running)
            {
                if (!_hasLaunchedProjectile)
                {
                    if (_enemy.Animator.CurrentAnimationName == "Attack" && _enemy.Animator.CurrentFrame == 3)
                    {
                        _hasLaunchedProjectile = true;
                        var dir = PlayerController.Instance.Entity.Position - _enemy.Entity.Position;
                        dir.Normalize();
                        var ent = new PausableEntity("spit-projectile");
                        Entity.Scene.AddEntity(ent);
                        ent.SetPosition(Entity.Position);
                        ent.AddComponent(new SpitAttackProjectile(dir));
                    }
                }
            }
        }

        void OnAttackAnimationCompleted(string animationName)
        {
            _enemy.Animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;
            _state = EnemyActionState.Succeeded;
        }

        public override void Abort()
        {
            base.Abort();

            _enemy.Animator.OnAnimationCompletedEvent -= OnAttackAnimationCompleted;
        }
    }
}
