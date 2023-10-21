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
    public class TestScene : BaseScene
    {
        //entities
        Entity _playerEntity;

        //components
        CombatUI _ui;

        //scene components
        Dungenerator _dungenerator;
        CombatManager _combatManager;

        public override void Initialize()
        {
            base.Initialize();

            ClearColor = Microsoft.Xna.Framework.Color.Transparent;

            //ui
            _ui = Camera.Entity.AddComponent(new CombatUI());

            //add player
            _playerEntity = new Entity("player");
            AddEntity(_playerEntity);
            var player = _playerEntity.AddComponent(new PlayerController());
            _playerEntity.SetPosition(new Vector2(64, 64));

            //camera
            Camera.Entity.AddComponent(new DeadzoneFollowCamera(_playerEntity, new Vector2(0, 0)));
            Camera.Entity.SetUpdateOrder(int.MaxValue);

            //add combat manager
            _combatManager = AddSceneComponent(new CombatManager());

            //add tiled object handler
            //AddSceneComponent(new TiledObjectHandler());

            //add dungenerator
            _dungenerator = AddSceneComponent(new Dungenerator());
        }

        public override void Begin()
        {
            base.Begin();

            _dungenerator.Generate();
            //Game1.AudioManager.PlayMusic(Nez.Content.Audio.Music.Babbulon_double, false, 125902);
        }

        public override void End()
        {
            base.End();

            Game1.AudioManager.StopMusic();
        }
    }
}
