using Nez;
using Nez.Sprites;
using PuppetRoguelite.Components.Characters.Enemies.ChainBot;
using PuppetRoguelite.SceneComponents.CombatManager;
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
        bool _shouldDestroyEntity;

        Status _status = new Status(Status.StatusType.Death, (int)StatusPriority.Death);

        public DeathComponent(string sound, SpriteAnimator animator, string deathAnimName, string hitAnimName, bool shouldDestroyEntity = true)
        {
            _sound = sound;
            _animator = animator;
            _deathAnimName = deathAnimName;
            _hitAnimName = hitAnimName;
            _shouldDestroyEntity = shouldDestroyEntity;
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

            var chainBot = Entity.GetComponent<ChainBot>();
            if (chainBot != null )
            {
                Debug.Log($"{chainBot.Id}: starting death animation");
            }
            _animator.Play(_deathAnimName, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnDeathAnimationCompleted;
        }

        void OnHealthDepleted(HealthComponent hc)
        {
            //push status so nothing changes
            if (Entity.TryGetComponent<StatusComponent>(out var sc))
            {
                sc.PushStatus(_status);
            }

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
            _animator.SetEnabled(false);
            OnDeathFinished?.Invoke(Entity);

            if (_shouldDestroyEntity) Entity.Destroy();
        }

        void OnTurnPhaseCompleted()
        {
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Die();
        }
    }
}
