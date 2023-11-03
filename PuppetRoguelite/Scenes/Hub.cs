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
using PuppetRoguelite.Tools;
using PuppetRoguelite.Interfaces;

namespace PuppetRoguelite.Scenes
{
    public class Hub : BaseScene
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
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Hub);
            var mapRenderer = _mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "props" });
            mapRenderer.RenderLayer = 10;
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);
            _mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));

            //add player
            _playerEntity = _playerSpawner.CreatePlayerEntity();
            //var playerEntity = FindEntity("player");
            //if (playerEntity != null)
            //{
            //    _playerEntity = playerEntity;
            //    _playerEntity.AttachToScene(this);
            //    var components = _playerEntity.GetComponents<Component>().Where(c => c is ITransferrable).ToList();
            //    for (int i = 0; i < components.Count; i++)
            //    {
            //        var comp = components[i] as ITransferrable;
            //        comp.AddObservers();
            //    }
            //}
            //else
            //{
            //    _playerEntity = CreateEntity("player");
            //    _playerEntity.AddComponent(new PlayerController());
            //}
            //var transferEntity = TransferManager.Instance.GetEntityToTransfer();
            //if (transferEntity != null)
            //{
            //    _playerEntity = transferEntity;
            //    transferEntity.AttachToScene(this);
            //    var components = transferEntity.GetComponents<Component>().Where(c => c is ITransferrable).ToList();
            //    for (int i = 0; i < components.Count; i++)
            //    {
            //        var comp = components[i] as ITransferrable;
            //        comp.AddObservers();
            //    }
            //    transferEntity.SetEnabled(false);
            //}
            //else
            //{
            //    _playerEntity = CreateEntity("player");
            //    _playerEntity.AddComponent(new PlayerController());
            //}

            //add player
            //_playerEntity = new Entity("player");
            //AddEntity(_playerEntity);
            //_playerEntity.AddComponent(new PlayerController());

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

            _playerSpawner.SpawnPlayer(_playerEntity, _mapEntity);

            var exitToDungeon = FindComponentsOfType<ExitArea>().FirstOrDefault(e => e.MapEntity == _mapEntity && e.TargetSceneType == typeof(MainDungeon));
            if (exitToDungeon != null)
            {
                exitToDungeon.Emitter.AddObserver(ExitAreaEvents.Triggered, OnDungeonExitAreaTriggered);
            }
        }

        void OnDungeonExitAreaTriggered()
        {
            Game1.AudioManager.StopMusic();
        }
    }
}
