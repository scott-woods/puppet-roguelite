using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.SceneComponents;
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

        List<Entity> _triggerEntities = new List<Entity>();
        List<Gate> _gates = new List<Gate>();
        List<EnemySpawnPoint> _enemySpawnPoints = new List<EnemySpawnPoint>();
        List<Enemy> _enemies = new List<Enemy>();

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
            _mapRenderer.SetLayersToRender(new[] { "floor", "details", "gates" });
            _mapRenderer.RenderLayer = 10;

            //pathfinding
            var graph = new AstarGridGraph(_mapRenderer.CollisionLayer);
            GridGraphManager = Entity.AddComponent(new GridGraphManager(graph, _map));

            //gates
            var gateObjGroup = _map.GetObjectGroup("gates");
            if (gateObjGroup != null)
            {
                gateObjGroup.Visible = true;
                foreach (var gateObj in gateObjGroup.Objects)
                {
                    gateObj.Y -= 16;
                    var ent = Entity.Scene.CreateEntity(gateObj.Name);
                    ent.SetPosition(Entity.Position + new Vector2((int)gateObj.X, (int)gateObj.Y));
                    var gate = ent.AddComponent(new Gate(gateObj));

                    _gates.Add(gate);
                }
            }

            //triggers
            var triggerObjGroup = _map.GetObjectGroup("entrance-triggers");
            if (triggerObjGroup != null)
            {
                for (int i = 0; i < triggerObjGroup.Objects.Count; i++)
                {
                    var trigger = triggerObjGroup.Objects[i];

                    var ent = Entity.Scene.CreateEntity(trigger.Name);
                    ent.SetPosition(Entity.Position + new Vector2((int)trigger.X, (int)trigger.Y));
                    ent.AddComponent(new EntranceTrigger(trigger, this));
                    _triggerEntities.Add(ent);
                }
            }

            //enemy spawns
            var enemySpawnObjGroup = _map.GetObjectGroup("enemy-spawn-points");
            if (enemySpawnObjGroup != null)
            {
                foreach(var spawnObj in enemySpawnObjGroup.Objects)
                {
                    var ent = Entity.Scene.CreateEntity(spawnObj.Name);
                    ent.SetPosition(Entity.Position + new Vector2((int)spawnObj.X, (int)spawnObj.Y));
                    var enemySpawnPoint = ent.AddComponent(new EnemySpawnPoint());
                    _enemySpawnPoints.Add(enemySpawnPoint);
                }
            }

            //key chests
            if (_node.RoomType == RoomType.Key)
            {
                var keyChestObjGroup = _map.GetObjectGroup("key-chests");
                if (keyChestObjGroup != null)
                {
                    foreach(var keyChestObj in  keyChestObjGroup.Objects)
                    {
                        var ent = Entity.Scene.CreateEntity(keyChestObj.Name);
                        var translatedObjPosition = Entity.Position + new Vector2((int)keyChestObj.X, (int)keyChestObj.Y);
                        var entPosition = translatedObjPosition + new Vector2(keyChestObj.Width / 2, keyChestObj.Height / 2);
                        ent.SetPosition(entPosition);
                        var chest = ent.AddComponent(new KeyChest());
                    }
                }
            }

            //pre boss stuff
            if (_node.RoomType == RoomType.PreBoss)
            {
                //boss gate
                var bossGateObjGroup = _map.GetObjectGroup("boss-gate");
                if (bossGateObjGroup != null)
                {
                    var bossGateObj = bossGateObjGroup.Objects.FirstOrDefault();
                    var ent = Entity.Scene.CreateEntity(bossGateObj.Name);
                    var translatedPosition = Entity.Position + new Vector2((int)bossGateObj.X, (int)bossGateObj.Y);
                    var entPosition = translatedPosition + new Vector2(bossGateObj.Width / 2, bossGateObj.Height / 2);
                    ent.SetPosition(entPosition);
                    ent.AddComponent(new BossGate());
                }

                //cereal shelves
                var cerealShelvesObjGroup = _map.GetObjectGroup("shelves");
                if (cerealShelvesObjGroup != null)
                {
                    foreach(var obj in cerealShelvesObjGroup.Objects)
                    {
                        var ent = Entity.Scene.CreateEntity(obj.Name);
                        var translatedPosition = Entity.Position + new Vector2((int)obj.X, (int)obj.Y);
                        var entPosition = translatedPosition + new Vector2(obj.Width / 2, obj.Height / 2);
                        ent.SetPosition(entPosition);
                        ent.AddComponent(new CerealShelf());
                    }
                }
            }

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public void HandleEntranceTriggered()
        {
            if (_node.RoomType == RoomType.Normal)
            {
                if (!_isCleared)
                {
                    _encounterStarted = true;

                    //lock gates
                    foreach (var gate in _gates)
                    {
                        gate.Lock();
                    }

                    //spawn enemies
                    foreach (var spawn in _enemySpawnPoints)
                    {
                        _enemies.Add(spawn.SpawnEnemy(_enemyTypes, this));
                    }

                    //emit
                    Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);

                    //destroy entrance triggers
                    foreach (var ent in _triggerEntities)
                    {
                        ent.Destroy();
                    }
                }
            }
        }

        void OnTurnPhaseCompleted()
        {
            if (_encounterStarted)
            {
                _enemies.RemoveAll(e => e.Entity == null);
                if (_enemies.Any())
                {
                    Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
                }
                else
                {
                    _isCleared = true;

                    //unlock gates
                    foreach (var gate in _gates)
                    {
                        gate.Unlock();
                    }

                    Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterEnded);
                }
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            foreach (var ent in _triggerEntities)
            {
                ent.Destroy();
            }
        }
    }
}
