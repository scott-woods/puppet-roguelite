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
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02, 1.2f);
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

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select, .3f);
        }

        void IInputListener.OnMouseEnter()
        {
            OnFocused();
        }

        void IInputListener.OnMouseExit()
        {
            OnUnfocused();
        }

        bool IInputListener.OnLeftMousePressed(Microsoft.Xna.Framework.Vector2 mousePos)
        {
            OnActionButtonPressed();
            return true;
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
