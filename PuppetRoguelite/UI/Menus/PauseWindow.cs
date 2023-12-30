using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class PauseWindow : CustomCanvas
    {
        //elements
        Table _table;
        WindowTable _pauseWindow;
        BulletPointSelector<string> _resumeButton;

        Skin _basicSkin;

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            //render layer
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            CreateUI();

            Stage.KeyboardActionKey = Microsoft.Xna.Framework.Input.Keys.E;
            Stage.SetGamepadFocusElement(_resumeButton);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);

            Emitters.GameEventsEmitter.AddObserver(GameEvents.Unpaused, OnUnpaused);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Core.Emitter.RemoveObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);

            Emitters.GameEventsEmitter.RemoveObserver(GameEvents.Unpaused, OnUnpaused);
        }

        public void CreateUI()
        {
            _table?.Clear();
            _table = null;

            //base table
            _table = Stage.AddElement(new Table()).Center();
            _table.SetFillParent(false);
            _table.SetWidth(Game1.UIResolution.X);
            _table.SetHeight(Game1.UIResolution.Y);

            _pauseWindow = new WindowTable(_basicSkin);
            _table.Add(_pauseWindow).Expand();

            _pauseWindow.Pad(Game1.UIResolution.X * .03f);

            _resumeButton = new BulletPointSelector<string>("Resume", "", false, _basicSkin, "default_xxl");
            _resumeButton.OnSelected += (res) => Game1.GameStateManager.Unpause();
            _pauseWindow.Add(_resumeButton);

            _pauseWindow.Row();

            var quitToMenuButton = new BulletPointSelector<string>("Quit to Main Menu", "", false, _basicSkin, "default_xxl");
            quitToMenuButton.OnSelected += (res) =>
            {
                Game1.GameStateManager.Unpause();
                Game1.AudioManager.StopMusic();
                Game1.SceneManager.ChangeScene(typeof(MainMenu));
                Emitters.GameEventsEmitter.Emit(GameEvents.ExitingToMainMenu);
            };
            _pauseWindow.Add(quitToMenuButton);

            _pauseWindow.Row();

            var quitButton = new BulletPointSelector<string>("Quit to Desktop", "", false, _basicSkin, "default_xxl");
            quitButton.OnSelected += (res) => Core.Exit();
            _pauseWindow.Add(quitButton);
        }

        void OnUnpaused()
        {
            Entity.Destroy();
        }

        void OnGraphicsDeviceReset()
        {
            CreateUI();
        }
    }
}
