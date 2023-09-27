using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using PuppetRoguelite.Scenes;

namespace PuppetRoguelite
{
    public class Game1 : Core
    {
        public static Point DesignResolution = new Point(480, 270);

        protected override void Initialize()
        {
            base.Initialize();

            DebugRenderEnabled = true;
            Window.AllowUserResizing = true;
            //IsFixedTimeStep = true;

            Scene.SetDefaultDesignResolution(DesignResolution.X, DesignResolution.Y, Scene.SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);

            Scene = new TestScene();
        }
    }
}