using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Components.Characters.Player.States;

namespace PuppetRoguelite.Scenes
{
    public class Bedroom : BaseScene
    {
        //scene components
        public PlayerSpawner PlayerSpawner;

        //entities
        Entity _playerEntity;
        Entity _mapEntity;

        public override void Initialize()
        {
            base.Initialize();

            PlayerSpawner = AddSceneComponent(new PlayerSpawner());

            //camera handler
            var camHandler = AddSceneComponent(new CameraHandler());

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Bedroom);
            var mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "props" });
            mapRenderer.RenderLayer = 10;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);
            _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));

            //add player
            _playerEntity = PlayerSpawner.CreatePlayerEntity();

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

            var camHandler = GetOrCreateSceneComponent<CameraHandler>();
            camHandler.FormatCamera();

            if (!Game1.AudioManager.IsPlayingMusic(Music.TheBay.Path))
            {
                Game1.AudioManager.PlayMusic(Music.TheBay, true);
            }

            PlayerSpawner.SpawnPlayer(_playerEntity, _mapEntity);
        }
    }
}
