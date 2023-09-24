using Nez;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PuppetRoguelite.Scenes
{
    public class ScaledScene : Scene
    {
        Entity _playerEntity;

        public override void Initialize()
        {
            base.Initialize();

            SetDesignResolution(1920, 1080, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);

            ClearColor = Color.Black;

            //tilemap
            var tiledEntity = CreateEntity("tiled-map-entity");
            var map = Content.LoadTiledMap("Content/Maps/test_tilemap.tmx");
            var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
            tiledMapRenderer.RenderLayer = 10;

            _playerEntity = CreateEntity("player");
            var player = _playerEntity.AddComponent(new Player());
            _playerEntity.SetPosition(50, 50);

            //camera
            var followCamera = Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);
            Camera.SetMaximumZoom(4);
            Camera.SetZoom(1);

            //var projectileEntity = CreateEntity("projectile");
            //var projectile = projectileEntity.AddComponent(new TestProjectile());
            //projectileEntity.SetPosition(480 / 3, 270 / 3);

            var enemyEntity = CreateEntity("enemy");
            var enemy = enemyEntity.AddComponent(new TestEnemy());
            enemyEntity.SetPosition(1920 / 4, 1080 / 4);
        }

        public override void Update()
        {
            base.Update();

            Debug.Log(Camera.Position);
            Debug.Log(_playerEntity.Position);
        }
    }
}
