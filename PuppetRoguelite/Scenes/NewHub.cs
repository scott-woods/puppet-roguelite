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
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.Components.Characters.NPCs.Proto;
using System.Collections;

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

            Camera.SetPosition(_playerEntity.Position);

            //mouse cursor
            var mouseEntity = CreateEntity("mouse-cursor");
            mouseEntity.AddComponent(new MouseCursor());
        }

        public override void Begin()
        {
            base.Begin();

            //configure observer for exit to dungeon
            var exitToDungeon = FindComponentsOfType<ExitArea>().FirstOrDefault(e => e.MapEntity == _mapEntity && e.TargetSceneType == typeof(MainDungeon));
            if (exitToDungeon != null)
            {
                exitToDungeon.Emitter.AddObserver(ExitAreaEvents.Triggered, OnDungeonExitAreaTriggered);
            }

            //spawn player
            _playerSpawner.SpawnPlayer(_mapEntity);
            Camera.Position = _playerEntity.Position;

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //if haven't completed intro, enable cutscene trigger
            var sceneTrigger = FindComponentsOfType<AreaTrigger>().FirstOrDefault(t => t.TmxObject.Name == "HubIntro");
            if (!GameContextData.Instance.HasCompletedIntro)
            {
                void OnIntroTriggered()
                {
                    sceneTrigger.OnTriggered -= OnIntroTriggered;
                    Game1.StartCoroutine(HubIntro());
                }
                sceneTrigger.OnTriggered += OnIntroTriggered;
                return;
            }
            else
            {
                sceneTrigger.Entity.Destroy();
            }

            if (Game1.SceneManager.State == GlobalManagers.SceneManagerState.Transitioning)
                Game1.SceneManager.Emitter.AddObserver(GlobalManagers.SceneEvents.TransitionEnded, OnSceneTransitionEnded);
            else
                Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.The_bay);

            void OnSceneTransitionEnded()
            {
                Game1.SceneManager.Emitter.RemoveObserver(GlobalManagers.SceneEvents.TransitionEnded, OnSceneTransitionEnded);
                Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.The_bay);
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

        IEnumerator HubIntro()
        {
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Welcome home, superstar.")
            });

            var camera = Camera.GetComponent<DeadzoneFollowCamera>();
            camera.SetFollowTarget(null);
            var proto = FindComponentOfType<Proto>();
            var camTween = camera.Entity.TweenPositionTo(proto.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            Game1.AudioManager.PlayMusic(Songs.TheBay);

            yield return Coroutine.WaitForSeconds(.5f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("It's a shame we've got to start from scratch, but it's nothing we can't overcome."),
                new DialogueLine("This is the Hub, our little base of operations."),
                new DialogueLine("I am Proto, interim Gamemaster."),
            });

            camTween = camera.Entity.TweenPositionTo(FindComponentOfType<ActionShop>().Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("That's Madame Florence, owner of the Action Shop."),
                new DialogueLine("She is basically the result of putting 1000 Grandmas into a blender and forming one super Grandma."),
                new DialogueLine("Despite what that might imply, she's quite capable with engineering."),
                new DialogueLine("Talk to her to swap out your Actions.")
            });

            camTween = camera.Entity.TweenPositionTo(FindComponentOfType<UpgradeShop>().Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Over here is Storald. He's, uh... not the best with money."),
                new DialogueLine("He was what he called a 'Professional Loan Acquirer' for several years."),
                new DialogueLine("I think he thought he could pay off the original loans by getting new ones..."),
                new DialogueLine("In any case, he's now running the Upgrade Shop, his first legitimate business."),
                new DialogueLine("Talk to him if you want to upgrade your Stats, like Max HP, AP, or your Action Slots.")
            });

            camTween = camera.Entity.TweenPositionTo(proto.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning()) yield return null;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Each one of us is here to help you."),
                new DialogueLine("When you die, we'll repair you."),
                new DialogueLine("When you succeed, we'll revel with you."),
                new DialogueLine("Anything you need, just ask."),
                new DialogueLine("...Don't be weird."),
                new DialogueLine("Oh, and one more thing.")
            });

            var hubShelf = FindComponentsOfType<InteractTrigger>().FirstOrDefault(t => t.TmxObject.Name == "HubShelf");
            camTween = camera.Entity.TweenPositionTo(hubShelf.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning()) yield return null;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("If you ever need to go through our little Tutorial again, just interact with this Shelf."),
                new DialogueLine("There's a magical book in there that will transport you back to that memory."),
                new DialogueLine("...is that a good enough explanation for you? It's magic stuff, deal with it.")
            });

            camTween = camera.Entity.TweenPositionTo(proto.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning()) yield return null;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Come and talk to me if you ever need advice or just want to chat."),
                new DialogueLine("Have fun out there!")
            });

            camTween = camera.Entity.TweenPositionTo(PlayerController.Instance.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning()) yield return null;

            camera.SetFollowTarget(PlayerController.Instance.Entity);

            GameContextData.Instance.HasCompletedIntro = true;

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);
        }
    }
}
