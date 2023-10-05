using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class ToolsMenu : UIMenu
    {
        Vector2 _offset = new Vector2(-20, 0);
        Vector2 _basePosition;
        Vector2 _anchorPosition;

        TurnHandler _turnHandler;
        
        //elements
        Dialog _dialog;

        Dictionary<ListButton, Type> _buttonDictionary = new Dictionary<ListButton, Type>();

        public ToolsMenu(Vector2 basePosition, TurnHandler turnHandler)
        {
            _basePosition = basePosition;
            _turnHandler = turnHandler;
        }

        public override Element ArrangeElements()
        {
            throw new NotImplementedException();
        }

        public override void ValidateButtons()
        {
            throw new NotImplementedException();
        }

        public override void DetermineDefaultElement()
        {
            throw new NotImplementedException();
        }

        public override void DeterminePosition()
        {
            throw new NotImplementedException();
        }
    }
}
