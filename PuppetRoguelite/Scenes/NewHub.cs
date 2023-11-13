using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Components;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PuppetRoguelite.Scenes
{
    public class NewHub : BaseScene
    {
        //entities
        Entity _playerEntity;
        Entity _mapEntity;

        //scene components
        PlayerSpawner _playerSpawner;

        public override void Initialize()
        {
            base.Initialize();

            //scene components
            _playerSpawner = AddSceneComponent(new PlayerSpawner());

            //map renderer
            _mapEntity = CreateEntity("map");
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Hub_2);
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

            var exitToDungeon = FindComponentsOfType<ExitArea>().FirstOrDefault(e => e.MapEntity == _mapEntity && e.TargetSceneType == typeof(MainDungeon));
            if (exitToDungeon != null)
            {
                exitToDungeon.Emitter.AddObserver(ExitAreaEvents.Triggered, OnDungeonExitAreaTriggered);
            }
        }

        public override void End()
        {
            base.End();

            var exitToDungeon = FindComponentsOfType<ExitArea>().FirstOrDefault(e => e.MapEntity == _mapEntity && e.TargetSceneType == typeof(MainDungeon));
            if (exitToDungeon != null)
            {
                exitToDungeon.Emitter.RemoveObserver(ExitAreaEvents.Triggered, OnDungeonExitAreaTriggered);
            }
        }

        void OnDungeonExitAreaTriggered()
        {
            PlayerController.Instance.DollahInventory.Dollahs = 0;
            PlayerData.Instance.UpdateAndSave();
            PlayerUpgradeData.Instance.UpdateAndSave();
            Game1.AudioManager.StopMusic();
        }
    }
}
