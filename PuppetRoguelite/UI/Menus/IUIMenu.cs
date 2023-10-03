using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public interface IUIMenu
    {
        /// <summary>
        /// arrange elements and return the base element
        /// </summary>
        /// <returns></returns>
        Element ArrangeElements();
        void ValidateButtons();
        void DetermineDefaultElement();
        void DeterminePosition();
    }
}
