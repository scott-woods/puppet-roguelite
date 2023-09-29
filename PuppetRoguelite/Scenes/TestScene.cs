using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Textures;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI;
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
            var tiledEntity = CreateEntity("tiled-map-entity");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Test_tilemap);
            var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
            tiledMapRenderer.RenderLayer = 10;

            //pathfinding
            //var graph = new AstarGridGraph(tiledMapRenderer.CollisionLayer);
            //AddSceneComponent(new GridGraphManager(graph, map));

            _playerEntity = CreateEntity("player");
            var player = _playerEntity.AddComponent(new Player());
            _playerEntity.SetPosition(480 / 3, 270 / 3);

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);
            //Camera.MaximumZoom = 4;
            //Camera.MinimumZoom = .3f;
            //Camera.Zoom = .45f;

            //var projectileEntity = CreateEntity("projectile");
            //var projectile = projectileEntity.AddComponent(new TestProjectile());
            //projectileEntity.SetPosition(480 / 3, 270 / 3);

            //var enemyEntity = CreateEntity("enemy");
            //var enemy = enemyEntity.AddComponent(new TestEnemy());
            //enemyEntity.SetPosition(480 / 4, 270 / 4);

            //var chainBotEntity = CreateEntity("chain-bot-entity");
            //var chainBot = chainBotEntity.AddComponent(new ChainBot());
            //chainBotEntity.SetPosition(64, 64);
        }
    }
}
