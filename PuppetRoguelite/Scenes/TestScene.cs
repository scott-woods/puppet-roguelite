using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Textures;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class TestScene : BaseScene
    {
        //entities
        Entity _playerEntity;

        //components
        CombatUI _ui;

        public override void Initialize()
        {
            base.Initialize();

            ClearColor = Color.Transparent;

            //ui
            var uiEntity = CreateEntity("ui");
            _ui = uiEntity.AddComponent(new CombatUI());

            //tilemap
            var tiledEntity = new PausableEntity("tiled-map-entity");
            AddEntity(tiledEntity);
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Test_tilemap);
            var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
            tiledMapRenderer.RenderLayer = 10;

            //pathfinding
            var graph = new AstarGridGraph(tiledMapRenderer.CollisionLayer);
            AddSceneComponent(new GridGraphManager(graph, map));

            //add player
            _playerEntity = new Entity("player");
            AddEntity(_playerEntity);
            var player = _playerEntity.AddComponent(new PlayerController());
            _playerEntity.SetPosition(480 / 3, 270 / 3);

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);
            //Camera.MaximumZoom = 4;
            //Camera.MinimumZoom = .3f;
            //Camera.Zoom = .45f;

            var chainBotEntity = new PausableEntity("chain-bot-entity");
            AddEntity(chainBotEntity);
            var chainBot = chainBotEntity.AddComponent(new ChainBot());
            chainBotEntity.SetPosition(64, 64);

            //add combat manager
            AddSceneComponent(new CombatManager());
        }

        public override void Begin()
        {
            base.Begin();

            Emitters.CombatEventsEmitter.Emit(CombatEvents.DodgePhaseStarted);
        }
    }
}
