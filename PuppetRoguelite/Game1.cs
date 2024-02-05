using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.UI;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.StaticData;
using SDL2;
using Serilog;
using System;
using System.IO;

namespace PuppetRoguelite
{
    public class Game1 : Core
    {
        public static Point LowerResolution = new Point(480, 270);
        public static Point DesignResolution = new Point(1920, 1080);
        public static Point BleedArea = new Point(4, 4);
        //public static Point UIResolution { get => Screen.Size.ToPoint(); }
        //public static Point UIResolution = new Point(480, 270);
        public static Point UIResolution = new Point(1920, 1080);
        //public static Vector2 ResolutionScale { get => MonoGameCompat.ToVector2(UIResolution) / MonoGameCompat.ToVector2(DesignResolution); }
        public static Vector2 ResolutionScale { get => new Vector2(4, 4); }
        public static Point DesignResolutionWithBleed { get => DesignResolution + BleedArea; }

        //global managers
        public static SceneManager SceneManager = new SceneManager();
        public static AudioManager AudioManager = new AudioManager();
        public static GameStateManager GameStateManager = new GameStateManager();
        public static ResolutionManager ResolutionManager = new ResolutionManager();
        public static DebugSettings DebugSettings = new DebugSettings();
        public static EventManager EventManager = new EventManager();
        public static InputStateManager InputStateManager = new InputStateManager();

        public Game1() : base()
        {
            System.Environment.SetEnvironmentVariable("FNA_OPENGL_BACKBUGGER_SCALE_NEAREST", "1");
        }

        protected override void Initialize()
        {
            base.Initialize();

            //configure logger
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //setup fna logger ext
            FNALoggerEXT.LogInfo = (msg) => Log.Information($"FNA: {msg}", msg);
            FNALoggerEXT.LogWarn = (msg) => Log.Warning($"FNA: {msg}", msg);
            FNALoggerEXT.LogError = (msg) => Log.Error($"FNA: {msg}", msg);

            //global variables
            DebugRenderEnabled = false;
            Window.AllowUserResizing = false;
            IsFixedTimeStep = true;
            SDL.SDL_GetCurrentDisplayMode(SDL.SDL_GetWindowDisplayIndex(Game1.Instance.Window.Handle), out var mode);
            var refreshRate = mode.refresh_rate == 0 ? 60 : mode.refresh_rate;
            TargetElapsedTime = System.TimeSpan.FromSeconds((double)1 / refreshRate);
            IsMouseVisible = false;
            PauseOnFocusLost = true;
            ExitOnEscapeKeypress = false;
            Graphics.Instance.Batcher.ShouldRoundDestinations = false;

            //init data directory
            if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");

            //init inspectables
            Inspectables.Initialize();

            //physics config
            Physics.SpatialHashCellSize = 32;
            Physics.Gravity = new Vector2(0, 600f);
            Physics.RaycastsHitTriggers = true;
            Physics.RaycastsStartInColliders = true;

            //global managers
            RegisterGlobalManager(SceneManager);
            RegisterGlobalManager(AudioManager);
            RegisterGlobalManager(GameStateManager);
            RegisterGlobalManager(ResolutionManager);
            RegisterGlobalManager(DebugSettings);
            RegisterGlobalManager(EventManager);
            RegisterGlobalManager(InputStateManager);

            //resolution and screen size
            Scene.SetDefaultDesignResolution(DesignResolutionWithBleed.X, DesignResolutionWithBleed.Y, Scene.SceneResolutionPolicy.LinearBleed, BleedArea.X, BleedArea.Y);
            Screen.SetSize(1920, 1080);

            Window.Title = "Threadlock";

            //start scene
            Log.Information("Starting initial Scene");
            SceneManager.TargetEntranceId = "0";
            Scene = new MainMenu();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //FmodManager.Update();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            Log.Information("Unloading Content");

            //FmodManager.Unload();

            Log.CloseAndFlush();
        }
    }
}