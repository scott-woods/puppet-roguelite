using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class DungeonRoom : Component
    {
        List<Type> _enemyTypes = new List<Type>()
        {
            typeof(ChainBot)
        };

        RoomNode _node;
        bool _isCleared = false;
        bool _encounterStarted = false;
        string _mapString;

        TmxMap _map;
        TiledMapRenderer _mapRenderer;
        TiledMapComponent _mapComponent;

        List<Entity> _triggerEntities = new List<Entity>();
        List<Gate> _gates = new List<Gate>();
        List<EnemySpawnPoint> _enemySpawnPoints = new List<EnemySpawnPoint>();
        List<Enemy> _enemies = new List<Enemy>();
        List<ExitArea> _exitAreas = new List<ExitArea>();

        public GridGraphManager GridGraphManager { get; set; }

        public DungeonRoom(RoomNode node, string mapString)
        {
            _node = node;
            _mapString = mapString;
        }

        public override void Initialize()
        {
            base.Initialize();

            //map renderer
            _map = Entity.Scene.Content.LoadTiledMap(_mapString);
            _mapRenderer = Entity.AddComponent(new TiledMapRenderer(_map, "collision"));
            _mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
            _mapRenderer.RenderLayer = 10;
            Flags.SetFlagExclusive(ref _mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);
            _mapComponent = Entity.AddComponent(new TiledMapComponent(_mapRenderer));

            //connect to TurnPhaseCompleted event
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //connect to enemy spawn triggers
            var enemySpawnTriggers = Entity.Scene.FindComponentsOfType<EnemySpawnTrigger>().Where(t => t.MapId == _mapComponent.Id).ToList();
            foreach(var trigger in enemySpawnTriggers)
            {
                trigger.Emitter.AddObserver(TriggerEventTypes.Triggered, OnEnemySpawnTriggered);
            }
        }

        void OnEnemySpawnTriggered()
        {
            if (!_isCleared)
            {
                _encounterStarted = true;

                //lock gates
                var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapId == _mapComponent.Id).ToList();
                foreach (var gate in gates)
                {
                    gate.Lock();
                }

                //spawn enemies
                var enemySpawns = Entity.Scene.FindComponentsOfType<EnemySpawnPoint>().Where(e => e.MapId == _mapComponent.Id).ToList();
                //foreach (var spawn in enemySpawns)
                //{
                //    _enemies.Add(spawn.SpawnEnemy());
                //}
                _enemies.Add(enemySpawns[0].SpawnEnemy());

                //emit
                Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);

                //destroy entrance triggers
                var enemySpawnTriggers = Entity.Scene.FindComponentsOfType<Trigger>().Where(t => t.MapId == _mapComponent.Id).ToList();
                foreach(var trigger in enemySpawnTriggers)
                {
                    trigger.Entity.Destroy();
                }
            }
        }

        void OnTurnPhaseCompleted()
        {
            if (_encounterStarted && !_isCleared)
            {
                //remove enemies that have been killed
                _enemies.RemoveAll(e => e.Entity == null);
                
                //if any enemies left, continue combat by entering dodge phase
                if (_enemies.Any())
                {
                    Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
                }
                else //no enemies left
                {
                    //set room as cleared
                    _isCleared = true;

                    //unlock gates
                    var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapId == _mapComponent.Id).ToList();
                    foreach (var gate in gates)
                    {
                        gate.Unlock();
                    }

                    Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterEnded);
                }
            }
        }

        //public override void OnRemovedFromEntity()
        //{
        //    base.OnRemovedFromEntity();

        //    foreach (var ent in _triggerEntities)
        //    {
        //        ent.Destroy();
        //    }
        //}
    }
}
