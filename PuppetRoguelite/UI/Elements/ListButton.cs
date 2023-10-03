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
        public ListButton(string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();
        }
    }
}
