using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters.HeartHoarder;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nez.Content.Textures;

namespace PuppetRoguelite.Scenes
{
    public class BossRoom : BaseScene
    {
        public const string MapString = Nez.Content.Tiled.Tilemaps.Boss_room;

        //scene components
        public TiledObjectHandler TiledObjectHandler;

        //entities
        Entity _playerEntity;
        Entity _mapEntity;
        Entity TurnHandlerEntity;

        //components
        TiledMapRenderer _mapRenderer;
        //TiledMapComponent _mapComponent;

        public BossSpawnPoint BossSpawnPoint;
        public HeartHoarder Boss;

        public int Turns = 0;

        public override void Initialize()
        {
            base.Initialize();

            //CREATE MAP

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Boss_room);
            var mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
            mapRenderer.RenderLayer = 10;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

            //grid graph
            var gridGraphManager = _mapEntity.AddComponent(new GridGraphManager(mapRenderer));

            //tiled obj handler
            var tiledObjectHandler = _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));

            //tiled obj handler
            //TiledObjectHandler = AddSceneComponent(new TiledObjectHandler());

            ////load map
            //var map = Content.LoadTiledMap(MapString);
            //var mapEntity = CreateEntity("map");
            //_mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            //_mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
            //_mapRenderer.RenderLayer = 10;
            //_mapComponent = mapEntity.AddComponent(new TiledMapComponent(_mapRenderer));
            //TiledObjectHandler.ProcessTiledMap(_mapRenderer, _mapComponent.Id);

            //ui
            Camera.Entity.AddComponent(new CombatUI());

            //add player
            _playerEntity = new Entity("player");
            AddEntity(_playerEntity);
            var player = _playerEntity.AddComponent(new PlayerController());
            _playerEntity.SetEnabled(false);

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //add combat manager
            AddSceneComponent(new CombatManager());

            var mouseEntity = CreateEntity("mouse");
            mouseEntity.AddComponent(new MouseCursor());

            //add tiled object handler
            //AddSceneComponent(new TiledObjectHandler());
        }

        public override void Begin()
        {
            base.Begin();

            BossSpawnPoint = FindComponentOfType<BossSpawnPoint>();

            //connect to trigger
            var trigger = FindComponentOfType<BossTrigger>();
            trigger.Emitter.AddObserver(TriggerEventTypes.Triggered, OnTriggered);

            //connect to combat events
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public override void OnStart()
        {
            base.OnStart();

            //spawn player
            var spawn = FindComponentOfType<PlayerSpawnPoint>();
            _playerEntity.SetPosition(spawn.Entity.Position.X, spawn.Entity.Position.Y);
            _playerEntity.SetEnabled(true);

            //spawn boss
            var bossSpawn = FindComponentOfType<BossSpawnPoint>();
            var bossEntity = AddEntity(new PausableEntity("boss"));
            bossEntity.SetPosition(bossSpawn.Entity.Position);
            Boss = bossEntity.AddComponent(new HeartHoarder(_mapEntity));
            Boss.SetEnabled(false);
        }

        void OnTriggered()
        {
            Game1.StartCoroutine(StartBattle());
            //var cutscene = new BossCutscene(this);
            //Game1.StartCoroutine(cutscene.PlayScene());
        }

        void OnTurnPhaseTriggered()
        {
            Turns += 1;

            //create turn handler
            TurnHandlerEntity = CreateEntity("turn-handler");
            TurnHandlerEntity.AddComponent(new TurnHandler());
        }

        void OnTurnPhaseCompleted()
        {
            if (Boss.Entity != null)
            {
                Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
            }
            else
            {
                //unlock gates
                var gates = FindComponentsOfType<Gate>().Where(g => g.MapEntity == _mapEntity).ToList();
                foreach (var gate in gates)
                {
                    gate.Unlock();
                }

                Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterEnded);
            }
        }

        IEnumerator StartBattle()
        {
            Boss.SetEnabled(true);

            //play animation
            yield return Boss.PlayAppearanceAnimation();

            //activate boss so it can start idling
            Boss.Activate();

            yield return Coroutine.WaitForSeconds(2f);

            //start combat
            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);
            Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
            Boss.StartCombat();
        }
    }
}
