using Nez;
using PuppetRoguelite.Components.Cameras;
using PuppetRoguelite.Components;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Components.TiledComponents;

namespace PuppetRoguelite.Scenes
{
    public class TutorialRoom : BaseScene
    {
        //entities
        Entity _playerEntity;
        Entity _mapEntity;

        //scene components
        PlayerSpawner _playerSpawner;

        public override void Initialize()
        {
            base.Initialize();

            CreateEntity("ui").AddComponent(new CombatUI());

            //scene components
            _playerSpawner = AddSceneComponent(new PlayerSpawner());

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.DungeonPrison.Dp_tutorial);
            TiledMapRenderer mapRenderer;
            if (map.Layers.Contains("Walls"))
                mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "Walls"));
            else
                mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map));
            var mainMapValidLayers = new List<string> { "Back", "AboveBack", "Walls" };
            mapRenderer.SetLayersToRender(map.Layers.Where(l => mainMapValidLayers.Contains(l.Name)).Select(l => l.Name).ToArray());
            mapRenderer.RenderLayer = 1;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);
            _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));
            _mapEntity.AddComponent(new GridGraphManager(mapRenderer));

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
        }

        public override void OnStart()
        {
            base.OnStart();

            Game1.AudioManager.StopMusic();

            Game1.AudioManager.PlayMusic(Songs.BabyMode);

            _playerSpawner.SpawnPlayer(_mapEntity);
        }
    }
}
