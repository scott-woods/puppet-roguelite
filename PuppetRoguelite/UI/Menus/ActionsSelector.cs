using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace PuppetRoguelite.UI.Menus
{
    public class ActionsSelector : UIMenu
    {
        Vector2 _worldSpaceOffset = new Vector2(0, -16);
        Vector2 _basePosition;

        //elements
        Table _actionTable;
        ActionButton _attackButton;
        ActionButton _utilitiesButton;
        ActionButton _supportButton;
        ActionButton _executeButton;

        TurnHandler _turnHandler;

        public ActionsSelector(Vector2 basePosition, TurnHandler turnHandler)
        {
            _basePosition = basePosition;
            _turnHandler = turnHandler;
        }

        public override Element ArrangeElements()
        {
            _actionTable = new Table();

            var attackLabel = new Label("Attack", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(attackLabel).Center();
            var utilityLabel = new Label("Utility", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(utilityLabel).Center();
            var supportLabel = new Label("Support", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(supportLabel).Center();
            var executeLabel = new Label("Execute", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(executeLabel).Center();

            _actionTable.Row();

            _attackButton = new ActionButton(_defaultSkin, "attackActionButton", attackLabel);
            _attackButton.OnClicked += OnAttackButtonClicked;
            _actionTable.Add(_attackButton).Center();
            _utilitiesButton = new ActionButton(_defaultSkin, "toolActionButton", utilityLabel);
            _utilitiesButton.OnClicked += OnUtilityButtonClicked;
            _actionTable.Add(_utilitiesButton).Center();
            _supportButton = new ActionButton(_defaultSkin, "itemActionButton", supportLabel);
            _supportButton.OnClicked += OnSupportButtonClicked;
            _actionTable.Add(_supportButton).Center();
            _executeButton = new ActionButton(_defaultSkin, "executeButton", executeLabel);
            _executeButton.OnClicked += OnExecuteButtonClicked;
            _actionTable.Add(_executeButton).Center();

            float maxWidth = 0;
            foreach(var cell in _actionTable.GetCells())
            {
                maxWidth = Math.Max(cell.GetPrefWidth(), maxWidth);
            }

            for (int i = 0; i < _actionTable.GetColumns(); i++)
            {
                var cell = _actionTable.GetCells()[i];
                cell.Width(maxWidth);
            }

            _actionTable.Pack();

            attackLabel.SetVisible(false);
            utilityLabel.SetVisible(false);
            supportLabel.SetVisible(false);
            executeLabel.SetVisible(false);

            Stage.AddElement(_actionTable);
            return _actionTable;
        }

        public override void ValidateButtons()
        {
            if (_turnHandler.ActionQueue.Count == 0) //disable if no queued actions
            {
                _executeButton.SetDisabled(true);
                _executeButton.SetColor(Color.Gray);
            }
            else
            {
                _executeButton.SetDisabled(false);
                _executeButton.SetColor(Color.White);
            }

            //setup explicit focus control
            //_attackButton.EnableExplicitFocusableControl(null, null, null, _toolButton);
            //_toolButton.EnableExplicitFocusableControl(null, null, _attackButton, _itemButton);
            //_itemButton.EnableExplicitFocusableControl(null, null, _toolButton, _executeButton.GetDisabled() ? null : _executeButton);
            //_executeButton.EnableExplicitFocusableControl(null, null, _itemButton, null);
        }

        public override void DetermineDefaultElement()
        {
            Stage.SetGamepadFocusElement(_attackButton);
        }

        public override void DeterminePosition()
        {
            _anchorPosition = _basePosition + _worldSpaceOffset;
            _screenSpaceOffset = new Vector2(-_actionTable.PreferredWidth / 2, -_actionTable.PreferredHeight);
        }

        void OnAttackButtonClicked(Button obj)
        {
            _turnHandler.OpenMenu(new AttacksMenu(_basePosition, _turnHandler));
        }

        void OnUtilityButtonClicked(Button obj)
        {
            _turnHandler.OpenMenu(new UtilityMenu(_basePosition, _turnHandler));
        }

        void OnSupportButtonClicked(Button obj)
        {
            _turnHandler.OpenMenu(new SupportMenu(_basePosition, _turnHandler));
        }

        void OnExecuteButtonClicked(Button obj)
        {
            _turnHandler.ExecuteActions();
        }
    }
}
