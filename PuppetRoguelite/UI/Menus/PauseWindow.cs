using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Nez.UI;
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

            var resumeButton = new ListButton("Resume", _basicSkin, "listButton_xxl");
            resumeButton.OnClicked += OnResumeClicked;
            _pauseWindow.Add(resumeButton);

            _pauseWindow.Row();

            var quitButton = new ListButton("Quit to Desktop", _basicSkin, "listButton_xxl");
            quitButton.OnClicked += OnQuitClicked;
            _pauseWindow.Add(quitButton);
        }

        void OnResumeClicked(Button button)
        {
            Game1.GameStateManager.Unpause();
        }

        void OnQuitClicked(Button button)
        {
            Core.Exit();
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
