using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class UpgradeShopMenu : CustomCanvas
    {
        //elements
        Table _table;
        List<UpgradeButton> _upgradeButtons = new List<UpgradeButton>();
        Label _totalDollahsLabel;
        WindowTable _box;

        //misc
        Skin _basicSkin;
        Sprite _dollahSprite;
        Dictionary<int, Label> _upgradeValueLabels = new Dictionary<int, Label>();
        Dictionary<int, Label> _upgradeCostLabels = new Dictionary<int, Label>();
        Dictionary<UpgradeBase, Label> _nextLevelLabels = new Dictionary<UpgradeBase, Label>();

        Action<bool> _closedCallback;
        bool _purchaseMade = false;

        bool _canActivateButton = false;

        public UpgradeShopMenu(Action<bool> closedCallback)
        {
            _closedCallback = closedCallback;
        }

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(false);
            _table.SetWidth(Game1.UIResolution.X);
            _table.SetHeight(Game1.UIResolution.Y);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            //set stage action key
            Stage.KeyboardActionKey = Controls.Instance.UIActionKey;
            Stage.GamepadActionButton = Controls.Instance.UIActionButton;
            Stage.IsSelectionEnabled = false;

            //load dollah sprite
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Tilesets.Dungeon_prison_props);
            _dollahSprite = new Sprite(texture, new Rectangle(80, 240, 16, 16));

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //arrange elements
            ArrangeElements();

            //focus the first button
            Stage.SetGamepadFocusElement(_upgradeButtons.First());
        }

        void ArrangeElements()
        {
            //container table
            var containerTable = new Table();
            _table.Add(containerTable).Expand();

            //textbox table
            _box = new WindowTable(_basicSkin);
            containerTable.Add(_box).Expand().Width(Game1.UIResolution.X * .75f).Height(Game1.UIResolution.Y * .75f).SetUniformX();

            //set padding of dialog
            _box.PadTop(50).PadBottom(50).PadLeft(30).PadRight(30);

            //header
            var headerLabel = new Label("Upgrade Shop", _basicSkin, "default_xxxl");
            _box.Add(headerLabel).Top().SetSpaceBottom(25f);

            _box.Row();

            //total dollahs
            var topRightTable = new Table();
            _box.Add(topRightTable).Expand().Right();
            _totalDollahsLabel = new Label(PlayerData.Instance.Dollahs.ToString(), _basicSkin, "default_xxxl");
            topRightTable.Add(_totalDollahsLabel).SetPadTop(4);
            var dollahImage = new Image(_dollahSprite);
            dollahImage.SetScaleX(Game1.ResolutionScale.X);
            dollahImage.SetScaleY(Game1.ResolutionScale.Y);
            topRightTable.Add(dollahImage);

            _box.Row();

            var upgradeTable = new Table();
            upgradeTable.Defaults().SetSpaceBottom(10);
            _box.Add(upgradeTable).SetSpaceTop(50).Grow();

            //add row for each upgrade
            AddRow(upgradeTable, PlayerUpgradeData.Instance.MaxHpUpgrade);
            AddRow(upgradeTable, PlayerUpgradeData.Instance.MaxApUpgrade);
            AddRow(upgradeTable, PlayerUpgradeData.Instance.AttackSlotsUpgrade);
            AddRow(upgradeTable, PlayerUpgradeData.Instance.UtilitySlotsUpgrade);
            AddRow(upgradeTable, PlayerUpgradeData.Instance.SupportSlotsUpgrade);

            ValidateButtons();

            //tip label
            containerTable.Row();
            var tipLabel = new Label("X: Go Back", _basicSkin, "default_lg");
            containerTable.Add(tipLabel).SetUniformX().SetExpandX().Left().Top().SetPadLeft(10);
        }

        void AddRow(Table table, UpgradeBase upgrade)
        {
            //upgrade label
            var cell = table.Add(new Label(upgrade.Name, _basicSkin, "default_xxxl")).SetExpandX().Left().SetPadTop(4);

            //value table
            var valueTable = new Table();
            table.Add(valueTable).SetExpandX().Left().SetPadTop(4);

            //upgrade value
            var valueLabel = new Label(upgrade.GetValueString(), _basicSkin, "default_xxxl");
            _upgradeValueLabels.Add(cell.GetRow(), valueLabel);
            valueTable.Add(valueLabel);

            //add label for next upgrade
            if (!upgrade.IsMaxLevel())
            {
                var nextValueLabel = new Label(" -> " + upgrade.GetValueString(upgrade.CurrentLevel + 1), _basicSkin, "default_xxl");
                valueTable.Add(nextValueLabel);
                nextValueLabel.SetVisible(false);
                _nextLevelLabels.Add(upgrade, nextValueLabel);
            }

            //upgrade button
            var button = new UpgradeButton(upgrade, cell.GetRow(), _basicSkin, "plusButton");
            table.Add(button).SetExpandX().Right().SetSpaceRight(10);

            //button focus handlers
            button.OnButtonFocused += OnPlusButtonFocused;
            button.OnButtonUnfocused += OnPlusButtonUnfocused;

            //button clicked handler
            button.OnClicked += OnPlusButtonClicked;
            _upgradeButtons.Add(button);

            //cost label
            var costText = upgrade.IsMaxLevel() ? "MAX" : upgrade.GetCostToUpgrade().ToString();
            var costLabel = new Label(costText, _basicSkin, "default_xxl");
            _upgradeCostLabels.Add(cell.GetRow(), costLabel);
            table.Add(costLabel).SetPadTop(4).Right();
            
            //dollah image next to cost
            var dollahImage = new Image(_dollahSprite);
            dollahImage.SetScaleX(Game1.ResolutionScale.X);
            dollahImage.SetScaleY(Game1.ResolutionScale.Y);
            table.Add(dollahImage);

            table.Row();
        }

        public override void Update()
        {
            base.Update();

            //wait until e is released so button isn't instantly clicked
            if (!_canActivateButton && !Controls.Instance.Confirm.IsDown)
            {
                _canActivateButton = true;
                Stage.IsSelectionEnabled = true;
            }

            //handle going back
            if (Controls.Instance.Cancel.IsPressed)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                _closedCallback?.Invoke(_purchaseMade);
            }
        }

        void OnPlusButtonClicked(Button button)
        {
            if (_canActivateButton)
            {
                var upgradeButton = button as UpgradeButton;

                //if can't afford
                if (PlayerData.Instance.Dollahs < upgradeButton.Upgrade.GetCostToUpgrade())
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    return;
                }

                //if at max level
                if (upgradeButton.Upgrade.IsMaxLevel())
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    return;
                }

                //play purchase sound
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Purchase);

                //set purchase made to true (so shopowner knows to thank you)
                _purchaseMade = true;
                
                //apply upgrade
                upgradeButton.Upgrade.ApplyUpgrade();

                //update total dollahs label
                _totalDollahsLabel.SetText(PlayerData.Instance.Dollahs.ToString());

                //update value display
                var valueLabel = _upgradeValueLabels[upgradeButton.RowIndex];
                valueLabel.SetText(upgradeButton.Upgrade.GetValueString());

                //update next level label
                var nextLevelLabel = _nextLevelLabels[upgradeButton.Upgrade];
                if (upgradeButton.Upgrade.IsMaxLevel())
                {
                    nextLevelLabel.Remove();
                    _nextLevelLabels.Remove(upgradeButton.Upgrade);
                }
                else
                {
                    nextLevelLabel.SetText($" -> {upgradeButton.Upgrade.GetValueString(upgradeButton.Upgrade.CurrentLevel + 1)}");
                }

                //update cost display
                var costLabel = _upgradeCostLabels[upgradeButton.RowIndex];
                if (upgradeButton.Upgrade.IsMaxLevel())
                {
                    costLabel.SetText("MAX");
                }
                else
                {
                    costLabel.SetText(upgradeButton.Upgrade.GetCostToUpgrade().ToString());
                }

                //update enabled/disabled for all buttons
                ValidateButtons();
            }
        }

        void ValidateButtons()
        {
            foreach(var button in _upgradeButtons)
            {
                var cost = button.Upgrade.GetCostToUpgrade();
                if (cost > PlayerData.Instance.Dollahs)
                {
                    button.SetDisabled(true);
                    continue;
                }

                if (button.Upgrade.IsMaxLevel())
                {
                    button.SetDisabled(true);
                    continue;
                }
            }
        }

        void OnPlusButtonFocused(UpgradeBase upgrade)
        {
            if (_nextLevelLabels.ContainsKey(upgrade))
            {
                _nextLevelLabels[upgrade].SetVisible(true);
            }
        }

        void OnPlusButtonUnfocused(UpgradeBase upgrade)
        {
            if (_nextLevelLabels.ContainsKey(upgrade))
            {
                _nextLevelLabels[upgrade].SetVisible(false);
            }
        }
    }
}
