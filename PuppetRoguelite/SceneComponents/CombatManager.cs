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
        public int Turns = 0;
        public Entity TurnHandlerEntity;
        public List<Enemy> Enemies = new List<Enemy>();
        public List<HealthComponent> EnemyHealthComponents = new List<HealthComponent>();
        public Entity MapEntity;
        public ComboComponent ComboComponent;

        public CombatManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
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
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
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

            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterEnded);
        }

        bool CheckForCompletion()
        {
            return !Enemies.Any();
        }

        #region OBSERVERS

        void OnEnemyDied(HealthComponent healthComponent)
        {
            healthComponent.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnEnemyDied);
            var enemy = healthComponent.Entity.GetComponent<Enemy>();
            Enemies.Remove(enemy);
            if (CheckForCompletion())
            {
                EndCombat();
            }
        }

        void OnEncounterStarted()
        {
            Turns = 0;
            CombatState = CombatState.Dodge;
            Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
        }

        void OnTurnPhaseTriggered()
        {
            Turns += 1;

            CombatState = CombatState.Turn;

            //create turn handler
            TurnHandlerEntity = Scene.CreateEntity("turn-handler");
            TurnHandlerEntity.AddComponent(new TurnHandler());
        }

        void OnTurnPhaseCompleted()
        {
            if (CheckForCompletion())
            {
                EndCombat();
            }
            else
            {
                CombatState = CombatState.Dodge;
                Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
            }
        }

        void OnEncounterEnded()
        {
            CombatState = CombatState.None;
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
