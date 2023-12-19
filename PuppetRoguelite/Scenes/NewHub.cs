using Nez;
using PuppetRoguelite.Components.Characters.Player;
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
using PuppetRoguelite.UI.HUDs;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.Components.Cameras;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Components.Effects;

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

            CreateEntity("ui").AddComponent(new CombatUI());

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
        }

        public override void OnStart()
        {
            base.OnStart();

            if (Game1.SceneManager.State == GlobalManagers.SceneManagerState.Transitioning)
                Game1.SceneManager.Emitter.AddObserver(GlobalManagers.SceneEvents.TransitionEnded, OnSceneTransitionEnded);
            else
                Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.The_bay);

            void OnSceneTransitionEnded()
            {
                Game1.SceneManager.Emitter.RemoveObserver(GlobalManagers.SceneEvents.TransitionEnded, OnSceneTransitionEnded);
                Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.The_bay);
            }

            //if (!Game1.AudioManager.IsPlayingMusic(Nez.Content.Audio.Music.The_bay))
            //{
            //    Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.The_bay);
            //}

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
            PlayerData.Instance.UpdateAndSave();
            PlayerUpgradeData.Instance.UpdateAndSave();
            Game1.AudioManager.StopMusic();
        }
    }
}
