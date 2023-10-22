using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class GameStateManager : SceneComponent
    {
        public GameState GameState = GameState.Exploration;

        public GameStateManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
        }

        void OnEncounterStarted()
        {
            GameState = GameState.Combat;
        }

        void OnEncounterEnded()
        {
            GameState = GameState.Exploration;
        }
    }

    public enum GameState
    {
        Exploration,
        Combat
    }
}
