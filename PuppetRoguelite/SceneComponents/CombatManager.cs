using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
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

        public CombatManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public void OnTurnPhaseTriggered()
        {
            //create turn handler
            _turnHandlerEntity = Scene.CreateEntity("turn-handler");
            _turnHandlerEntity.AddComponent(new TurnHandler(this));
        }

        public void OnTurnPhaseCompleted()
        {
            _turnHandlerEntity.Destroy();
            Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
        }
    }
}
