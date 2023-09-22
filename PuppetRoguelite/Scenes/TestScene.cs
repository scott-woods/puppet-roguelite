using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class TestScene : Scene
    {
        public override void Initialize()
        {
            base.Initialize();

            SetDesignResolution(640, 360, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);

            ClearColor = Color.Black;

            var playerEntity = CreateEntity("player");
            var player = playerEntity.AddComponent(new Player());
            playerEntity.SetPosition(640 / 2, 360 / 2);
        }
    }
}
