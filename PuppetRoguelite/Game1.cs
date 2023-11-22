using FmodForFoxes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Scenes;
using System;
using System.IO;

namespace PuppetRoguelite
{
    public class Game1 : Core
    {
        public static Point LowerResolution = new Point(480, 270);
        public static Point DesignResolution = new Point(480, 270);
        //public static Point UIResolution { get => Screen.Size.ToPoint(); }
        //public static Point UIResolution = new Point(480, 270);
        public static Point UIResolution = new Point(1920, 1080);
        public static Vector2 ResolutionScale { get => UIResolution.ToVector2() / DesignResolution.ToVector2(); }

        //global managers
        public static SceneManager SceneManager = new SceneManager();
        public static AudioManager AudioManager = new AudioManager();
        public static GameStateManager GameStateManager = new GameStateManager();
        public static ResolutionManager ResolutionManager = new ResolutionManager();

        protected override void Initialize()
        {
            base.Initialize();

            DebugRenderEnabled = false;
            Window.AllowUserResizing = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = System.TimeSpan.FromSeconds((double)1 / 240);
            IsMouseVisible = false;
            PauseOnFocusLost = true;
            ExitOnEscapeKeypress = false;

            if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
            Inspectables.Initialize();

            Physics.SpatialHashCellSize = 32;
            Physics.Gravity = new Vector2(0, 600f);

            FmodManager.Init(new DesktopNativeFmodLibrary(), FmodInitMode.CoreAndStudio, "");

            RegisterGlobalManager(SceneManager);
            RegisterGlobalManager(AudioManager);
            RegisterGlobalManager(GameStateManager);
            RegisterGlobalManager(ResolutionManager);

            Scene.SetDefaultDesignResolution(DesignResolution.X, DesignResolution.Y, Scene.SceneResolutionPolicy.BestFit);
            Screen.SetSize(1920, 1080);

            SceneManager.TargetEntranceId = "0";
            Scene = new NewHub();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            FmodManager.Update();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            FmodManager.Unload();
        }
    }
}