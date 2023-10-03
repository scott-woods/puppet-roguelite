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
    public class ActionButton : Button
    {
        Label _label;

        public ActionButton(Skin skin, string styleName = null, Label label = null) : base(skin, styleName)
        {
            _label = label;
        }

        protected override void OnFocused()
        {
            base.OnFocused();

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
    }
}
