using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class TestScene : Scene
    {
        Entity _playerEntity;

        public override void Initialize()
        {
            base.Initialize();

            SetDesignResolution(480, 270, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);

            ClearColor = Color.Black;

            //tilemap
            var tiledEntity = CreateEntity("tiled-map-entity");
            var map = Content.LoadTiledMap("Content/Maps/test_tilemap.tmx");
            var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
            tiledMapRenderer.RenderLayer = 10;

            //pathfinding
            var graph = new AstarGridGraph(tiledMapRenderer.CollisionLayer);
            AddSceneComponent(new GridGraphManager(graph, map));

            _playerEntity = CreateEntity("player");
            var player = _playerEntity.AddComponent(new Player());
            _playerEntity.SetPosition(480 / 3, 270 / 3);

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //var projectileEntity = CreateEntity("projectile");
            //var projectile = projectileEntity.AddComponent(new TestProjectile());
            //projectileEntity.SetPosition(480 / 3, 270 / 3);

            //var enemyEntity = CreateEntity("enemy");
            //var enemy = enemyEntity.AddComponent(new TestEnemy());
            //enemyEntity.SetPosition(480 / 4, 270 / 4);

            var chainBotEntity = CreateEntity("chain-bot-entity");
            var chainBot = chainBotEntity.AddComponent(new ChainBot());
            chainBotEntity.SetPosition(64, 64);
        }
    }
}
