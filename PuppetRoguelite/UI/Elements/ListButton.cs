using Nez.UI;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ListButton : TextButton
    {
        UIMenu _menu;

        public ListButton(string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
        }

        public ListButton(UIMenu menu, string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
            _menu = menu;
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            if (_menu != null)
            {
                _menu.OnMenuButtonFocused(this);
            }
        }
    }
}
