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
        bool _pausedForTurn = false;
        bool _gamePaused = false;

        public PausableEntity(string name) : base(name)
        {

        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.GameEventsEmitter.AddObserver(GameEvents.Paused, OnPaused);
            Emitters.GameEventsEmitter.AddObserver(GameEvents.Unpaused, OnUnpaused);
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.GameEventsEmitter.RemoveObserver(GameEvents.Paused, OnPaused);
            Emitters.GameEventsEmitter.RemoveObserver(GameEvents.Unpaused, OnUnpaused);
        }

        void OnTurnPhaseTriggered()
        {
            _pausedForTurn = true;
        }

        void OnTurnPhaseExecuting()
        {
            _pausedForTurn = false;
        }

        public override void Update()
        {
            if (!_pausedForTurn && !_gamePaused)
            {
                base.Update();
            }
        }

        void OnPaused()
        {
            _gamePaused = true;
        }

        void OnUnpaused()
        {
            _gamePaused = false;
        }
    }
}
