using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.Models.Upgrades;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class UpgradeShopMenu : UICanvas
    {
        //elements
        Table _table;
        Dialog _dialog;
        List<Button> _addButtons = new List<Button>();
        Label _totalDollahsLabel;

        //misc
        Skin _basicSkin;
        Sprite _dollahSprite;
        Dictionary<int, Label> _upgradeValueLabels = new Dictionary<int, Label>();
        Dictionary<int, Label> _upgradeCostLabels = new Dictionary<int, Label>();

        Action _closedCallback;

        bool _canActivateButton = true;

        public UpgradeShopMenu(Action closedCallback)
        {
            _closedCallback = closedCallback;
        }

        public override void Initialize()
        {
            base.Initialize();

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            //set stage action key
            Stage.KeyboardActionKey = Keys.E;

            //load dollah sprite
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Tilesets.Dungeon_prison_props);
            _dollahSprite = new Sprite(texture, new Rectangle(80, 240, 16, 16));

            SetRenderLayer(int.MinValue);

            //arrange elements
            ArrangeElements();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            _canActivateButton = false;
            Stage.SetGamepadFocusElement(_addButtons.First());
        }

        void ArrangeElements()
        {
            //create dialog
            _dialog = new Dialog("", _basicSkin);
            _table.Add(_dialog).Width(Entity.Scene.SceneRenderTargetSize.X * .6f).Height(Entity.Scene.SceneRenderTargetSize.Y * .75f);

            //get internal table of dialog
            var contentTable = _dialog.GetContentTable();

            //set padding of dialog
            contentTable.PadTop(10).PadBottom(10).PadLeft(20).PadRight(20).Top();
            contentTable.Defaults().SetSpaceBottom(0).SetSpaceTop(0);

            //total dollahs
            var topRightTable = new Table();
            contentTable.Add(topRightTable).SetColspan(3).SetExpandX().Right();
            _totalDollahsLabel = new Label(PlayerData.Instance.Dollahs.ToString(), _basicSkin);
            topRightTable.Add(_totalDollahsLabel).SetPadTop(4);
            topRightTable.Add(new Image(_dollahSprite));

            //add row for each upgrade
            AddRow(contentTable, PlayerUpgradeData.Instance.MaxHpUpgrade);
            AddRow(contentTable, PlayerUpgradeData.Instance.MaxApUpgrade);
            AddRow(contentTable, PlayerUpgradeData.Instance.AttackSlotsUpgrade);
            AddRow(contentTable, PlayerUpgradeData.Instance.UtilitySlotsUpgrade);
            AddRow(contentTable, PlayerUpgradeData.Instance.SupportSlotsUpgrade);
        }

        void AddRow(Table contentTable, Upgrade upgrade)
        {
            contentTable.Row();

            //upgrade label
            var cell = contentTable.Add(new Label(upgrade.Name, _basicSkin)).SetExpandX().Left().SetPadTop(4);

            //upgrade value
            var valueLabel = new Label(upgrade.GetValueString(), _basicSkin);
            _upgradeValueLabels.Add(cell.GetRow(), valueLabel);
            contentTable.Add(valueLabel).SetExpandX().Left().SetPadTop(4);

            //upgrade cost and purchase button
            var purchaseTable = new Table();
            contentTable.Add(purchaseTable).SetExpandX().Right();
            var button = new UpgradeButton(upgrade, cell.GetRow(), _basicSkin, "plusButton");
            var canAfford = PlayerData.Instance.Dollahs >= upgrade.GetCurrentCost();
            button.SetDisabled(!canAfford);
            button.OnClicked += OnPlusButtonClicked;
            _addButtons.Add(button);
            purchaseTable.Add(button).SetSpaceRight(4);
            var costLabel = new Label(upgrade.GetCurrentCost().ToString(), _basicSkin);
            _upgradeCostLabels.Add(cell.GetRow(), costLabel);
            purchaseTable.Add(costLabel).SetPadTop(4);
            purchaseTable.Add(new Image(_dollahSprite));
        }

        public override void Update()
        {
            base.Update();

            if (!_canActivateButton && !Input.IsKeyDown(Keys.E))
            {
                _canActivateButton = true;
            }

            if (Input.IsKeyPressed(Keys.X))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                _closedCallback?.Invoke();
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

                PlayerData.Instance.Dollahs -= upgradeButton.Upgrade.GetCurrentCost();
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
