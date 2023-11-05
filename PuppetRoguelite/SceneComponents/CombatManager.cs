using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.UI;
using System;
using System.Collections;
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
        public CombatState CombatState = CombatState.None;
        public List<Enemy> Enemies = new List<Enemy>();
        public List<HealthComponent> EnemyHealthComponents = new List<HealthComponent>();
        public Entity MapEntity;
        public ComboManager ComboManager = new ComboManager();
        public TurnHandler TurnHandler = new TurnHandler();

        public CombatManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();

            foreach (var hc in EnemyHealthComponents)
            {
                hc.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnEnemyDied);
            }

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);

            ComboManager.Reset();
        }

        public void AddEnemy(Enemy enemy)
        {
            MapEntity = enemy.MapEntity;
            if (enemy.Entity.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                EnemyHealthComponents.Add(healthComponent);
                healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnEnemyDied);
            }
            Enemies.Add(enemy);
        }

        public void EndCombat()
        {
            //unlock gates
            var gates = Scene.FindComponentsOfType<Gate>().Where(g => g.MapEntity == MapEntity).ToList();
            foreach (var gate in gates)
            {
                gate.Unlock();
            }

            CombatState = CombatState.None;

            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterEnded);
        }

        #region OBSERVERS

        void OnEncounterStarted()
        {
            CombatState = CombatState.Dodge;
            Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
        }

        void OnTurnPhaseTriggered()
        {
            CombatState = CombatState.Turn;
            TurnHandler.BeginTurn();
        }

        void OnTurnPhaseExecuting()
        {
            ComboManager.StartCounting(Enemies);
        }

        void OnTurnPhaseCompleted()
        {
            ComboManager.Reset();

            if (!Enemies.Any())
            {
                EndCombat();
            }
            else
            {
                CombatState = CombatState.Dodge;
                Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
            }
        }

        void OnEnemyDied(HealthComponent healthComponent)
        {
            healthComponent.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnEnemyDied);
            var enemy = healthComponent.Entity.GetComponent<Enemy>();
            Enemies.Remove(enemy);
            if (!Enemies.Any())
            {
                EndCombat();
            }
        }

        #endregion
    }

    public enum CombatState
    {
        None,
        Dodge,
        Turn
    }
}
