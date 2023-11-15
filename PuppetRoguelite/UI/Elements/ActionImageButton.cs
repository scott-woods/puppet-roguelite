using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ActionImageButton : ImageButton
    {
        Label _label;
        public ActionButtonType Type;

        public ActionImageButton(Skin skin, ActionButtonType type, string styleName = null, Label label = null) : base(skin, styleName)
        {
            _label = label;
            Type = type;
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
    }
}
