using Nez;
using Nez.UI;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ActionButton : ImageButton, IInputListener
    {
        Label _label;
        public ActionButtonType Type;
        bool _isMouseEnabled = true;

        public ActionButton(Skin skin, ActionButtonType type, string styleName = null, Label label = null) : base(skin, styleName)
        {
            _label = label;
            Type = type;
            GetImage().SetScaleX(Game1.ResolutionScale.X / 2);
            GetImage().SetScaleY(Game1.ResolutionScale.Y / 2);
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            if (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);
            }

            if (_label != null)
            {
                _label.SetVisible(true);
            }
        }

        protected override void OnUnfocused()
        {
            base.OnUnfocused();

            if (_label != null)
            {
                _label.SetVisible(false);
            }
        }

        protected override void OnActionButtonPressed()
        {
            base.OnActionButtonPressed();

            if (!_isDisabled)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select);
            }
            else
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
            }
        }

        void IInputListener.OnMouseEnter()
        {
            if (_isMouseEnabled)
            {
                GetStage().SetGamepadFocusElement(null);
                GetStage().SetGamepadFocusElement(this);
            }
        }

        void IInputListener.OnMouseExit()
        {

        }

        bool IInputListener.OnLeftMousePressed(Microsoft.Xna.Framework.Vector2 mousePos)
        {
            if (_isMouseEnabled)
            {
                if (!_isDisabled)
                    OnActionButtonPressed();
            }
            return !_isDisabled;
        }

        public void SetMouseEnabled(bool enabled)
        {
            _isMouseEnabled = enabled;
        }
    }

    public enum ActionButtonType
    {
        Attack,
        Utility,
        Support,
        Execute
    }
}
