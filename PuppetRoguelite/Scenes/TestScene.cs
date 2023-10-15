using FmodForFoxes;
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
using PuppetRoguelite.UI;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        //scene components
        Dungenerator _dungenerator;

        public override void Initialize()
        {
            base.Initialize();

            ClearColor = Microsoft.Xna.Framework.Color.Transparent;

            //ui
            //var uiEntity = CreateEntity("ui");
            _ui = Camera.Entity.AddComponent(new CombatUI());

            //tilemap
            //var tiledEntity = new PausableEntity("tiled-map-entity");
            //AddEntity(tiledEntity);
            //var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.TBLR_1);
            //var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            //tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
            //tiledMapRenderer.RenderLayer = 10;

            ////2nd tilemap
            //var tiledEntity2 = new PausableEntity("tiled-map-entity2");
            //tiledEntity2.SetPosition(new Vector2(16 * 24, 0));
            //AddEntity(tiledEntity2);
            //var map2 = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.TBLR_1);
            //var tiledMapRenderer2 = tiledEntity2.AddComponent(new TiledMapRenderer(map2, "collision"));
            //tiledMapRenderer2.SetLayersToRender(new[] { "floor", "details" });
            //tiledMapRenderer2.RenderLayer = 10;

            //pathfinding
            //var graph = new AstarGridGraph(tiledMapRenderer.CollisionLayer);
            //AddSceneComponent(new GridGraphManager(graph, map));

            //add player
            _playerEntity = new Entity("player");
            AddEntity(_playerEntity);
            var player = _playerEntity.AddComponent(new PlayerController());
            _playerEntity.SetPosition(new Vector2(64, 64));

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //var chainBotEntity = new PausableEntity("chain-bot-entity");
            //AddEntity(chainBotEntity);
            //var chainBot = chainBotEntity.AddComponent(new ChainBot());
            //chainBotEntity.SetPosition(64, 64);

            //add combat manager
            AddSceneComponent(new CombatManager());

            //add tiled object handler
            AddSceneComponent(new TiledObjectHandler());

            //add dungenerator
            _dungenerator = AddSceneComponent(new Dungenerator());
        }

        public override void Begin()
        {
            base.Begin();

            //var dungenerator = new Dungenerator();
            //dungenerator.CreateGraph();
            //var maps = dungenerator.GetMaps();
            //foreach (var map in maps)
            //{
            //    var tiledEntity = new PausableEntity("tiled-map-entity");
            //    tiledEntity.SetPosition(map.Value);
            //    var mapData = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.TBLR_1);
            //    var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(mapData, "collision"));
            //    tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
            //    tiledMapRenderer.RenderLayer = 10;
            //}

            _dungenerator.Generate();
            Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.Babbulon_double, false, 125902);
        }
    }
}
