using Nez;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Enums;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.HUDs
{
    public class PauseWindow : UICanvas
    {
        //elements
        Table _table;
        Dialog _dialog;

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

            Emitters.GameEventsEmitter.AddObserver(GameEvents.Unpaused, OnUnpaused);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Emitters.GameEventsEmitter.RemoveObserver(GameEvents.Unpaused, OnUnpaused);
        }

        void CreateUI()
        {
            _table = new Table();
            _table.SetWidth(Game1.UIResolution.X);
            _table.SetHeight(Game1.UIResolution.Y);
            Stage.AddElement(_table);

            _dialog = new Dialog("", _basicSkin);
            _table.Add(_dialog);

            var contentTable = _dialog.GetContentTable();
            contentTable.Pad(Game1.UIResolution.X * .03f);

            var resumeButton = new ListButton("Resume", _basicSkin, "listButton_xxl");
            resumeButton.OnClicked += OnResumeClicked;
            contentTable.Add(resumeButton);

            contentTable.Row();

            var quitButton = new ListButton("Quit to Desktop", _basicSkin, "listButton_xxl");
            quitButton.OnClicked += OnQuitClicked;
            contentTable.Add(quitButton);
        }

        void OnResumeClicked(Button button)
        {
            Game1.GameStateManager.Unpause();
        }

        void OnQuitClicked(Button button)
        {
            Game1.Exit();
        }

        void OnUnpaused()
        {
            Entity.Destroy();
        }
    }
}
