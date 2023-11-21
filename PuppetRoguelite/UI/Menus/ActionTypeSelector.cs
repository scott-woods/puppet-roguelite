using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
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
    public class ActionTypeSelector : UIMenu
    {
        //elements
        Table _actionTable;
        ActionButton _attackButton;
        ActionButton _utilitiesButton;
        ActionButton _supportButton;
        ActionButton _executeButton;

        //misc
        Action<Button> _selectionHandler;

        public ActionTypeSelector(Action<Button> selectionHandler, Action cancelHandler) : base(cancelHandler)
        {
            _selectionHandler = selectionHandler;
        }

        public override void Initialize()
        {
            base.Initialize();

            WorldSpaceOffset = new Vector2(0, -16);

            Stage.IsFullScreen = true;
        }

        public override Element ArrangeElements()
        {
            var skin = CustomSkins.CreateBasicSkin();

            _actionTable = new Table();
            _actionTable.Defaults().Space(5);

            var attackLabel = new Label("Attack", skin, "default_lg").SetAlignment(Align.Center);
            _actionTable.Add(attackLabel).Center();
            var utilityLabel = new Label("Utility", skin, "default_lg").SetAlignment(Align.Center);
            _actionTable.Add(utilityLabel).Center();
            var supportLabel = new Label("Support", skin, "default_lg").SetAlignment(Align.Center);
            _actionTable.Add(supportLabel).Center();
            var executeLabel = new Label("Execute", skin, "default_lg").SetAlignment(Align.Center);
            _actionTable.Add(executeLabel).Center();

            _actionTable.Row();

            _attackButton = new ActionButton(skin, ActionButtonType.Attack, "attackActionButton", attackLabel);
            _actionTable.Add(_attackButton).Center();
            _utilitiesButton = new ActionButton(skin, ActionButtonType.Utility, "toolActionButton", utilityLabel);
            _actionTable.Add(_utilitiesButton).Center();
            _supportButton = new ActionButton(skin, ActionButtonType.Support, "itemActionButton", supportLabel);
            _actionTable.Add(_supportButton).Center();
            _executeButton = new ActionButton(skin, ActionButtonType.Execute, "executeButton", executeLabel);
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

            ScreenSpaceOffset = new Vector2(-_actionTable.PreferredWidth / 2, -_actionTable.PreferredHeight);

            return _actionTable;
        }

        //void OnAttackButtonClicked(Button obj)
        //{
        //    _turnHandler.OpenMenu(new AttacksMenu(_basePosition, _turnHandler));
        //}

        //void OnUtilityButtonClicked(Button obj)
        //{
        //    _turnHandler.OpenMenu(new UtilityMenu(_basePosition, _turnHandler));
        //}

        //void OnSupportButtonClicked(Button obj)
        //{
        //    _turnHandler.OpenMenu(new SupportMenu(_basePosition, _turnHandler));
        //}

        //void OnExecuteButtonClicked(Button obj)
        //{
        //    _turnHandler.ExecuteActions();
        //}

        public override IGamepadFocusable GetDefaultFocus()
        {
            return _attackButton;
        }

        public override void AddHandlersToButtons()
        {
            _attackButton.OnClicked += _selectionHandler;
            _attackButton.OnClicked += OnMenuButtonClicked;
            _utilitiesButton.OnClicked += _selectionHandler;
            _utilitiesButton.OnClicked += OnMenuButtonClicked;
            _supportButton.OnClicked += _selectionHandler;
            _supportButton.OnClicked += OnMenuButtonClicked;
            _executeButton.OnClicked += _selectionHandler;
            _executeButton.OnClicked += OnMenuButtonClicked;
        }

        public override void RemoveHandlersFromButtons()
        {
            _attackButton.OnClicked -= _selectionHandler;
            _attackButton.OnClicked -= OnMenuButtonClicked;
            _utilitiesButton.OnClicked -= _selectionHandler;
            _utilitiesButton.OnClicked -= OnMenuButtonClicked;
            _supportButton.OnClicked -= _selectionHandler;
            _supportButton.OnClicked -= OnMenuButtonClicked;
            _executeButton.OnClicked -= _selectionHandler;
            _executeButton.OnClicked -= OnMenuButtonClicked;
        }

        public override void ValidateButtons()
        {

        }
    }
}
