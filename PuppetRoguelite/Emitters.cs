using Nez.Systems;
using PuppetRoguelite.Components.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public static class Emitters
    {
        /// <summary>
        /// Events relating to IPlayerAction
        /// </summary>
        public static Emitter<PlayerActionEvents, IPlayerAction> PlayerActionEmitter = new Emitter<PlayerActionEvents, IPlayerAction>();
        public static Emitter<CombatEvents> CombatEventsEmitter = new Emitter<CombatEvents>();
    }

    public enum PlayerActionEvents
    {
        ActionFinishedPreparing,
        ActionExecuting,
        ActionFinishedExecuting
    }
    public enum CombatEvents
    {
        TurnPhaseTriggered,
        TurnPhaseExecuting,
        TurnPhaseCompleted
    }
}
