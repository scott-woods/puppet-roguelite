using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.PlayerActions;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class AttacksMenu : UIMenu
    {
        Vector2 _offset = new Vector2(-20, 0);
        Vector2 _basePosition;
        Vector2 _anchorPosition;

        TurnHandler _turnHandler;

        //elements
        Dialog _dialog;

        Dictionary<Button, Type> _buttonDictionary = new Dictionary<Button, Type>();

        public AttacksMenu(Vector2 basePosition, TurnHandler turnHandler)
        {
            _basePosition = basePosition;
            _turnHandler = turnHandler;
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            if (_dialog != null)
            {
                var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition);
                _dialog.SetPosition(pos.X - _dialog.PreferredWidth, pos.Y - _dialog.PreferredHeight);
            }
        }

        public override Element ArrangeElements()
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
                var button = new ListButton(PlayerActionUtils.GetName(attackType), _defaultSkin, "listButton");
                button.SetDisabled(disabled);
                button.OnClicked += button => _turnHandler.HandleActionSelected(attackType);
                _buttonDictionary.Add(button, attackType);

                //create cost label
                var label = new Label(PlayerActionUtils.GetApCost(attackType).ToString(), new LabelStyle(Graphics.Instance.BitmapFont, apCostColor));

                //add to content list
                dialogContent.Add(button, label);
            }

            //create dialog box
            _dialog = new SelectionDialog(dialogContent, "Tools", _defaultSkin, labelHeader: "AP Cost");
            _dialog.SetDebug(false);

            //validate buttons
            ValidateButtons();

            //add to stage and set position
            Stage.AddElement(_dialog);
            return _dialog;
        }

        public override void ValidateButtons()
        {
            foreach(var pair in  _buttonDictionary)
            {
                //check affordability
                var apCostColor = Color.White;
                var disabled = false;
                var apCost = PlayerActionUtils.GetApCost(pair.Value);
                if (apCost > Player.Instance.ActionPointComponent.ActionPoints)
                {
                    apCostColor = Color.Gray;
                    disabled = true;
                }

                pair.Key.SetDisabled(disabled);
            }
        }

        public override void DetermineDefaultElement()
        {
            var contentTable = _dialog.GetContentTable();
            Stage.SetGamepadFocusElement(contentTable.GetCells().FirstOrDefault((c) =>
            {
                var button = c.GetElement<Button>();
                if (button == null || button.GetDisabled())
                {
                    return false;
                }
                return true;
            })?.GetElement<Button>());
        }

        public override void DeterminePosition()
        {
            _anchorPosition = _basePosition + _offset;
        }
    }
}
