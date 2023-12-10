using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class StatsMenu : CustomCanvas, IUpdatable
    {
        //elements
        Table _mainTable;
        WindowTable _statsAndActionsWindowTable;
        Table _statsTable;
        Table _actionsTable;
        WindowTable _infoTable;
        Table _attacksTable;
        Table _utilitiesTable;
        Table _supportTable;
        List<FocusLabel> _attackButtons = new List<FocusLabel>();
        List<FocusLabel> _utilityButtons = new List<FocusLabel>();
        List<FocusLabel> _supportButtons = new List<FocusLabel>();
        Label _infoTableNameLabel, _infoTableTypeLabel, _infoTableCostLabel, _infoTableDescriptionLabel;

        //misc
        Skin _basicSkin;

        Action _closedCallback;

        public StatsMenu(Action closedCallback)
        {
            _closedCallback = closedCallback;
        }

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //base table
            _mainTable = Stage.AddElement(new Table());
            _mainTable.SetFillParent(false);
            _mainTable.SetWidth(Game1.UIResolution.X);
            _mainTable.SetHeight(Game1.UIResolution.Y);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            //set stage action key
            Stage.KeyboardActionKey = Keys.E;

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            ArrangeElements();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            Stage.SetGamepadFocusElement(_attackButtons.First());
        }

        public override void Update()
        {
            base.Update();

            if (Input.IsKeyPressed(Keys.Tab))
            {
                _closedCallback?.Invoke();
                Entity.Destroy();
            }
        }

        void ArrangeElements()
        {
            //stats and actions window
            _statsAndActionsWindowTable = new WindowTable(_basicSkin);
            _statsAndActionsWindowTable.Pad(50f);
            _mainTable.Add(_statsAndActionsWindowTable)
                .Expand(1, 3)
                .SetUniformX()
                .Top().Left()
                .SetPadTop(Game1.UIResolution.Y * .05f)
                .SetPadLeft(Game1.UIResolution.Y * .05f)
                .Width(Game1.UIResolution.X * .4f)
                .SetFillY();

            //stats table
            _statsTable = new Table();
            _statsTable.DebugAll();
            _statsAndActionsWindowTable.Add(_statsTable)
                .GrowX()
                .Top().Left();

            //hp label
            var hpLabel = new Label($"HP: {PlayerController.Instance.HealthComponent.Health}/{PlayerController.Instance.HealthComponent.MaxHealth}", _basicSkin, "default_lg");
            hpLabel.SetAlignment(Align.Left);
            _statsTable.Add(hpLabel).Expand().Left();

            _statsTable.Row();

            //ap label
            var apLabel = new Label($"Max AP: {PlayerUpgradeData.Instance.MaxApUpgrade.GetCurrentMaxAp()}", _basicSkin, "default_lg");
            apLabel.SetAlignment(Align.Left);
            _statsTable.Add(apLabel).Expand().Left();

            _statsTable.Row();

            //dollahs label
            var dollahsLabel = new Label($"Dollahs: {PlayerData.Instance.Dollahs}", _basicSkin, "default_lg");
            dollahsLabel.SetAlignment(Align.Left);
            _statsTable.Add(dollahsLabel).Expand().Left();

            _statsAndActionsWindowTable.Row();

            //actions table
            _actionsTable = new Table();
            _actionsTable.DebugAll();
            _statsAndActionsWindowTable.Add(_actionsTable)
                .Grow()
                .Left();

            //attacks table
            _attacksTable = new Table();
            _actionsTable.Add(_attacksTable)
                .Grow();
            _attacksTable.Add(new Label("Attacks", _basicSkin, "default_xl"))
                .SetExpandX()
                .Left();
            _attacksTable.Row();
            for (int i = 0; i < 3; i++)
            {
                if (PlayerData.Instance.AttackActions.Count() > i)
                {
                    var attackAction = PlayerData.Instance.AttackActions[i];
                    var attackFocusLabel = new FocusLabel(PlayerActionUtils.GetName(attackAction.ToType()), _basicSkin, "default_lg");
                    attackFocusLabel.OnLabelFocused += () =>
                    {
                        UpdateInfoTable(attackAction);
                    };
                    _attackButtons.Add(attackFocusLabel);
                    _attacksTable.Add(attackFocusLabel)
                        .GrowX()
                        .SetUniformX();
                }
                else
                {
                    _attacksTable.Add().GrowX().SetUniformX();
                }
            }

            _actionsTable.Row();

            //utils table
            _utilitiesTable = new Table();
            _actionsTable.Add(_utilitiesTable)
                .Grow();
            _utilitiesTable.Add(new Label("Utilities", _basicSkin, "default_xl"))
                .SetExpandX()
                .Left();
            _utilitiesTable.Row();
            for (int i = 0; i < 3; i++)
            {
                if (PlayerData.Instance.UtilityActions.Count() > i)
                {
                    var utilityAction = PlayerData.Instance.UtilityActions[i];
                    var utilityListButton = new FocusLabel(PlayerActionUtils.GetName(utilityAction.ToType()), _basicSkin, "default_lg");
                    utilityListButton.OnLabelFocused += () =>
                    {
                        UpdateInfoTable(utilityAction);
                    };
                    _utilityButtons.Add(utilityListButton);
                    _utilitiesTable.Add(utilityListButton)
                        .GrowX()
                        .SetUniformX();
                }
                else
                {
                    _utilitiesTable.Add().GrowX().SetUniformX();
                }
            }

            _actionsTable.Row();

            //support table
            _supportTable = new Table();
            _actionsTable.Add(_supportTable)
                .Grow();
            _supportTable.Add(new Label("Support", _basicSkin, "default_xl"))
                .SetExpandX()
                .Left();
            _supportTable.Row();
            for (int i = 0; i < 3; i++)
            {
                if (PlayerData.Instance.SupportActions.Count() > i)
                {
                    var supportAction = PlayerData.Instance.SupportActions[i];
                    var supportListButton = new FocusLabel(PlayerActionUtils.GetName(supportAction.ToType()), _basicSkin, "default_lg");
                    supportListButton.OnLabelFocused += () =>
                    {
                        UpdateInfoTable(supportAction);
                    };
                    _supportButtons.Add(supportListButton);
                    _supportTable.Add(supportListButton)
                        .GrowX()
                        .SetUniformX();
                }
                else
                {
                    _supportTable.Add().GrowX().SetUniformX();
                }
            }

            _mainTable.Row();

            //info window table
            _infoTable = new WindowTable(_basicSkin);
            _infoTable.Pad(50f);
            _mainTable.Add(_infoTable)
                .Expand(1, 2)
                .Top().Left()
                .SetUniformX()
                .SetPadLeft(Game1.UIResolution.Y * .05f)
                .SetPadBottom(Game1.UIResolution.Y * .05f)
                .SetFillY()
                .Width(Game1.UIResolution.X * .4f);

            var infoInnerTable = new Table();
            infoInnerTable.DebugAll();
            _infoTable.Add(infoInnerTable).Top().Left().Grow();

            _infoTableNameLabel = new Label("", _basicSkin, "default_xxl");
            infoInnerTable.Add(_infoTableNameLabel).GrowX().Left();
            infoInnerTable.Row();
            _infoTableTypeLabel = new Label("", _basicSkin, "default_lg");
            infoInnerTable.Add(_infoTableTypeLabel).GrowX().Left();
            infoInnerTable.Row();
            _infoTableCostLabel = new Label("", _basicSkin, "default_lg");
            infoInnerTable.Add(_infoTableCostLabel).GrowX().Left();
            infoInnerTable.Row();
            _infoTableDescriptionLabel = new Label("", _basicSkin, "default_md");
            _infoTableDescriptionLabel.SetWrap(true);
            infoInnerTable.Add(_infoTableDescriptionLabel).GrowX().Left();
            infoInnerTable.Row();
        }

        void UpdateInfoTable(PlayerActionType actionType)
        {
            _infoTableNameLabel.SetText(PlayerActionUtils.GetName(actionType.ToType()));
            _infoTableTypeLabel.SetText(PlayerActionUtils.GetCategory(actionType.ToType()).ToString());
            _infoTableCostLabel.SetText($"AP Cost: {PlayerActionUtils.GetApCost(actionType.ToType())}");
            _infoTableDescriptionLabel.SetText(PlayerActionUtils.GetDescription(actionType.ToType()));
        }
    }
}
