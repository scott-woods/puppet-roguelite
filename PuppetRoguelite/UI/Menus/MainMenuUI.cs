using Microsoft.Xna.Framework;
using Nez.UI;
using PuppetRoguelite.SaveData;
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
    public class MainMenuUI : CustomCanvas
    {
        //elements
        Table _mainTable;
        GrowButton _startButton;
        GrowButton _quitButton;

        //skin
        Skin _basicSkin;

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            _mainTable = Stage.AddElement(new Table());
            _mainTable.SetSize(Game1.UIResolution.X, Game1.UIResolution.Y);

            Stage.KeyboardActionKey = Microsoft.Xna.Framework.Input.Keys.E;

            //render layer
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            ArrangeElements();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Stage.SetGamepadFocusElement(_startButton);
        }

        void ArrangeElements()
        {
            //title
            var titleTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Title);
            var title = new Image(titleTexture);
            title.SetScale(2f);
            var titleJitter = new ElementJitter(title, 1f, .05f);
            _mainTable.Add(title).SetExpandX().SetPadTop(Value.PercentHeight(.2f, _mainTable));

            _mainTable.Row();

            //lower table
            var lowerTable = new Table();
            _mainTable.Add(lowerTable).Grow();

            var buttonTable = new Table();
            buttonTable.Defaults().SetSpaceBottom(50f);
            lowerTable.Add(buttonTable).GrowY();

            buttonTable.Row();

            var startNoBgTexture = Game1.Content.LoadTexture(Nez.Content.Textures.UI.Start_button);
            _startButton = new GrowButton(new SpriteDrawable(startNoBgTexture), 2.5f, 1.15f);
            _startButton.OnPressed += OnStartButtonPressed;
            buttonTable.Add(_startButton);

            buttonTable.Row();

            //quit
            var quitNoBgTexture = Game1.Content.LoadTexture(Nez.Content.Textures.UI.Quit_no_bg);
            _quitButton = new GrowButton(new SpriteDrawable(quitNoBgTexture), 2.5f, 1.15f);
            _quitButton.OnPressed += OnQuitButtonPressed;
            buttonTable.Add(_quitButton);

            _startButton.EnableExplicitFocusableControl(null, _quitButton, null, null);
            _quitButton.EnableExplicitFocusableControl(_startButton, null, null, null);
        }

        void OnStartButtonPressed()
        {
            _startButton.OnPressed -= OnStartButtonPressed;
            _startButton.Remove();
            _quitButton.Remove();

            Game1.AudioManager.StopMusic();

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Start_sound);

            Type sceneType = null;
            if (!GameContextData.Instance.HasCompletedIntro)
                sceneType = typeof(IntroArea);
            else sceneType = typeof(NewHub);

            Game1.SceneManager.ChangeScene(sceneType, "0", Color.White, fadeOutDuration: 2f, delayBeforeFadeInDuration: .5f, fadeInDuration: 1f);
        }

        void OnQuitButtonPressed()
        {
            _quitButton.OnPressed -= OnQuitButtonPressed;
            Game1.AudioManager.StopMusic();
            Game1.Exit();
        }
    }
}
