using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class ActionMenu : UIMenu
    {
        //elements
        SelectionDialog<PlayerActionType> _dialog;

        List<PlayerActionType> _options;
        string _dialogTitle;
        Action<Type> _actionSelectedHandler;

        Dictionary<BulletPointSelector<PlayerActionType>, Type> _buttonDictionary = new Dictionary<BulletPointSelector<PlayerActionType>, Type>();

        public ActionMenu(List<PlayerActionType> options, string dialogTitle, Action<Type> actionSelectedHandler, Action cancelHandler) : base(cancelHandler)
        {
            _options = options;
            _dialogTitle = dialogTitle;
            _actionSelectedHandler = actionSelectedHandler;
        }

        public override void Initialize()
        {
            base.Initialize();

            WorldSpaceOffset = new Vector2(0, -16);
        }

        public override Element ArrangeElements()
        {
            var skin = CustomSkins.CreateBasicSkin();

            //dialog content
            Dictionary<BulletPointSelector<PlayerActionType>, Label> dialogContent = new Dictionary<BulletPointSelector<PlayerActionType>, Label>();
            foreach (var option in _options)
            {
                var type = option.ToType();

                //check affordability
                var apCostColor = Color.White;
                var disabled = false;
                var apCost = PlayerActionUtils.GetApCost(type);
#if DEBUG
                if (Game1.DebugSettings.FreeActions) apCost = 0;
#endif
                if (apCost > PlayerController.Instance.ActionPointComponent.ActionPoints)
                {
                    apCostColor = Color.Gray;
                    disabled = true;
                }

                //create button
                var button = new BulletPointSelector<PlayerActionType>(PlayerActionUtils.GetName(type), option, false, skin, "default_lg");
                button.SetDisabled(disabled);
                _buttonDictionary.Add(button, type);

                //create cost label
                var label = new Label(PlayerActionUtils.GetApCost(type).ToString(), skin, "default_lg");

                //add to content list
                dialogContent.Add(button, label);
            }

            //create dialog box
            _dialog = new SelectionDialog<PlayerActionType>(dialogContent, skin, labelHeader: "AP Cost");

            ScreenSpaceOffset = new Vector2(-_dialog.PreferredWidth, -_dialog.PreferredHeight);

            return _dialog;
        }

        public override IGamepadFocusable GetDefaultFocus()
        {
            var button = _dialog.GetCells().FirstOrDefault((c) =>
            {
                var button = c.GetElement<BulletPointSelector<PlayerActionType>>();
                if (button == null || button.GetDisabled())
                {
                    return false;
                }
                return true;
            })?.GetElement<BulletPointSelector<PlayerActionType>>();

            if (button == null)
            {
                if (_buttonDictionary != null && _buttonDictionary.Count > 0)
                    button = _buttonDictionary.First().Key;
            }

            return button;
        }

        public override void AddHandlersToButtons()
        {
            foreach (var button in _buttonDictionary)
            {
                button.Key.OnSelected += OnButtonClicked;
            }
        }

        public override void RemoveHandlersFromButtons()
        {
            foreach (var button in _buttonDictionary)
            {
                button.Key.OnSelected -= OnButtonClicked;
            }
        }

        void OnButtonClicked(PlayerActionType actionType)
        {
            _actionSelectedHandler?.Invoke(actionType.ToType());
        }

        public override void ValidateButtons()
        {
            foreach (var button in _buttonDictionary.Keys)
            {
                //check affordability
                var disabled = false;
                var apCost = PlayerActionUtils.GetApCost(_buttonDictionary[button]);
#if DEBUG
                if (Game1.DebugSettings.FreeActions) apCost = 0;
#endif
                if (apCost > PlayerController.Instance.ActionPointComponent.ActionPoints)
                {
                    disabled = true;
                }

                button.SetDisabled(disabled);
            }
        }

        public List<BulletPointSelector<PlayerActionType>> GetButtons()
        {
            return _buttonDictionary.Keys.ToList();
        }

        public override void EnableButtons()
        {
            base.EnableButtons();

            foreach (var button in _buttonDictionary.Keys)
            {
                button.SetDisabled(false);
                //button.SetMouseEnabled(true);
            }
        }

        public override void DisableForTutorial()
        {
            base.DisableForTutorial();

            foreach (var button in _buttonDictionary.Keys)
            {
                button.SetDisabled(true);
                //button.SetMouseEnabled(false);
            }
        }
    }
}
