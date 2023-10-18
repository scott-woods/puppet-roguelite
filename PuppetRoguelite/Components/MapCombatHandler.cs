using Nez;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class MapCombatHandler : Component
    {
        public bool IsCombatStarted = false;
        public bool IsCleared = false;
        public List<Enemy> Enemies = new List<Enemy>();
        public int Turns = 0;

        public Entity TurnHandlerEntity;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //connect to TurnPhaseCompleted event
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            //connect to enemy spawn triggers
            var enemySpawnTriggers = Entity.Scene.FindComponentsOfType<EnemySpawnTrigger>().Where(t => t.MapEntity == Entity).ToList();
            foreach (var trigger in enemySpawnTriggers)
            {
                trigger.Emitter.AddObserver(TriggerEventTypes.Triggered, OnEnemySpawnTriggered);
            }
        }

        #region OBSERVERS

        void OnEnemySpawnTriggered()
        {
            IsCombatStarted = true;

            //destroy other enemy spawn triggers
            var triggers = Entity.Scene.FindComponentsOfType<EnemySpawnTrigger>().Where(t => t.MapEntity == Entity).ToList();
            foreach (var trigger in triggers)
            {
                trigger.Entity.Destroy();
            }

            //lock gates
            var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapEntity == Entity).ToList();
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            //spawn enemies
            var enemySpawns = Entity.Scene.FindComponentsOfType<EnemySpawnPoint>().Where(e => e.MapEntity == Entity).ToList();
            //foreach (var spawn in enemySpawns)
            //{
            //    _enemies.Add(spawn.SpawnEnemy());
            //}
            Enemies.Add(enemySpawns[0].SpawnEnemy());

            //emit
            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);
        }

        void OnEncounterStarted()
        {
            if (IsCombatStarted && !IsCleared)
            {
                Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
            }
        }

        void OnTurnPhaseTriggered()
        {
            if (IsCombatStarted && !IsCleared)
            {
                Turns += 1;

                //create turn handler
                TurnHandlerEntity = Entity.Scene.CreateEntity("turn-handler");
                TurnHandlerEntity.AddComponent(new TurnHandler());
            }
        }

        void OnTurnPhaseCompleted()
        {
            if (IsCombatStarted && !IsCleared)
            {
                //remove enemies that have been killed
                Enemies.RemoveAll(e => e.Entity == null);

                //if any enemies left, continue combat by entering dodge phase
                if (Enemies.Any())
                {
                    Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
                }
                else //no enemies left
                {
                    //unlock gates
                    var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapEntity == Entity).ToList();
                    foreach (var gate in gates)
                    {
                        gate.Unlock();
                    }

                    IsCleared = true;

                    Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterEnded);
                }
            }
        }

        #endregion
    }
}
