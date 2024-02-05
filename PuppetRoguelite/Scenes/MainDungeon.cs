using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Cameras;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.SceneComponents.CombatManager;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.HUDs;

namespace PuppetRoguelite.Scenes
{
    public class MainDungeon : BaseScene
    {
        //entities
        Entity _playerEntity;

        //components
        CombatUI _ui;

        //scene components
        Dungenerator _dungenerator;
        BSPDungenerator _bspDungenerator;
        CombatManager _combatManager;
        PlayerSpawner _playerSpawner;

        public override void Initialize()
        {
            base.Initialize();

            _playerSpawner = AddSceneComponent(new PlayerSpawner());

            ClearColor = Microsoft.Xna.Framework.Color.Transparent;

            //ui
            //_ui = Camera.Entity.AddComponent(new CombatUI());
            _ui = CreateEntity("ui").AddComponent(new CombatUI());

            //add player
            _playerEntity = _playerSpawner.CreatePlayerEntity();

            //add combat manager
            _combatManager = AddSceneComponent(new CombatManager());

            //mouse cursor
            var mouseEntity = CreateEntity("mouse-cursor");
            mouseEntity.AddComponent(new MouseCursor());

            //add dungenerator
            _dungenerator = AddSceneComponent(new Dungenerator());

            _bspDungenerator = AddSceneComponent(new BSPDungenerator());
        }

        public override void Begin()
        {
            base.Begin();

            //_dungenerator.Generate();
            _bspDungenerator.Generate();

            //var spawn = _dungenerator.GetPlayerSpawnPoint();
            var spawn = new Vector2(0, 0);
            _playerSpawner.SpawnPlayer(spawn);
            _playerEntity.SetPosition(spawn);
            Camera.Position = _playerEntity.Position;

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            Game1.AudioManager.PlayMusic(Songs.Babbulon, true);

            DungeonRuns.Instance.StartNewRun();
        }

        public override void End()
        {
            base.End();

            Game1.AudioManager.StopMusic();
        }
    }
}
