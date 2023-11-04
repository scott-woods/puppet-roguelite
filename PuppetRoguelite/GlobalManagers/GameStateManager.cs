using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.Tools;
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
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);

            Emitters.PlayerEventsEmitter.AddObserver(PlayerEvents.PlayerDied, OnPlayerDied);
        }

        public void ReturnToHubAfterSuccess()
        {
            Game1.SceneManager.ChangeScene(typeof(Bedroom), "0", Color.White, 4f, 4f, 1f);
            Game1.SceneManager.Emitter.AddObserver(SceneEvents.ScreenObscured, OnScreenObscured);
        }

        public void ReturnToHubAfterDeath()
        {
            Game1.SceneManager.ChangeScene(typeof(Bedroom), "0");
            Game1.SceneManager.Emitter.AddObserver(SceneEvents.ScreenObscured, OnScreenObscured);
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

        void OnScreenObscured()
        {
            Game1.SceneManager.Emitter.RemoveObserver(SceneEvents.ScreenObscured, OnScreenObscured);
            PlayerController.Instance.HealthComponent.Health = PlayerController.Instance.HealthComponent.MaxHealth;
        }
    }

    public enum GameState
    {
        Exploration,
        Combat
    }
}
