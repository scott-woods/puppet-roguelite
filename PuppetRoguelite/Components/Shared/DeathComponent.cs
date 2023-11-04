using Nez;
using Nez.Sprites;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    /// <summary>
    /// component that plays a sound and animation after health depleted.
    /// </summary>
    public class DeathComponent : Component
    {
        public event Action<Entity> OnDeathStarted;
        public event Action<Entity> OnDeathFinished;

        string _sound;
        SpriteAnimator _animator;
        string _deathAnimName;
        string _hitAnimName;

        public DeathComponent(string sound, SpriteAnimator animator, string deathAnimName, string hitAnimName)
        {
            _sound = sound;
            _animator = animator;
            _deathAnimName = deathAnimName;
            _hitAnimName = hitAnimName;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            }
        }

        void Die()
        {
            OnDeathStarted?.Invoke(Entity);

            Game1.AudioManager.PlaySound(_sound);
            
            _animator.Play(_deathAnimName, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnDeathAnimationCompleted;
        }

        void OnHealthDepleted(HealthComponent hc)
        {
            var combatManager = Entity.Scene.GetSceneComponent<CombatManager>();
            if (combatManager != null)
            {
                if (combatManager.CombatState == CombatState.Turn)
                {
                    _animator.Play(_hitAnimName);
                    Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
                    return;
                }
            }

            Die();
        }

        void OnDeathAnimationCompleted(string animationName)
        {
            _animator.OnAnimationCompletedEvent -= OnDeathAnimationCompleted;
            OnDeathFinished?.Invoke(Entity);
            Entity.Destroy();
        }

        void OnTurnPhaseCompleted()
        {
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Die();
        }
    }
}
