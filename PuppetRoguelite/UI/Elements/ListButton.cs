using Nez;
using Nez.UI;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ListButton : TextButton, IInputListener
    {
        bool _isMouseEnabled = true;

        public ListButton(string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            if (!Controls.Instance.Cancel.IsPressed)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);
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

        public void SetMouseEnabled(bool enabled)
        {
            _isMouseEnabled = enabled;
        }
    }
}
