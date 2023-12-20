using Nez;
using PuppetRoguelite.Components.Cameras;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Components;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Collections;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.Components.Characters.Player;

namespace PuppetRoguelite.Scenes
{
    public class IntroArea : BaseScene
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
            var map = Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.Hub.Hub_intro);
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

            Game1.StartCoroutine(IntroCoroutine());
        }

        IEnumerator IntroCoroutine()
        {
            _playerSpawner.SpawnPlayer(_mapEntity);

            //disable lever for now
            var wallLever = FindComponentOfType<WallLever>();
            wallLever.CanBeInteractedWith = false;

            //wait a few seconds
            yield return Coroutine.WaitForSeconds(5f);

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Hello? Hey! Can you hear me in there?"),
                new DialogueLine("Hopefully your speaker isn't fried... unless the rats chewed up your wiring."),
                new DialogueLine("I'm just gonna keep talking and hope for the best, alright?"),
                new DialogueLine("First thing, let's get you out of that old storeroom."),
                new DialogueLine("They sealed it up a long time ago, but don't worry, I left you a way to escape just in case."),
                new DialogueLine("You see that Lever over on the wall there?")
            });

            var camera = Camera.GetComponent<DeadzoneFollowCamera>();
            camera.SetFollowTarget(null);

            var camTween = camera.Entity.TweenPositionTo(wallLever.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
               new DialogueLine("Run over there and hit that thing for me, would you?"),
               new DialogueLine("Just walk up to it and press 'E' to interact with it.")
            });

            camTween = camera.Entity.TweenPositionTo(PlayerController.Instance.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            camera.SetFollowTarget(PlayerController.Instance.Entity);

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            wallLever.CanBeInteractedWith = true;
            wallLever.OnLeverPulled += OnLeverPulled;
            bool leverPulled = false;
            void OnLeverPulled()
            {
                leverPulled = true;
                var bossGate = FindComponentOfType<BossGate>();
                bossGate.AddKey();
                bossGate.AddKey();
            }

            while (!leverPulled)
                yield return null;

            yield return Coroutine.WaitForSeconds(.5f);

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Oh, it actually worked! Wonderful!"),
                new DialogueLine("I've been so looking forward to resuming our work."),
                new DialogueLine("You do remember what to do, don't you?")
            });

            var choices = new List<string>() { "Yes, I know what to do! (Skip Tutorial)", "...? (Go to Tutorial)" };
            var selectedChoice = -1;
            yield return GlobalTextboxManager.DisplayChoices(choices, index => selectedChoice = index);

            switch (selectedChoice)
            {
                case 0:
                    yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                    {
                        new DialogueLine("Ah, I knew you were still in there. C'mon, let's get started.")
                    });
                    GameContextData.Instance.HasCompletedIntro = true;
                    Game1.SceneManager.ChangeScene(typeof(NewHub), "0", fadeOutDuration: 2f, delayBeforeFadeInDuration: 1f, fadeInDuration: 1f);
                    break;
                case 1:
                    yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                    {
                        new DialogueLine("Oh, I see..."),
                        new DialogueLine("Well, no matter. I'm sure we can get you back up and running in no time."),
                        new DialogueLine("They didn't put all of that craftsmanship into you for nothing...")
                    });
                    Game1.SceneManager.ChangeScene(typeof(TutorialRoom), "0", fadeOutDuration: 2f, delayBeforeFadeInDuration: 1f, fadeInDuration: 1f);
                    break;
            }
        }
    }
}
