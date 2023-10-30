using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class GameStateManager : GlobalManager
    {
        public GameState GameState = GameState.Exploration;

        public GameStateManager()
        {
            Game1.SceneManager.Emitter.AddObserver(SceneEvents.TransitionEnded, OnSceneTransitionEnded);
            AddObservers();
        }

        void AddObservers()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);

            Emitters.PlayerEventsEmitter.AddObserver(PlayerEvents.PlayerDied, OnPlayerDied);
        }

        IEnumerator HandleGameOver()
        {
            //stop music
            Game1.AudioManager.StopMusic();

            //change game state
            GameState = GameState.Exploration;

            //wait for player entity to be destroyed
            while (PlayerController.Instance.Entity != null)
            {
                yield return null;
            }

            Game1.SceneManager.ChangeScene(typeof(Bedroom), "0");
        }

        void OnSceneTransitionEnded()
        {
            AddObservers();
        }

        void OnEncounterStarted()
        {
            GameState = GameState.Combat;
        }

        void OnEncounterEnded()
        {
            GameState = GameState.Exploration;
        }

        void OnPlayerDied()
        {
            Game1.StartCoroutine(HandleGameOver());
        }
    }

    public enum GameState
    {
        Exploration,
        Combat
    }
}
