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
        Vector2 _offset = new Vector2(0, -16);
        Vector2 _basePosition;
        Vector2 _anchorPosition;

        //elements
        Table _actionTable;
        ActionButton _attackButton;
        ActionButton _toolButton;
        ActionButton _itemButton;
        ActionButton _executeButton;

        TurnHandler _turnHandler;

        public ActionsSelector(Vector2 basePosition, TurnHandler turnHandler)
        {
            _basePosition = basePosition;
            _turnHandler = turnHandler;
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            if (_actionTable != null)
            {
                var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition);
                _actionTable.SetPosition(pos.X - _actionTable.PreferredWidth / 2, pos.Y);
            }
        }

        public override Element ArrangeElements()
        {
            _actionTable = new Table();

            var attackLabel = new Label("Attack", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(attackLabel).Center();
            var toolLabel = new Label("Tools", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(toolLabel).Center();
            var itemLabel = new Label("Items", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(itemLabel).Center();
            var executeLabel = new Label("Execute", _defaultSkin).SetAlignment(Align.Center);
            _actionTable.Add(executeLabel).Center();

            _actionTable.Row();

            _attackButton = new ActionButton(_defaultSkin, "attackActionButton", attackLabel);
            _attackButton.OnClicked += OnAttackButtonClicked;
            _actionTable.Add(_attackButton).Center();
            _toolButton = new ActionButton(_defaultSkin, "toolActionButton", toolLabel);
            _toolButton.OnClicked += OnToolButtonClicked;
            _actionTable.Add(_toolButton).Center();
            _itemButton = new ActionButton(_defaultSkin, "itemActionButton", itemLabel);
            _itemButton.OnClicked += OnItemButtonClicked;
            _actionTable.Add(_itemButton).Center();
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
            toolLabel.SetVisible(false);
            itemLabel.SetVisible(false);
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
            _attackButton.EnableExplicitFocusableControl(null, null, null, _toolButton);
            _toolButton.EnableExplicitFocusableControl(null, null, _attackButton, _itemButton);
            _itemButton.EnableExplicitFocusableControl(null, null, _toolButton, _executeButton.GetDisabled() ? null : _executeButton);
            _executeButton.EnableExplicitFocusableControl(null, null, _itemButton, null);
        }

        public override void DetermineDefaultElement()
        {
            Stage.SetGamepadFocusElement(_attackButton);
        }

        public override void DeterminePosition()
        {
            _anchorPosition = _basePosition + _offset - new Vector2(0, _actionTable.PreferredHeight / 2);
        }

        void OnAttackButtonClicked(Button obj)
        {
            _turnHandler.OpenMenu(new AttacksMenu(_basePosition, _turnHandler));
        }

        void OnToolButtonClicked(Button obj)
        {
            throw new NotImplementedException();
        }

        void OnItemButtonClicked(Button obj)
        {

        }

        void OnExecuteButtonClicked(Button obj)
        {
            _turnHandler.ExecuteActions();
        }
    }
}
