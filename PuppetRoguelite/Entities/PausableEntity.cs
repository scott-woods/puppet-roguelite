using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Entities
{
    public class PausableEntity : Entity
    {
        bool _paused = false;

        public PausableEntity(string name) : base(name)
        {

        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
        }

        void OnTurnPhaseTriggered()
        {
            _paused = true;
        }

        void OnTurnPhaseExecuting()
        {
            _paused = false;
        }

        public override void Update()
        {
            if (!_paused)
            {
                base.Update();
            }
        }
    }
}
