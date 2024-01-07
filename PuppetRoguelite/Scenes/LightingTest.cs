using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Cameras;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class LightingTest : Scene
    {
        Entity _playerEntity;
        Entity _mapEntity;
        PlayerSpawner _playerSpawner;
        RenderLayerRenderer _lightRenderer;
        SpriteLightPostProcessor _spriteLightPostProcessor;

        public override void Initialize()
        {
            base.Initialize();

            //scene components
            _playerSpawner = AddSceneComponent(new PlayerSpawner());

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Hub.Hub_2);
            var mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "Walls"));
            mapRenderer.SetLayersToRender(new[] { "Back", "AboveBack", "Walls" });
            mapRenderer.RenderLayer = 1;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);
            _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));

            //create above map renderer
            var tiledMapDetailsRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map));
            var layersToRender = new List<string>();
            if (map.Layers.Contains("Front"))
                layersToRender.Add("Front");
            if (map.Layers.Contains("AboveFront"))
                layersToRender.Add("AboveFront");
            tiledMapDetailsRenderer.SetLayersToRender(layersToRender.ToArray());
            tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
            tiledMapDetailsRenderer.Material = Material.StencilWrite();

            //add player
            _playerEntity = _playerSpawner.CreatePlayerEntity();

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //mouse cursor
            var mouseEntity = CreateEntity("mouse-cursor");
            mouseEntity.AddComponent(new MouseCursor());

            AddRenderer(new RenderLayerExcludeRenderer(0, RenderLayers.ScreenSpaceRenderLayer, RenderLayers.LightLayer));
            _lightRenderer = AddRenderer(new RenderLayerRenderer(-1, RenderLayers.LightLayer));
            _lightRenderer.RenderTexture = new RenderTexture();
            _lightRenderer.RenderTargetClearColor = new Color(100, 100, 100, 255);

            _spriteLightPostProcessor = AddPostProcessor(new SpriteLightPostProcessor(0, _lightRenderer.RenderTexture));

            
        }

        public override void OnStart()
        {
            base.OnStart();

            //spawn player
            _playerSpawner.SpawnPlayer(_mapEntity);

            var lightTexture = Content.LoadTexture(Nez.Content.Textures.Effects.Spritelight);

            var lightEntity = CreateEntity("light");
            var sprite = lightEntity.AddComponent(new SpriteRenderer(lightTexture));
            lightEntity.Position = PlayerController.Instance.Entity.Position;
            lightEntity.Scale = new Vector2(3f);
            sprite.RenderLayer = RenderLayers.LightLayer;
            sprite.SetColor(new Color(255, 255, 255, 128));
        }
    }
}
