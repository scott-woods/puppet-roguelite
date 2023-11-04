using Nez;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class ComboComponent : SceneComponent
    {
        public int ComboCounter = 0;

        List<HealthComponent> _connectedHealthComponents = new List<HealthComponent>();
        List<DollahDropper> _connectedDollahDroppers = new List<DollahDropper>();

        public ComboComponent()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);

            if (_connectedHealthComponents.Count > 0)
            {
                foreach (var component in _connectedHealthComponents)
                {
                    component.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnEnemyHealthDepleted);
                }
            }
        }

        void OnTurnPhaseExecuting()
        {
            ComboCounter = 0;

            var enemies = Scene.FindComponentsOfType<Enemy>();
            foreach (var enemy in enemies)
            {
                if (enemy.Entity.TryGetComponent<HealthComponent>(out var hc))
                {
                    hc.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnEnemyHealthDepleted);
                    _connectedHealthComponents.Add(hc);
                }
            }
        }

        void OnTurnPhaseCompleted()
        {
            foreach (var hc in _connectedHealthComponents)
            {
                hc.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnEnemyHealthDepleted);
            }

            _connectedHealthComponents.Clear();
            _connectedDollahDroppers.Clear();
        }

        void OnEnemyHealthDepleted(HealthComponent hc)
        {
            ComboCounter += 1;
            if (hc.Entity.TryGetComponent<DollahDropper>(out var dollahDropper))
            {
                _connectedDollahDroppers.Add(dollahDropper);
            }

            foreach (var dropper in _connectedDollahDroppers)
            {
                dropper.SetMultiplier(ComboCounter);
            }
        }
    }
}
