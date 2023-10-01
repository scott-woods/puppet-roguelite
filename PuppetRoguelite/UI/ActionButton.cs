﻿using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class ActionButton : Button
    {
        UIMenu _menu;

        public ActionButton(Skin skin, string styleName = null) : base(skin, styleName)
        {

        }

        public ActionButton(UIMenu menu, Skin skin, string styleName = null) : base(skin, styleName)
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
