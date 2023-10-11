using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;
using PuppetRoguelite.Scenes;

namespace PuppetRoguelite
{
    public class Game1 : Core
    {
        public static Point DesignResolution = new Point(480, 270);
        public static SceneManager SceneManager = new SceneManager();

        protected override void Initialize()
        {
            base.Initialize();

            DebugRenderEnabled = false;
            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;

            RegisterGlobalManager(SceneManager);

            Scene.SetDefaultDesignResolution(DesignResolution.X, DesignResolution.Y, Scene.SceneResolutionPolicy.BestFit);
            Screen.SetSize(1920, 1080);

            Scene = new TestScene();
        }
    }
}