using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.GOAP;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Unlocks;
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
    public class ActionShopMenu : CustomCanvas
    {
        //elements
        Table _table;
        Table _currentActionsInnerTable;
        Table _availableActionsInnerTable;
        WindowTable _primaryWindow;
        Table _currentAttacksCol;
        Table _currentUtilsCol;
        Table _currentSupportCol;
        WindowTable _infoWindow;
        Label _infoActionName;
        Label _infoActionCost;
        Label _infoActionDescription;
        List<BulletPointSelector<PlayerActionType>> _currentActionButtons = new List<BulletPointSelector<PlayerActionType>>();

        //skin
        Skin _basicSkin;

        //misc
        bool _canActivateButton;
        bool _isSelectingNewAction = false;
        int _lastSelectedIndex = 0;
        PlayerActionType _selectedActionType;

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
            Stage.KeyboardActionKey = Keys.None;

            //render layer
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //arrange elements
            ArrangeElements();
            //CreateMainDialog();
            //CreateActionDialog();

            //display current actions
            DisplayCurrentActions();
        }

        void ArrangeElements()
        {
            //create primary window
            _primaryWindow = new WindowTable(_basicSkin);
            _primaryWindow.Pad(50f);
            _table.Add(_primaryWindow)
                .Width(Value.PercentWidth(.6f, _table))
                .Height(Value.PercentHeight(.6f, _table))
                .SetUniformX();

            //pack primary window
            _primaryWindow.Pack();

            _table.Row();

            //info window
            _infoWindow = new WindowTable(_basicSkin);
            _infoWindow.Pad(50f);
            _table.Add(_infoWindow)
                .Width(Value.PercentWidth(1f, _primaryWindow))
                .Height(Value.PercentHeight(.3f, _table))
                .SetUniformX();

            //info table upper section
            var upperInfoTable = new Table();
            _infoWindow.Add(upperInfoTable).Expand().Top().Left().SetSpaceBottom(25f);

            //info table action name
            _infoActionName = new Label("", _basicSkin, "default_xl");
            upperInfoTable.Add(_infoActionName).Left();
            upperInfoTable.Row();

            //info table action cost
            _infoActionCost = new Label("", _basicSkin, "default_lg");
            upperInfoTable.Add(_infoActionCost).Left();
            upperInfoTable.Row();

            //info table lower section
            _infoWindow.Row();
            var lowerInfoTable = new Table();
            _infoWindow.Add(lowerInfoTable).Grow();

            //info table action description
            _infoActionDescription = new Label("", _basicSkin, "default_lg");
            _infoActionDescription.SetAlignment(Align.Center);
            _infoActionDescription.SetWrap(true);
            lowerInfoTable.Add(_infoActionDescription).Grow();

            _table.Row();

            _primaryWindow.Pack();
            _infoWindow.Pack();

            //tips
            var tipTable = new Table();
            _table.Add(tipTable).SetUniformX().Left();
            var backLabel = new Label("X: Go Back", _basicSkin, "default_lg");
            tipTable.Add(backLabel).SetPadLeft(10);
        }

        void DisplayCurrentActions()
        {
            //update state
            _isSelectingNewAction = false;
            _selectedActionType = null;

            //clear window
            _primaryWindow.Clear();

            //clear action buttons
            _currentActionButtons.Clear();

            _currentActionsInnerTable = new Table();
            _primaryWindow.Add(_currentActionsInnerTable).Grow();

            //header label
            var actionsHeader = new Label("Actions", _basicSkin, "default_xxxl");
            _currentActionsInnerTable.Add(actionsHeader).Top().SetColspan(3).SetSpaceBottom(25f);

            _currentActionsInnerTable.Row();

            //lower section table
            var lowerTable = new Table();
            lowerTable.Defaults().Space(50f).Grow();
            _currentActionsInnerTable.Add(lowerTable).Grow();

            //current attacks column
            _currentAttacksCol = new Table();
            lowerTable.Add(_currentAttacksCol).Uniform();
            var attackIcon = _basicSkin.GetDrawable("Style 4 Icon 005");
            var attackIconImage = new Image(attackIcon);
            attackIconImage.SetScale(Game1.ResolutionScale.X);
            _currentAttacksCol.Add(attackIconImage).SetSpaceBottom(25f);
            _currentAttacksCol.Row();
            var attackButtonsTable = new Table();
            _currentAttacksCol.Add(attackButtonsTable).Grow().SetMaxWidth(Value.PercentWidth(1f, _currentAttacksCol));
            for (int i = 0; i < PlayerUpgradeData.Instance.AttackSlotsUpgrade.Levels.Count; i++)
            {
                string name;
                PlayerActionType actionType = null;
                if (PlayerData.Instance.AttackActions.Count >= i + 1)
                {
                    actionType = PlayerData.Instance.AttackActions[i];
                    name = PlayerActionUtils.GetName(actionType.ToType());
                }
                else
                {
                    name = "------";
                    actionType = null;
                }

                var button = new BulletPointSelector<PlayerActionType>(name, actionType, true, _basicSkin, "default_xl");
                button.OnPointFocused += () =>
                {
                    UpdateInfoTable(actionType);
                };
                button.OnSelected += (actionType) =>
                {
                    OnCurrentActionSelected(actionType, PlayerActionCategory.Attack);
                };
                attackButtonsTable.Add(button).Expand().SetFillX();
                attackButtonsTable.Row();
                _currentActionButtons.Add(button);

                if (PlayerUpgradeData.Instance.AttackSlotsUpgrade.CurrentLevel < i + 1)
                {
                    button.SetVisible(false);
                    button.SetDisabled(true);
                }
            }

            //current utils column
            _currentUtilsCol = new Table();
            lowerTable.Add(_currentUtilsCol).Uniform();
            var utilsIcon = _basicSkin.GetDrawable("Style 4 Icon 289");
            var utilsIconImage = new Image(utilsIcon);
            utilsIconImage.SetScale(Game1.ResolutionScale.X);
            _currentUtilsCol.Add(utilsIconImage).SetSpaceBottom(25f);
            _currentUtilsCol.Row();
            var utilButtonsTable = new Table();
            _currentUtilsCol.Add(utilButtonsTable).Grow().SetMaxWidth(Value.PercentWidth(1f, _currentUtilsCol));
            for (int i = 0; i < PlayerUpgradeData.Instance.UtilitySlotsUpgrade.Levels.Count; i++)
            {
                string name;
                PlayerActionType actionType = null;
                if (PlayerData.Instance.UtilityActions.Count >= i + 1)
                {
                    actionType = PlayerData.Instance.UtilityActions[i];
                    name = PlayerActionUtils.GetName(actionType.ToType());
                }
                else
                {
                    name = "------";
                    actionType = null;
                }

                var button = new BulletPointSelector<PlayerActionType>(name, actionType, true, _basicSkin, "default_xl");
                button.OnPointFocused += () =>
                {
                    UpdateInfoTable(actionType);
                };
                button.OnSelected += (actionType) =>
                {
                    OnCurrentActionSelected(actionType, PlayerActionCategory.Utility);
                };
                utilButtonsTable.Add(button).Expand().SetFillX();
                utilButtonsTable.Row();
                _currentActionButtons.Add(button);

                if (PlayerUpgradeData.Instance.UtilitySlotsUpgrade.CurrentLevel < i + 1)
                {
                    button.SetVisible(false);
                    button.SetDisabled(true);
                }
            }

            //current support column
            _currentSupportCol = new Table();
            lowerTable.Add(_currentSupportCol).Uniform();
            var supportIcon = _basicSkin.GetDrawable("Style 4 Icon 155");
            var supportIconImage = new Image(supportIcon);
            supportIconImage.SetScale(Game1.ResolutionScale.X);
            _currentSupportCol.Add(supportIconImage).SetSpaceBottom(25f);
            _currentSupportCol.Row();
            var supportButtonsTable = new Table();
            _currentSupportCol.Add(supportButtonsTable).Grow().SetMaxWidth(Value.PercentWidth(1f, _currentSupportCol));
            for (int i = 0; i < PlayerUpgradeData.Instance.SupportSlotsUpgrade.Levels.Count; i++)
            {
                string name;
                PlayerActionType actionType = null;
                if (PlayerData.Instance.SupportActions.Count >= i + 1)
                {
                    actionType = PlayerData.Instance.SupportActions[i];
                    name = PlayerActionUtils.GetName(actionType.ToType());
                }
                else
                {
                    name = "------";
                    actionType = null;
                }

                var button = new BulletPointSelector<PlayerActionType>(name, actionType, true, _basicSkin, "default_xl");
                button.OnPointFocused += () =>
                {
                    UpdateInfoTable(actionType);
                };
                button.OnSelected += (actionType) =>
                {
                    OnCurrentActionSelected(actionType, PlayerActionCategory.Support);
                };
                supportButtonsTable.Add(button).Expand().SetFillX();
                supportButtonsTable.Row();
                _currentActionButtons.Add(button);

                if (PlayerUpgradeData.Instance.SupportSlotsUpgrade.CurrentLevel < i + 1)
                {
                    button.SetVisible(false);
                    button.SetDisabled(true);
                }
            }

            //add selection handlers to each button
            for (int i = 0; i < _currentActionButtons.Count; i++)
            {
                var button = _currentActionButtons[i];
                var index = i;
                button.OnSelected += (actionType) =>
                {
                    _lastSelectedIndex = index;
                };
            }

            //set focus element
            Stage.SetGamepadFocusElement(_currentActionButtons[_lastSelectedIndex]);
        }

        void DisplayAvailableActions(PlayerActionCategory category)
        {
            //update state
            _isSelectingNewAction = true;

            //clear window
            _primaryWindow.Clear();

            //new inner table
            _availableActionsInnerTable = new Table();
            _primaryWindow.Add(_availableActionsInnerTable).Grow();

            //header label
            var actionsHeader = new Label($"Available Actions", _basicSkin, "default_xxxl");
            _availableActionsInnerTable.Add(actionsHeader).Top().SetSpaceBottom(25f);

            _availableActionsInnerTable.Row();

            //lower section table
            var lowerTable = new Table();
            lowerTable.Defaults().Space(50f);
            _availableActionsInnerTable.Add(lowerTable).Grow();

            //determine icon by category
            IDrawable icon = null;
            switch (category)
            {
                case PlayerActionCategory.Attack:
                    icon = _basicSkin.GetDrawable("Style 4 Icon 005");
                    break;
                case PlayerActionCategory.Utility:
                    icon = _basicSkin.GetDrawable("Style 4 Icon 289");
                    break;
                case PlayerActionCategory.Support:
                    icon = _basicSkin.GetDrawable("Style 4 Icon 155");
                    break;
            }

            //create icon image
            var iconImage = new Image(icon);
            iconImage.SetScale(Game1.ResolutionScale.X);
            lowerTable.Add(iconImage).SetSpaceBottom(25f);

            lowerTable.Row();

            //buttons list table
            var buttonsTable = new Table();
            lowerTable.Add(buttonsTable).Grow();

            //get unlocks for this category
            var unlocks = ActionUnlockData.Instance.Unlocks
                .Where(u => PlayerActionUtils.GetCategory(u.Action.ToType()) == category).ToList();

            //add button for each unlock
            var buttons = new List<BulletPointSelector<PlayerActionType>>();
            foreach (var unlock in unlocks)
            {
                var action = unlock.Action;
                var name = PlayerActionUtils.GetName(action.ToType());
                var button = new BulletPointSelector<PlayerActionType>(name, action, true, _basicSkin, "default_xl");
                button.OnPointFocused += () =>
                {
                    UpdateInfoTable(action);
                };
                button.OnSelected += OnNewActionSelected;
                buttonsTable.Add(button).Grow();
                buttonsTable.Row();

                if (!unlock.IsUnlocked)
                    button.SetDisabled(true);
                else
                    buttons.Add(button);
            }

            //add an empty button at the bottom if applicable
            int categoryCount = 0;
            switch (category)
            {
                case PlayerActionCategory.Attack:
                    categoryCount = PlayerData.Instance.AttackActions.Count; break;
                case PlayerActionCategory.Utility:
                    categoryCount = PlayerData.Instance.UtilityActions.Count; break;
                case PlayerActionCategory.Support:
                    categoryCount = PlayerData.Instance.SupportActions.Count; break;
            }
            if (categoryCount > 1 && _selectedActionType != null)
            {
                var emptyButton = new BulletPointSelector<PlayerActionType>("--Remove Action--", null, true, _basicSkin, "default_xl");
                emptyButton.OnPointFocused += () => UpdateInfoTable(null);
                emptyButton.OnSelected += OnNewActionSelected;
                buttonsTable.Add(emptyButton).Grow();
            }

            Stage.SetGamepadFocusElement(buttons.First());
        }

        public override void Update()
        {
            base.Update();

            //wait until action key is released before allowing it to be pressed
            if (!_canActivateButton && !Input.IsKeyDown(Keys.E))
            {
                _canActivateButton = true;
                Stage.KeyboardActionKey = Keys.E;
            }

            if (Input.IsKeyPressed(Keys.X))
            {
                if (_isSelectingNewAction)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    DisplayCurrentActions();
                }
                else
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                    _closedCallback?.Invoke();
                }
            }
        }

        void UpdateInfoTable(PlayerActionType action)
        {
            if (action != null)
            {
                _infoActionName.SetText(PlayerActionUtils.GetName(action.ToType()));
                _infoActionCost.SetText($"AP Cost: {PlayerActionUtils.GetApCost(action.ToType())}");
                _infoActionDescription.SetText(PlayerActionUtils.GetDescription(action.ToType()));
            }
            else
            {
                _infoActionName.SetText("");
                _infoActionCost.SetText("");
                _infoActionDescription.SetText("");
            }
        }

        void OnCurrentActionSelected(PlayerActionType action, PlayerActionCategory category)
        {
            _selectedActionType = action;

            //clear primary table
            _primaryWindow.ClearChildren();
            _primaryWindow.Clear();

            DisplayAvailableActions(category);
        }

        void OnNewActionSelected(PlayerActionType action)
        {
            if (action != null)
            {
                var category = PlayerActionUtils.GetCategory(action.ToType());
                switch (category)
                {
                    case PlayerActionCategory.Attack:
                        if (_selectedActionType != null)
                        {
                            PlayerData.Instance.AttackActions.RemoveAll(a => a.ToType() == _selectedActionType.ToType());
                        }
                        PlayerData.Instance.AttackActions.RemoveAll(a => a.ToType() == action.ToType());
                        PlayerData.Instance.AttackActions.Add(action);
                        break;
                    case PlayerActionCategory.Utility:
                        if (_selectedActionType != null)
                        {
                            PlayerData.Instance.AttackActions.RemoveAll(a => a.ToType() == _selectedActionType.ToType());
                        }
                        PlayerData.Instance.UtilityActions.RemoveAll(a => a.ToType() == action.ToType());
                        PlayerData.Instance.UtilityActions.Add(action);
                        break;
                    case PlayerActionCategory.Support:
                        if (_selectedActionType != null)
                        {
                            PlayerData.Instance.AttackActions.RemoveAll(a => a.ToType() == _selectedActionType.ToType());
                        }
                        PlayerData.Instance.SupportActions.RemoveAll(a => a.ToType() == action.ToType());
                        PlayerData.Instance.SupportActions.Add(action);
                        break;
                }
            }
            else //if null, remove action was selected
            {
                if (_selectedActionType != null)
                {
                    var category = PlayerActionUtils.GetCategory(_selectedActionType.ToType());
                    switch (category)
                    {
                        case PlayerActionCategory.Attack:
                            PlayerData.Instance.AttackActions.RemoveAll(a => a.ToType() == _selectedActionType.ToType());
                            break;
                        case PlayerActionCategory.Utility:
                            PlayerData.Instance.UtilityActions.RemoveAll(a => a.ToType() == _selectedActionType.ToType());
                            break;
                        case PlayerActionCategory.Support:
                            PlayerData.Instance.SupportActions.RemoveAll(a => a.ToType() == _selectedActionType.ToType());
                            break;
                    }
                }
            }

            DisplayCurrentActions();
        }
    }
}
