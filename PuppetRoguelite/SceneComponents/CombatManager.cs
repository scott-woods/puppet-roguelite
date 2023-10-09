using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Entities;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    /// <summary>
    /// Handles the flow of combat in a scene
    /// </summary>
    public class CombatManager : SceneComponent
    {
        Entity _turnHandlerEntity;
        TurnHandler _turnHandler;
        int _turns = 0;

        public CombatManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);

            //_turnHandler = new TurnHandler(this);
        }

        void OnEncounterStarted()
        {
            _turns = 0;
            Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
        }

        public void OnTurnPhaseTriggered()
        {
            _turns += 1;

            //create turn handler
            _turnHandlerEntity = Scene.CreateEntity("turn-handler");
            _turnHandlerEntity.AddComponent(new TurnHandler(this));
        }

        /// <summary>
        /// called after all actions finished executing
        /// </summary>
        public void OnTurnPhaseCompleted()
        {
            _turnHandlerEntity.Destroy();
        }
    }
}
