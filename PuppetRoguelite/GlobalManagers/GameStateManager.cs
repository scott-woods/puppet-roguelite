using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.HUDs;
using PuppetRoguelite.UI.Menus;
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
        GameState _previousGameState;

        bool _pauseDisabled = false;

        public GameStateManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);

            Emitters.PlayerEventsEmitter.AddObserver(PlayerEvents.PlayerDied, OnPlayerDied);

            Game1.SceneManager.Emitter.AddObserver(SceneEvents.TransitionStarted, OnSceneTransitionStarted);
            Game1.SceneManager.Emitter.AddObserver(SceneEvents.TransitionEnded, OnSceneTransitionEnded);
        }

        public override void Update()
        {
            base.Update();

            if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape) && !_pauseDisabled)
            {
                if (GameState == GameState.Paused)
                {
                    Unpause();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Pause()
        {
            //sound
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._094_Pause_06);

            //disable other ui
            var ui = Game1.Scene.FindComponentsOfType<UICanvas>();
            foreach (var canvas in ui)
            {
                canvas.SetEnabled(false);
            }

            //create pause menu ui
            Game1.Scene.CreateEntity("pause-menu")
                .AddComponent(new PauseWindow());

            Time.TimeScale = 0;

            _previousGameState = GameState;
            GameState = GameState.Paused;
            Emitters.GameEventsEmitter.Emit(GameEvents.Paused);
        }

        public void Unpause()
        {
            //sound
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._100_Unpause_06);

            Time.TimeScale = 1;

            var ui = Game1.Scene.FindComponentsOfType<UICanvas>();
            foreach (var canvas in ui)
            {
                canvas.SetEnabled(true);
            }

            GameState = _previousGameState;
            _previousGameState = GameState.Paused;
            Emitters.GameEventsEmitter.Emit(GameEvents.Unpaused);
        }

        public void ReturnToHubAfterSuccess()
        {
            Game1.SceneManager.ChangeScene(typeof(NewHub), "0", Color.White, 4f, 4f, 1f);
            Game1.SceneManager.Emitter.AddObserver(SceneEvents.ScreenObscured, OnScreenObscured);
        }

        public void ReturnToHubAfterDeath()
        {
            Game1.SceneManager.ChangeScene(typeof(NewHub), "0");
            Game1.SceneManager.Emitter.AddObserver(SceneEvents.ScreenObscured, OnScreenObscured);
        }

        IEnumerator HandleGameOver()
        {
            //stop music
            Game1.AudioManager.StopMusic();

            //change game state
            GameState = GameState.Exploration;

            yield break;
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
            GameState = GameState.Exploration;
            PlayerController.Instance.HealthComponent.Health = PlayerController.Instance.HealthComponent.MaxHealth;
        }

        void OnSceneTransitionStarted()
        {
            _pauseDisabled = true;
        }

        void OnSceneTransitionEnded()
        {
            _pauseDisabled = false;
        }
    }

    public enum GameState
    {
        Exploration,
        Combat,
        Paused,
        Tutorial,
        AttackTutorial
    }
}
