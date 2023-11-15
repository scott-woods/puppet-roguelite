using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Models.Upgrades;
using PuppetRoguelite.Models;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;

namespace PuppetRoguelite.UI.Menus
{
    public class ActionShopMenu : UICanvas
    {
        //elements
        Table _table;
        Dialog _dialog;
        List<ListButton> _attackButtons = new List<ListButton>();
        List<ListButton> _utilityButtons = new List<ListButton>();
        List<ListButton> _supportButtons = new List<ListButton>();

        //misc
        Skin _basicSkin;

        bool _canActivateButton = true;

        Action _closedCallback;

        public ActionShopMenu(Action closedCallback)
        {
            _closedCallback = closedCallback;
        }

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            //set stage action key
            Stage.KeyboardActionKey = Keys.E;

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //arrange elements
            ArrangeElements();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            _canActivateButton = false;
        }

        void ArrangeElements()
        {
            //create dialog
            _dialog = new Dialog("", _basicSkin);
            _table.Add(_dialog).Expand();

            //get internal table of dialog
            var contentTable = _dialog.GetContentTable();

            //set padding of dialog
            contentTable.PadTop(10).PadBottom(10).PadLeft(20).PadRight(20);
            contentTable.Defaults().SetSpaceBottom(0).SetSpaceTop(0);

            AddActionCategoryColumn(_basicSkin.GetDrawable("Style 4 Icon 005"), PlayerData.Instance.AttackActions,
                PlayerUpgradeData.Instance.AttackSlotsUpgrade.GetCurrentValue(), _attackButtons);

            AddActionCategoryColumn(_basicSkin.GetDrawable("Style 4 Icon 289"), PlayerData.Instance.UtilityActions,
                PlayerUpgradeData.Instance.UtilitySlotsUpgrade.GetCurrentValue(), _utilityButtons);

            AddActionCategoryColumn(_basicSkin.GetDrawable("Style 4 Icon 155"), PlayerData.Instance.SupportActions,
                PlayerUpgradeData.Instance.SupportSlotsUpgrade.GetCurrentValue(), _supportButtons);
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

        void AddActionCategoryColumn(IDrawable icon, List<PlayerActionType> actions, int maxSlots, List<ListButton> buttonList)
        {
            var contentTable = _dialog.GetContentTable();

            var columnTable = new Table().Top();
            contentTable.Add(columnTable).Fill();
            var iconImage = new Image(icon);
            iconImage.ScaleBy(Game1.ResolutionScale);
            columnTable.Add(iconImage);

            columnTable.Row();

            var lowerTable = new Table().Top().Left();
            lowerTable.DebugAll();
            columnTable.Add(lowerTable).SetSpaceTop(10).Fill();
            lowerTable.Defaults().SetSpaceBottom(5);

            for (int i = 0; i < maxSlots; i++)
            {
                var label = "";
                if (actions.Count > i)
                {
                    var action = actions[i];
                    label = PlayerActionUtils.GetName(action.ToType());
                }
                var button = new ListButton($"* {label}", _basicSkin);
                lowerTable.Add(button).Expand().Left();
                lowerTable.Row();
                buttonList.Add(button);
            }
        }
    }
}
