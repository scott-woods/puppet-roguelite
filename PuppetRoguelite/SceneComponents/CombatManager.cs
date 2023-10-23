﻿using Microsoft.Xna.Framework.Input;
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
        public Entity MapEntity;

        public CombatManager()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public void AddEnemy(Enemy enemy)
        {
            MapEntity = enemy.MapEntity;
            if (enemy.Entity.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnEnemyDied);
            }
            Enemies.Add(enemy);
        }

        void EndCombat()
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
