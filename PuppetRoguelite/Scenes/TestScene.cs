using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components;
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

            SetDesignResolution(480, 270, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);

            ClearColor = Color.Black;

            var playerEntity = CreateEntity("player");
            var player = playerEntity.AddComponent(new Player());
            playerEntity.SetPosition(480 / 2, 270 / 2);

            var projectileEntity = CreateEntity("projectile");
            var projectile = projectileEntity.AddComponent(new TestProjectile());
            projectileEntity.SetPosition(480 / 3, 270 / 3);
        }
    }
}
