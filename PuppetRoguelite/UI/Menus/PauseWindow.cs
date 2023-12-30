using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
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
    public class PauseWindow : CustomCanvas
    {
        //elements
        Table _table;
        WindowTable _pauseWindow;
        WindowTable _settingsWindow;
        BulletPointSelector<string> _resumeButton;
        BulletPointSelector<string> _settingsButton;

        Skin _basicSkin;

        //misc
        bool _isInSettingsWindow = false;
        int _lastSelectedIndex = 0;

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
            //Stage.SetGamepadFocusElement(_resumeButton);
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
            CreateMainTable();

            if (!_isInSettingsWindow)
            {
                DisplayPauseWindow();
            }
            else
            {
                DisplaySettingsWindow();
            }
        }

        void CreateMainTable()
        {
            _table?.Clear();
            _table = null;

            //base table
            _table = Stage.AddElement(new Table()).Center();
            _table.SetFillParent(false);
            _table.SetWidth(Game1.UIResolution.X);
            _table.SetHeight(Game1.UIResolution.Y);
        }

        void DisplayPauseWindow()
        {
            //update state
            _isInSettingsWindow = false;

            //clear table
            _table?.Clear();

            //create window
            _pauseWindow = new WindowTable(_basicSkin);
            _table.Add(_pauseWindow).Expand();
            _pauseWindow.Pad(Game1.UIResolution.X * .03f);

            List<BulletPointSelector<string>> buttons = new List<BulletPointSelector<string>>();

            //resume button
            _resumeButton = new BulletPointSelector<string>("Resume", "", false, _basicSkin, "default_xxl");
            _resumeButton.OnSelected += (res) =>
            {
                _lastSelectedIndex = 0;
                Game1.GameStateManager.Unpause();
            };
            _pauseWindow.Add(_resumeButton);
            buttons.Add(_resumeButton);

            _pauseWindow.Row();

            //settings button
            _settingsButton = new BulletPointSelector<string>("Settings", "", false, _basicSkin, "default_xxl");
            _settingsButton.OnSelected += (res) =>
            {
                _lastSelectedIndex = 1;
                DisplaySettingsWindow();
            };
            _pauseWindow.Add(_settingsButton);
            buttons.Add(_settingsButton);

            _pauseWindow.Row();

            var quitToMenuButton = new BulletPointSelector<string>("Quit to Main Menu", "", false, _basicSkin, "default_xxl");
            quitToMenuButton.OnSelected += (res) =>
            {
                _lastSelectedIndex = 2;
                Game1.GameStateManager.Unpause();
                Game1.AudioManager.StopMusic();
                Game1.SceneManager.ChangeScene(typeof(MainMenu));
                Emitters.GameEventsEmitter.Emit(GameEvents.ExitingToMainMenu);
            };
            _pauseWindow.Add(quitToMenuButton);
            buttons.Add(quitToMenuButton);

            _pauseWindow.Row();

            var quitButton = new BulletPointSelector<string>("Quit to Desktop", "", false, _basicSkin, "default_xxl");
            quitButton.OnSelected += (res) =>
            {
                _lastSelectedIndex = 3;
                Core.Exit();
            };
            _pauseWindow.Add(quitButton);
            buttons.Add(quitButton);

            //set focus
            Stage.SetGamepadFocusElement(buttons[_lastSelectedIndex]);
        }

        void DisplaySettingsWindow()
        {
            //update state
            _isInSettingsWindow = true;

            //clear table
            _table?.Clear();

            var containerTable = new Table();
            _table.Add(containerTable).Expand();

            _settingsWindow = new WindowTable(_basicSkin);
            containerTable.Add(_settingsWindow);
            containerTable.Row();
            var backLabel = new Label("X: Go Back", _basicSkin, "default_md");
            containerTable.Add(backLabel).Left().SetPadLeft(10f);

            _settingsWindow.Pad(Game1.UIResolution.X * .03f);

            var settingsHeader = new Label("Settings", _basicSkin, "default_xxl");
            _settingsWindow.Add(settingsHeader).SetSpaceBottom(Game1.UIResolution.X * .015f);

            _settingsWindow.Row();

            var settingsTable = new Table();
            settingsTable.Defaults().Space(25f);
            _settingsWindow.Add(settingsTable).Grow();

            var soundVolumeTable = new Table();
            soundVolumeTable.Defaults().Space(10f);
            settingsTable.Add(soundVolumeTable).Grow();
            var soundVolumeLabel = new Label("SFX Volume", _basicSkin, "default_lg");
            soundVolumeTable.Add(soundVolumeLabel);
            soundVolumeTable.Row();
            var soundSlider = new SettingsSlider(0, 1, .1f, false, new SliderStyle(new PrimitiveDrawable(10f, Color.White), new PrimitiveDrawable(20f, Color.White)));
            soundSlider.Value = Settings.Instance.SoundVolume;
            soundSlider.ShouldUseExplicitFocusableControl = true;
            soundSlider.SetTouchable(Touchable.Enabled);
            soundSlider.OnChanged += (newValue) =>
            {
                Settings.Instance.SoundVolume = newValue;
                Settings.Instance.UpdateAndSave();
            };
            soundSlider.OnMouseOver += (slider) =>
            {
                Stage.SetGamepadFocusElement(slider);
            };
            soundVolumeTable.Add(soundSlider);

            settingsTable.Row();

            var musicVolumeTable = new Table();
            musicVolumeTable.Defaults().Space(10f);
            settingsTable.Add(musicVolumeTable).Grow();
            var musicVolumeLabel = new Label("Music Volume", _basicSkin, "default_lg");
            musicVolumeTable.Add(musicVolumeLabel);
            musicVolumeTable.Row();
            var musicSlider = new SettingsSlider(0, 1, .1f, false, new SliderStyle(new PrimitiveDrawable(10f, Color.White), new PrimitiveDrawable(20f, Color.White)));
            musicSlider.Value = Settings.Instance.MusicVolume;
            musicSlider.ShouldUseExplicitFocusableControl = true;
            musicSlider.SetTouchable(Touchable.Enabled);
            musicSlider.OnChanged += (newValue) =>
            {
                Settings.Instance.MusicVolume = newValue;
                Settings.Instance.UpdateAndSave();
            };
            musicSlider.OnMouseOver += (slider) =>
            {
                Stage.SetGamepadFocusElement(slider);
            };
            musicVolumeTable.Add(musicSlider);

            soundSlider.EnableExplicitFocusableControl(musicSlider, musicSlider, null, null);
            musicSlider.EnableExplicitFocusableControl(soundSlider, soundSlider, null, null);

            Stage.SetGamepadFocusElement(soundSlider);
        }

        public override void Update()
        {
            base.Update();

            if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
            {
                if (_isInSettingsWindow)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    DisplayPauseWindow();
                }
            }
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
