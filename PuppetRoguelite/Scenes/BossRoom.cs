using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters.Boss;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI.HUDs;
using System;
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

        //components
        TiledMapRenderer _mapRenderer;
        TiledMapComponent _mapComponent;

        public BossSpawnPoint BossSpawnPoint;
        public Boss Boss;

        public override void Initialize()
        {
            base.Initialize();

            ClearColor = Color.Green;

            //tiled obj handler
            TiledObjectHandler = AddSceneComponent(new TiledObjectHandler());

            //load map
            var map = Content.LoadTiledMap(MapString);
            var mapEntity = CreateEntity("map");
            _mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            _mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
            _mapRenderer.RenderLayer = 10;
            _mapComponent = mapEntity.AddComponent(new TiledMapComponent(_mapRenderer));
            TiledObjectHandler.ProcessTiledMap(_mapRenderer, _mapComponent.Id);

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

            //add tiled object handler
            AddSceneComponent(new TiledObjectHandler());
        }

        public override void Begin()
        {
            base.Begin();

            BossSpawnPoint = FindComponentOfType<BossSpawnPoint>();

            //connect to trigger
            var trigger = FindComponentOfType<BossTrigger>();
            trigger.Emitter.AddObserver(TriggerEventTypes.Triggered, OnTriggered);
        }

        public override void OnStart()
        {
            base.OnStart();

            var spawn = FindComponentOfType<PlayerSpawnPoint>();
            _playerEntity.SetPosition(spawn.Entity.Position.X, spawn.Entity.Position.Y);
            _playerEntity.SetEnabled(true);

            var bossSpawn = FindComponentOfType<BossSpawnPoint>();
            var bossEntity = CreateEntity("boss", bossSpawn.Entity.Position);
            Boss = bossEntity.AddComponent(new Boss(_mapComponent.Id));
            //Boss.SetActive(false);
        }

        void OnTriggered()
        {
            var cutscene = new BossCutscene(this);
            Game1.StartCoroutine(cutscene.PlayScene());
        }
    }
}
