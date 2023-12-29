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
        List<Button> _addButtons = new List<Button>();
        Label _totalDollahsLabel;
        WindowTable _box;

        //misc
        Skin _basicSkin;
        Sprite _dollahSprite;
        Dictionary<int, Label> _upgradeValueLabels = new Dictionary<int, Label>();
        Dictionary<int, Label> _upgradeCostLabels = new Dictionary<int, Label>();

        Action<bool> _closedCallback;
        bool _purchaseMade = false;

        bool _canActivateButton = true;

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
            Stage.KeyboardActionKey = Keys.None;

            //load dollah sprite
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Tilesets.Dungeon_prison_props);
            _dollahSprite = new Sprite(texture, new Rectangle(80, 240, 16, 16));

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //arrange elements
            ArrangeElements();

            Stage.SetGamepadFocusElement(_addButtons.First());
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            _canActivateButton = false;
            //Stage.SetGamepadFocusElement(_addButtons.First());
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

            //tip label
            containerTable.Row();
            var tipLabel = new Label("X: Go Back", _basicSkin, "default_lg");
            containerTable.Add(tipLabel).SetUniformX().SetExpandX().Left().Top();
        }

        void AddRow(Table table, Upgrade upgrade)
        {
            //upgrade label
            var cell = table.Add(new Label(upgrade.Name, _basicSkin, "default_xxxl")).SetExpandX().Left().SetPadTop(4);

            //upgrade value
            var valueLabel = new Label(upgrade.GetValueString(), _basicSkin, "default_xxxl");
            _upgradeValueLabels.Add(cell.GetRow(), valueLabel);
            table.Add(valueLabel).SetExpandX().Left().SetPadTop(4);

            var button = new UpgradeButton(upgrade, cell.GetRow(), _basicSkin, "plusButton");
            var canAfford = PlayerData.Instance.Dollahs >= upgrade.GetCurrentCost();
            //button.SetDisabled(!canAfford);
            button.OnClicked += OnPlusButtonClicked;
            _addButtons.Add(button);
            table.Add(button).SetExpandX().Right().SetSpaceRight(10);

            var costLabel = new Label(upgrade.GetCurrentCost().ToString(), _basicSkin, "default_xxl");
            _upgradeCostLabels.Add(cell.GetRow(), costLabel);
            table.Add(costLabel).SetPadTop(4).Right();
            var dollahImage = new Image(_dollahSprite);
            dollahImage.SetScaleX(Game1.ResolutionScale.X);
            dollahImage.SetScaleY(Game1.ResolutionScale.Y);
            table.Add(dollahImage);

            table.Row();
        }

        public override void Update()
        {
            base.Update();

            if (!_canActivateButton && !Input.IsKeyDown(Keys.E))
            {
                _canActivateButton = true;
                Stage.KeyboardActionKey = Keys.E;
            }

            if (Input.IsKeyPressed(Keys.X))
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
                if (PlayerData.Instance.Dollahs < upgradeButton.Upgrade.GetCurrentCost())
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    return;
                }

                //if at max level
                if (upgradeButton.Upgrade.CurrentLevel >= upgradeButton.Upgrade.Levels.Count - 1)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    return;
                }

                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Purchase);

                _purchaseMade = true;

                PlayerController.Instance.DollahInventory.Dollahs -= upgradeButton.Upgrade.GetCurrentCost();
                //PlayerData.Instance.Dollahs -= upgradeButton.Upgrade.GetCurrentCost();
                _totalDollahsLabel.SetText(PlayerData.Instance.Dollahs.ToString());

                upgradeButton.Upgrade.ApplyUpgrade();

                var valueLabel = _upgradeValueLabels[upgradeButton.RowIndex];
                valueLabel.SetText(upgradeButton.Upgrade.GetValueString());

                var costLabel = _upgradeCostLabels[upgradeButton.RowIndex];
                costLabel.SetText(upgradeButton.Upgrade.GetCurrentCost().ToString());
            }
        }
    }
}
