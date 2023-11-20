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
        Stack<Table> _tableStack = new Stack<Table>();

        //main dialog
        Table _mainTable;
        Dialog _mainDialog;
        List<ListButton> _attackButtons = new List<ListButton>();
        List<ListButton> _utilityButtons = new List<ListButton>();
        List<ListButton> _supportButtons = new List<ListButton>();

        //action selection dialog
        Table _actionTable;
        Dialog _actionDialog;
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
            //_mainTable = Stage.AddElement(new Table());
            //_mainTable.SetFillParent(true);

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

            Stage.AddElement(_mainTable);
            
            _canActivateButton = false;

            //set focus
            Stage.SetGamepadFocusElement(_attackButtons.First());
        }

        void CreateMainDialog()
        {
            _mainTable = Stage.AddElement(new Table());
            _mainTable.SetWidth(Game1.UIResolution.X);
            _mainTable.SetHeight(Game1.UIResolution.Y);

            //create dialog
            _mainDialog = new Dialog("", _basicSkin);
            var hPad = Game1.UIResolution.X * .4f;
            var vPad = Game1.UIResolution.Y * .4f;
            _mainTable.Add(_mainDialog).Grow().SetPadLeft(hPad / 2).SetPadRight(hPad / 2).SetPadTop(vPad / 2).SetPadBottom(vPad / 2);

            //get internal table of dialog
            var contentTable = _mainDialog.GetContentTable();

            //set padding of dialog
            contentTable.PadTop(10).PadBottom(10).PadLeft(20).PadRight(20);
            contentTable.Defaults().SetSpaceBottom(0).SetSpaceTop(0);

            UpdateMainDialog();
        }

        void UpdateMainDialog()
        {
            var contentTable = _mainDialog.GetContentTable();
            contentTable.Clear();
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
            _actionTable = new Table();
            _actionTable.SetFillParent(true);

            _actionDialog = new Dialog("", _basicSkin);
            _actionTable.Add(_actionDialog);

            var contentTable = _actionDialog.GetContentTable();
            contentTable.Pad(50);
        }

        void UpdateActionDialog(PlayerActionCategory actionCategory, PlayerActionType actionType)
        {
            _newActionButtons.Clear();

            var contentTable = _actionDialog.GetContentTable();
            contentTable.Clear();

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
                    Stage.AddElement(_mainTable);
                    var focus = _previouslyFocusedButton != null ? _previouslyFocusedButton : _attackButtons.First();
                    Stage.SetGamepadFocusElement(focus);
                };
                contentTable.Add(button).Left();
                contentTable.Row();
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
                    Stage.AddElement(_mainTable);
                    var focus = _previouslyFocusedButton != null ? _previouslyFocusedButton : _attackButtons.First();
                    Stage.SetGamepadFocusElement(focus);
                }
            }
        }

        void AddActionCategoryColumn(IDrawable icon, List<PlayerActionType> actions, int maxSlots,
            List<ListButton> buttonList, PlayerActionCategory category)
        {
            var contentTable = _mainDialog.GetContentTable();

            var columnTable = new Table();
            contentTable.Add(columnTable).Grow();
            var upperTable = new Table();
            columnTable.Add(upperTable).Grow();
            var iconImage = new Image(icon);
            upperTable.Add(iconImage).Grow().Pad(50).Center();

            columnTable.Row();

            var lowerTable = new Table();
            columnTable.Add(lowerTable).Grow();
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
                Stage.AddElement(_actionTable);
                if (_newActionButtons.Count > 0)
                {
                    Stage.SetGamepadFocusElement(_newActionButtons.First());
                }
            }
        }
    }
}
