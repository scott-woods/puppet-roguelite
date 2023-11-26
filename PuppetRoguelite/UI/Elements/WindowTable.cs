using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class WindowTable : Table
    {
        public WindowTable(Skin skin)
        {
            SetBackground(skin.GetNinePatchDrawable("np_inventory_01"));
        }
    }
}
