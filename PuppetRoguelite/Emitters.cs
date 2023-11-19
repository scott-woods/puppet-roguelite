using Nez.Systems;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
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
        public static Emitter<CombatEvents> CombatEventsEmitter = new Emitter<CombatEvents>();
        public static Emitter<ActionPointEvents, ActionPointComponent> ActionPointEmitter = new Emitter<ActionPointEvents, ActionPointComponent>();
        public static Emitter<InteractableEvents> InteractableEmitter = new Emitter<InteractableEvents>();
        public static Emitter<CutsceneEvents> CutsceneEmitter = new Emitter<CutsceneEvents>();
        public static Emitter<PlayerEvents> PlayerEventsEmitter = new Emitter<PlayerEvents>();
        public static Emitter<GameEvents> GameEventsEmitter = new Emitter<GameEvents>();

        public static void ResetEmitters()
        {
            CombatEventsEmitter = new Emitter<CombatEvents>();
            ActionPointEmitter = new Emitter<ActionPointEvents, ActionPointComponent>();
            InteractableEmitter = new Emitter<InteractableEvents>();
            CutsceneEmitter = new Emitter<CutsceneEvents>();
            PlayerEventsEmitter = new Emitter<PlayerEvents>();
            GameEventsEmitter = new Emitter<GameEvents>();
        }
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
        ActionPointsTimerUpdated,
        ActionPointsProgressChanged
    }
    public enum InteractableEvents
    {
        Interacted,
        InteractionFinished
    }
    public enum CutsceneEvents
    {
        CutsceneStarted,
        CutsceneEnded,
    }
    public enum PlayerEvents
    {
        PlayerDied
    }

    public enum GameEvents
    {
        RespawningAtHub,
        StartingDungeonRun
    }
}
