using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.StaticData;
using SDL2;
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
        public static Vector2 ResolutionScale { get => MonoGameCompat.ToVector2(UIResolution) / MonoGameCompat.ToVector2(DesignResolution); }

        //global managers
        public static SceneManager SceneManager = new SceneManager();
        public static AudioManager AudioManager = new AudioManager();
        public static GameStateManager GameStateManager = new GameStateManager();
        public static ResolutionManager ResolutionManager = new ResolutionManager();

        public Game1() : base()
        {
            System.Environment.SetEnvironmentVariable("FNA_OPENGL_BACKBUGGER_SCALE_NEAREST", "1");
        }

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

            //FmodManager.Update();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            //FmodManager.Unload();
        }
    }
}