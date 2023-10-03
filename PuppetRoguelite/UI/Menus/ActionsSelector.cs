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
        Skin _basicSkin;
        Vector2 _anchorPosition;

        //elements
        Table _actionTable;
        ActionButton _attackButton;
        ActionButton _toolButton;
        ActionButton _itemButton;
        ActionButton _executeButton;

        Vector2 _offset = new Vector2(0, -16);

        TurnHandler _turnHandler;
        AttacksMenu _attacksMenu;

        public ActionsSelector(Vector2 anchorPosition, TurnHandler turnHandler, AttacksMenu attacksMenu,
            bool shouldMaintainFocusedElement = false) : base(shouldMaintainFocusedElement)
        {
            _anchorPosition = anchorPosition;
            _turnHandler = turnHandler;
            _attacksMenu = attacksMenu;
        }

        public override void Initialize()
        {
            base.Initialize();

            _basicSkin = CustomSkins.CreateBasicSkin();
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            ArrangeElements();
            ValidateButtons();
            DefaultElement = _attackButton;
            Stage.SetGamepadFocusElement(_attackButton);

            SetEnabled(false);
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            if (_actionTable != null)
            {
                var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition + _offset - new Vector2(0, _actionTable.PreferredHeight / 2));
                _actionTable.SetPosition(pos.X - _actionTable.PreferredWidth / 2, pos.Y);
            }
        }

        public override void OnEnabled()
        {
            ValidateButtons();
            DefaultElement = _attackButton;

            base.OnEnabled();
        }

        void ArrangeElements()
        {
            _actionTable = new Table();

            var attackLabel = new Label("Attack", _basicSkin).SetAlignment(Align.Center);
            _actionTable.Add(attackLabel).Center();
            var toolLabel = new Label("Tools", _basicSkin).SetAlignment(Align.Center);
            _actionTable.Add(toolLabel).Center();
            var itemLabel = new Label("Items", _basicSkin).SetAlignment(Align.Center);
            _actionTable.Add(itemLabel).Center();
            var executeLabel = new Label("Execute", _basicSkin).SetAlignment(Align.Center);
            _actionTable.Add(executeLabel).Center();

            _actionTable.Row();

            _attackButton = new ActionButton(this, _basicSkin, "attackActionButton", attackLabel);
            _attackButton.OnClicked += OnAttackButtonClicked;
            _actionTable.Add(_attackButton).Center();
            _toolButton = new ActionButton(this, _basicSkin, "toolActionButton", toolLabel);
            _toolButton.OnClicked += OnToolButtonClicked;
            _actionTable.Add(_toolButton).Center();
            _itemButton = new ActionButton(this, _basicSkin, "itemActionButton", itemLabel);
            _itemButton.OnClicked += OnItemButtonClicked;
            _actionTable.Add(_itemButton).Center();
            _executeButton = new ActionButton(this, _basicSkin, "executeButton", executeLabel);
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
        }

        void ValidateButtons()
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

        void OnAttackButtonClicked(Button obj)
        {
            _turnHandler.OpenMenu(_attacksMenu);
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
