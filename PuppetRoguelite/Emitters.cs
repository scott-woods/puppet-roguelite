﻿using Nez.Systems;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Actions;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public static class Emitters
    {
        public static Emitter<PlayerActionEvents, IPlayerAction> PlayerActionEmitter = new Emitter<PlayerActionEvents, IPlayerAction>();
        public static Emitter<CombatEvents> CombatEventsEmitter = new Emitter<CombatEvents>();
        public static Emitter<ActionPointEvents, ActionPointComponent> ActionPointEmitter = new Emitter<ActionPointEvents, ActionPointComponent>();
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
