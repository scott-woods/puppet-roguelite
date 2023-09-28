using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Textures;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.PostProcessors;
using PuppetRoguelite.Renderers;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class TestScene : Scene
    {
        //entities
        Entity _playerEntity;

        //components
        CombatUI _ui;

        //renderers
        ScreenSpaceRenderer _screenSpaceRenderer;

        public TestScene()
        {
            //_screenSpaceRenderer = new ScreenSpaceRenderer(100, 999);
            ////_screenSpaceRenderer.RenderTexture = new RenderTexture(1920, 1080);
            //_screenSpaceRenderer.ShouldDebugRender = false;
            //_screenSpaceRenderer.Camera.Zoom = 1;
            //FinalRenderDelegate = this;
        }

        //public override void Begin()
        //{
        //    var mainRenderer = AddRenderer(new RenderLayerExcludeRenderer(0, new[] { 999 }));
        //    //mainRenderer.Material = Material.StencilRead();
        //    mainRenderer.RenderTargetClearColor = Color.Black;

        //    var uiRenderTarget = new RenderTexture(1920, 1080);
        //    uiRenderTarget.ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None;
        //    var uiRenderer = new ScreenSpaceRenderer(-1, 999);
        //    uiRenderer.RenderTexture = uiRenderTarget;
        //    uiRenderer.WantsToRenderAfterPostProcessors = false;
        //    AddRenderer(uiRenderer);
        //    AddPostProcessor(new SimplePostProcessor(0, uiRenderer.RenderTexture));

        //    base.Begin();
        //}

        public override void Initialize()
        {
            base.Initialize();

            ClearColor = Color.Black;

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
            var graph = new AstarGridGraph(tiledMapRenderer.CollisionLayer);
            AddSceneComponent(new GridGraphManager(graph, map));

            _playerEntity = CreateEntity("player");
            var player = _playerEntity.AddComponent(new Player());
            _playerEntity.SetPosition(480 / 3, 270 / 3);

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);
            Camera.MaximumZoom = 4;
            Camera.MinimumZoom = .3f;
            Camera.Zoom = .45f;

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
