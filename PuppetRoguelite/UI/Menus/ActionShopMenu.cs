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
    public class ActionShopMenu : CustomCanvas
    {
        Stack<Table> _tableStack = new Stack<Table>();

        Table _table;

        //main dialog
        Table _mainTable;
        List<ListButton> _attackButtons = new List<ListButton>();
        List<ListButton> _utilityButtons = new List<ListButton>();
        List<ListButton> _supportButtons = new List<ListButton>();

        //action selection dialog
        Table _actionTable;
        List<ListButton> _newActionButtons = new List<ListButton>();

        //misc
        Skin _basicSkin;
        IGamepadFocusable _previouslyFocusedButton;
        PlayerActionType _actionToRemove;

        bool _canActivateButton = true;

        System.Action _closedCallback;

        public ActionShopMenu(System.Action closedCallback)
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
            Stage.KeyboardActionKey = Keys.E;

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //arrange elements
            CreateMainDialog();
            CreateActionDialog();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            _table.Add(_mainTable);
            
            _canActivateButton = false;

            //set focus
            Stage.SetGamepadFocusElement(_attackButtons.First());
        }

        void CreateMainDialog()
        {
            //create dialog
            _mainTable = new Table();
            _mainTable.Pad(50);
            _mainTable.SetBackground(_basicSkin.GetNinePatchDrawable("np_inventory_01"));
            //_table.Add(_mainTable).Expand();

            UpdateMainDialog();
        }

        void UpdateMainDialog()
        {
            _mainTable.Clear();
            _attackButtons.Clear();
            _utilityButtons.Clear();
            _supportButtons.Clear();
            _previouslyFocusedButton = null;

            AddActionCategoryColumn(_basicSkin.GetDrawable("Style 4 Icon 005"), PlayerData.Instance.AttackActions,
                PlayerUpgradeData.Instance.AttackSlotsUpgrade.GetCurrentValue(), _attackButtons, PlayerActionCategory.Attack);

            AddActionCategoryColumn(_basicSkin.GetDrawable("Style 4 Icon 289"), PlayerData.Instance.UtilityActions,
                PlayerUpgradeData.Instance.UtilitySlotsUpgrade.GetCurrentValue(), _utilityButtons, PlayerActionCategory.Utility);

            AddActionCategoryColumn(_basicSkin.GetDrawable("Style 4 Icon 155"), PlayerData.Instance.SupportActions,
                PlayerUpgradeData.Instance.SupportSlotsUpgrade.GetCurrentValue(), _supportButtons, PlayerActionCategory.Support);
        }

        void CreateActionDialog()
        {
            //create dialog
            _actionTable = new Table();
            _actionTable.SetBackground(_basicSkin.GetNinePatchDrawable("np_inventory_01"));
            //_table.Add(_actionTable).Expand();
            _actionTable.Pad(Game1.UIResolution.X * .05f);
        }

        void UpdateActionDialog(PlayerActionCategory actionCategory, PlayerActionType actionType)
        {
            _newActionButtons.Clear();

            _actionTable.Clear();

            var unlocks = ActionUnlockData.Instance.Unlocks.Where(u => PlayerActionUtils.GetCategory(u.Action.ToType()) == actionCategory);
            foreach (var unlock in unlocks)
            {
                var button = new ListButton($"* {PlayerActionUtils.GetName(unlock.Action.ToType())}", _basicSkin, "listButton_xl");
                button.SetDisabled(!unlock.IsUnlocked);
                button.OnClicked += button =>
                {
                    switch (actionCategory)
                    {
                        case PlayerActionCategory.Attack:
                            if (actionType != null)
                            {
                                PlayerData.Instance.AttackActions.Remove(actionType);
                            }
                            PlayerData.Instance.AttackActions.RemoveAll(a => a.ToType() == unlock.Action.ToType());
                            PlayerData.Instance.AttackActions.Add(unlock.Action);
                            break;
                        case PlayerActionCategory.Utility:
                            if (actionType != null)
                            {
                                PlayerData.Instance.UtilityActions.Remove(actionType);
                            }
                            PlayerData.Instance.UtilityActions.RemoveAll(a => a.ToType() == unlock.Action.ToType());
                            PlayerData.Instance.UtilityActions.Add(unlock.Action);
                            break;
                        case PlayerActionCategory.Support:
                            if (actionType != null)
                            {
                                PlayerData.Instance.SupportActions.Remove(actionType);
                            }
                            PlayerData.Instance.SupportActions.RemoveAll(a => a.ToType() == unlock.Action.ToType());
                            PlayerData.Instance.SupportActions.Add(unlock.Action);
                            break;
                    }
                    _actionTable.Remove();
                    UpdateMainDialog();
                    _table.Add(_mainTable);
                    var focus = _previouslyFocusedButton != null ? _previouslyFocusedButton : _attackButtons.First();
                    Stage.SetGamepadFocusElement(focus);
                };
                _actionTable.Add(button).Left();
                _actionTable.Row();
                _newActionButtons.Add(button);
            }
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
                Stage.SetGamepadFocusElement(null);

                if (_mainTable.HasParent())
                {
                    PlayerData.Instance.UpdateAndSave();
                    _closedCallback?.Invoke();
                }
                else
                {
                    _actionTable.Remove();
                    _table.AddElement(_mainTable);
                    var focus = _previouslyFocusedButton != null ? _previouslyFocusedButton : _attackButtons.First();
                    Stage.SetGamepadFocusElement(focus);
                }
            }
        }

        void AddActionCategoryColumn(IDrawable icon, List<PlayerActionType> actions, int maxSlots,
            List<ListButton> buttonList, PlayerActionCategory category)
        {
            var columnTable = new Table();
            _mainTable.Add(columnTable).Grow().Uniform().Space(Game1.UIResolution.X * .03f);
            var upperTable = new Table();
            columnTable.Add(upperTable).Grow();
            var iconImage = new Image(icon);
            iconImage.SetScale(Game1.ResolutionScale.X);
            upperTable.Add(iconImage).Center();

            columnTable.Row();

            var lowerTable = new Table();
            columnTable.Add(lowerTable).Grow().SetSpaceTop(Game1.UIResolution.Y * .03f);
            var listTable = new Table();
            lowerTable.Add(listTable).GrowY().Top();
            //listTable.Defaults().SetSpaceBottom(5);

            for (int i = 0; i < maxSlots; i++)
            {
                var label = "";
                PlayerActionType actionType = null;
                if (actions.Count > i)
                {
                    actionType = actions[i];
                    label = PlayerActionUtils.GetName(actionType.ToType());
                }
                var button = new ListButton($"* {label}", _basicSkin, "listButton_xl");
                button.OnClicked += button =>
                {
                    _previouslyFocusedButton = button;
                    OnMainActionButtonClicked(category, actionType);
                };
                listTable.Add(button).Left();
                listTable.Row();
                buttonList.Add(button);
            }
        }

        void OnMainActionButtonClicked(PlayerActionCategory category, PlayerActionType actionType)
        {
            if (_canActivateButton)
            {
                Stage.SetGamepadFocusElement(null);
                _mainTable.Remove();
                UpdateActionDialog(category, actionType);
                _table.Add(_actionTable);
                if (_newActionButtons.Count > 0)
                {
                    Stage.SetGamepadFocusElement(_newActionButtons.First());
                }
            }
        }
    }
}
