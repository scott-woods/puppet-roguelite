using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.PlayerActions;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class ToolsMenu : UIMenu
    {
        Vector2 _offset = new Vector2(-20, 0);
        Vector2 _playerPosition;
        TurnHandler _turnHandler;
        Skin _skin;
        Vector2 _anchorPosition;
        int _maxPerPage = 4;
        Dialog _dialog;
        Dictionary<ListButton, Type> _buttonDictionary = new Dictionary<ListButton, Type>();

        public ToolsMenu(Vector2 playerPosition, TurnHandler turnHandler,
            bool shouldMaintainFocusedElement = false) : base(shouldMaintainFocusedElement)
        {
            _playerPosition = playerPosition;
            _turnHandler = turnHandler;
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);
        }

        public override void Initialize()
        {
            base.Initialize();

            //get skin
            _skin = CustomSkins.CreateBasicSkin();

            //determine anchor pos
            _anchorPosition = ResolutionHelper.GameToUiPoint(Entity, _playerPosition + _offset);

            //disabled by default
            SetEnabled(false);
        }

        public override void OnEnabled()
        {
            ArrangeElements();

            //set default focus element
            var contentTable = _dialog.GetContentTable();
            DefaultElement = contentTable.GetCells().FirstOrDefault((c) =>
            {
                var button = c.GetElement<Button>();
                if (button == null || button.GetDisabled())
                {
                    return false;
                }
                return true;
            })?.GetElement<Button>();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (_dialog != null)
            {
                _dialog.Remove();
            }

            base.OnDisabled();
        }

        void ArrangeElements()
        {
            //dialog content
            Dictionary<ListButton, Label> dialogContent = new Dictionary<ListButton, Label>();
            var options = Player.Instance.AttacksList.AvailableAttackTypes;
            foreach (var attackType in options)
            {
                //check affordability
                var apCostColor = Color.White;
                var disabled = false;
                var apCost = PlayerActionUtils.GetApCost(attackType);
                if (apCost > Player.Instance.ActionPointComponent.ActionPoints)
                {
                    apCostColor = Color.Gray;
                    disabled = true;
                }

                //create button
                var button = new ListButton(this, PlayerActionUtils.GetName(attackType), _skin, "listButton");
                button.SetDisabled(disabled);
                button.OnClicked += button => _turnHandler.HandleActionSelected(attackType);
                _buttonDictionary.Add(button, attackType);

                //create cost label
                var label = new Label(PlayerActionUtils.GetApCost(attackType).ToString(), new LabelStyle(Graphics.Instance.BitmapFont, apCostColor));

                //add to content list
                dialogContent.Add(button, label);
            }

            //create dialog box
            _dialog = new SelectionDialog(dialogContent, "Tools", _skin, labelHeader: "AP Cost");
            _dialog.SetDebug(false);

            //validate buttons
            //ValidateButtons();

            //add to stage and set position
            Stage.AddElement(_dialog);
            _dialog.SetPosition(_anchorPosition.X - _dialog.PreferredWidth, _anchorPosition.Y - _dialog.PreferredHeight);
        }
    }
}
