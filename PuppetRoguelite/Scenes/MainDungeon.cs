using FmodForFoxes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Textures;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI;
using PuppetRoguelite.UI.HUDs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        CombatManager _combatManager;
        PlayerSpawner _playerSpawner;
        ComboComponent _comboComponent;

        public override void Initialize()
        {
            base.Initialize();

            _playerSpawner = AddSceneComponent(new PlayerSpawner());
            _comboComponent = AddSceneComponent(new ComboComponent());

            ClearColor = Microsoft.Xna.Framework.Color.Transparent;

            //ui
            _ui = Camera.Entity.AddComponent(new CombatUI());

            //add player
            _playerEntity = _playerSpawner.CreatePlayerEntity();

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //add combat manager
            _combatManager = AddSceneComponent(new CombatManager());

            //mouse cursor
            var mouseEntity = CreateEntity("mouse-cursor");
            mouseEntity.AddComponent(new MouseCursor());

            //add dungenerator
            _dungenerator = AddSceneComponent(new Dungenerator());
        }

        public override void Begin()
        {
            base.Begin();

            _dungenerator.Generate();

            var spawn = _dungenerator.GetPlayerSpawnPoint();
            _playerSpawner.SpawnPlayer(_playerEntity, spawn);
            _playerEntity.SetPosition(spawn);

            Game1.AudioManager.PlayMusic(Music.Babbulon, true);
        }

        public override void End()
        {
            base.End();

            Game1.AudioManager.StopMusic();
        }
    }
}
