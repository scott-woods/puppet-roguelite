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
        List<FocusLabel> _attackLabels = new List<FocusLabel>();
        List<FocusLabel> _utilityLabels = new List<FocusLabel>();
        List<FocusLabel> _supportLabels = new List<FocusLabel>();
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

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Open_stats_menu);
            Stage.SetGamepadFocusElement(_attackLabels.First());
        }

        public override void Update()
        {
            base.Update();

            if (Controls.Instance.ShowStats.IsPressed)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Close_stats_menu);
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
            _statsAndActionsWindowTable.Add(_statsTable)
                .GrowX()
                .Top().Left();

            //hp label
            var hpLabel = new Label($"HP: {PlayerController.Instance.HealthComponent.Health}/{PlayerController.Instance.HealthComponent.MaxHealth}", _basicSkin, "default_lg");
            hpLabel.SetAlignment(Align.Left);
            _statsTable.Add(hpLabel).Expand().Left();

            _statsTable.Row();

            //ap label
            var apLabel = new Label($"Max AP: {PlayerUpgradeData.Instance.MaxApUpgrade.GetCurrentValue()}", _basicSkin, "default_lg");
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
                    attackFocusLabel.SetWrap(true);
                    attackFocusLabel.OnLabelFocused += () =>
                    {
                        UpdateInfoTable(attackAction);
                    };
                    _attackLabels.Add(attackFocusLabel);
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
                    var utilityFocusLabel = new FocusLabel(PlayerActionUtils.GetName(utilityAction.ToType()), _basicSkin, "default_lg");
                    utilityFocusLabel.SetWrap(true);
                    utilityFocusLabel.OnLabelFocused += () =>
                    {
                        UpdateInfoTable(utilityAction);
                    };
                    _utilityLabels.Add(utilityFocusLabel);
                    _utilitiesTable.Add(utilityFocusLabel)
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
                    var supportFocusLabel = new FocusLabel(PlayerActionUtils.GetName(supportAction.ToType()), _basicSkin, "default_lg");
                    supportFocusLabel.SetWrap(true);
                    supportFocusLabel.OnLabelFocused += () =>
                    {
                        UpdateInfoTable(supportAction);
                    };
                    _supportLabels.Add(supportFocusLabel);
                    _supportTable.Add(supportFocusLabel)
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
                .SetFillY()
                .Width(Game1.UIResolution.X * .4f);

            var infoUpperTable = new Table();
            _infoTable.Add(infoUpperTable)
                .GrowX()
                .Top().Left();
            _infoTableNameLabel = new Label("", _basicSkin, "default_xxl");
            infoUpperTable.Add(_infoTableNameLabel).GrowX().Left();
            infoUpperTable.Row();
            _infoTableTypeLabel = new Label("", _basicSkin, "default_lg");
            infoUpperTable.Add(_infoTableTypeLabel).GrowX().Left();
            infoUpperTable.Row();
            _infoTableCostLabel = new Label("", _basicSkin, "default_lg");
            infoUpperTable.Add(_infoTableCostLabel).GrowX().Left();

            _infoTable.Row();

            _infoTableDescriptionLabel = new Label("", _basicSkin, "default_lg");
            _infoTableDescriptionLabel.SetWrap(true).SetAlignment(Align.Center);
            _infoTable.Add(_infoTableDescriptionLabel)
                .Grow();

            _mainTable.Row();

            _mainTable.Add(new Label("Tab: Close", _basicSkin, "default_lg"))
                .Top().Left()
                .SetUniformX()
                .SetPadTop(10)
                .SetPadLeft((Game1.UIResolution.Y * .05f) + 10)
                .SetPadBottom(Game1.UIResolution.Y * .05f)
                .Width(Game1.UIResolution.X * .4f);
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
