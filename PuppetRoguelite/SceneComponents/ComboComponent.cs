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

        List<DeathComponent> _connectedDeathComponents = new List<DeathComponent>();

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

            if (_connectedDeathComponents.Count > 0)
            {
                foreach (var component in _connectedDeathComponents)
                {
                    component.OnDeathStarted -= OnEnemyDeathStarted;
                }
            }
        }

        void OnTurnPhaseExecuting()
        {
            ComboCounter = 0;

            var enemies = Scene.FindComponentsOfType<Enemy>();
            foreach (var enemy in enemies)
            {
                if (enemy.Entity.TryGetComponent<DeathComponent>(out var dc))
                {
                    dc.OnDeathStarted += OnEnemyDeathStarted;
                    _connectedDeathComponents.Add(dc);
                }
            }
        }

        void OnTurnPhaseCompleted()
        {
            foreach (var dc in _connectedDeathComponents)
            {
                dc.OnDeathStarted -= OnEnemyDeathStarted;
            }

            _connectedDeathComponents.Clear();
        }

        void OnEnemyDeathStarted(Entity entity)
        {
            ComboCounter += 1;
        }
    }
}
