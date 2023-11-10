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
        public List<DeathComponent> EnemyDeathComponents = new List<DeathComponent>();
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

            foreach (var dc in EnemyDeathComponents)
            {
                dc.OnDeathStarted -= OnEnemyDied;
            }

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseExecuting, OnTurnPhaseExecuting);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);

            ComboManager.Reset();
        }

        public void AddEnemy(Enemy enemy)
        {
            MapEntity = enemy.MapEntity;
            if (enemy.Entity.TryGetComponent<DeathComponent>(out var dc))
            {
                EnemyDeathComponents.Add(dc);
                dc.OnDeathStarted += OnEnemyDied;
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
                Debug.Log("combat finishing after turn phase completed and no enemies.");
                EndCombat();
            }
            else
            {
                CombatState = CombatState.Dodge;
                Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
            }
        }

        void OnEnemyDied(Entity enemyEntity)
        {
            var enemy = enemyEntity.GetComponent<Enemy>();
            Enemies.Remove(enemy);
            if (!Enemies.Any())
            {
                Debug.Log("combat finishing because all enemies died");
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
