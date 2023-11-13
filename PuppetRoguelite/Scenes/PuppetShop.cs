using Nez;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Components;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PuppetRoguelite.Scenes
{
    public class PuppetShop : BaseScene
    {
        //scene components
        PlayerSpawner _playerSpawner;

        //entities
        Entity _mapEntity;
        Entity _playerEntity;

        public override void Initialize()
        {
            base.Initialize();

            //scene components
            _playerSpawner = AddSceneComponent(new PlayerSpawner());

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Hub.Puppet_shop);
            var mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "props" });
            mapRenderer.RenderLayer = 10;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);
            _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));

            var tiledMapDetailsRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map));
            tiledMapDetailsRenderer.SetLayerToRender("above-details");
            tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
            tiledMapDetailsRenderer.Material = Material.StencilWrite();
            //tiledMapDetailsRenderer.Material.Effect = Content.LoadNezEffect<SpriteAlphaTestEffect>();

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

            if (!Game1.AudioManager.IsPlayingMusic(Nez.Content.Audio.Music.The_bay))
            {
                Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.The_bay);
            }

            _playerSpawner.SpawnPlayer(_mapEntity);
        }
    }
}
