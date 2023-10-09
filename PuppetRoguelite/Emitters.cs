using Nez.Systems;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public static class Emitters
    {
        public static Emitter<PlayerActionEvents, PlayerAction> PlayerActionEmitter = new Emitter<PlayerActionEvents, PlayerAction>();
        public static Emitter<CombatEvents> CombatEventsEmitter = new Emitter<CombatEvents>();
        public static Emitter<ActionPointEvents, ActionPointComponent> ActionPointEmitter = new Emitter<ActionPointEvents, ActionPointComponent>();
    }

    public enum PlayerActionEvents
    {
        ActionFinishedPreparing,
        ActionExecuting,
        ActionFinishedExecuting,
        SimActionExecuting,
        SimActionFinishedExecuting,
        ActionPrepCanceled
    }
    public enum CombatEvents
    {
        EncounterStarted,
        EncounterEnded,
        TurnPhaseTriggered,
        TurnPhaseExecuting,
        TurnPhaseCompleted,
        DodgePhaseStarted
    }
    public enum ActionPointEvents
    {
        ActionPointsChanged,
        MaxActionPointsChanged,
        ActionPointsTimerStarted,
        ActionPointsTimerUpdated
    }
}
