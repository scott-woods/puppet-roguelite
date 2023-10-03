using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    public class UIMenu : UICanvas
    {
        bool _shouldMaintainFocusedElement;

        protected IGamepadFocusable DefaultElement;
        IGamepadFocusable _lastFocusedElement;

        public UIMenu(bool shouldMaintainFocusedElement)
        {
            _shouldMaintainFocusedElement = shouldMaintainFocusedElement;
        }

        public override void Initialize()
        {
            base.Initialize();
            
            //set default accept key
            Stage.KeyboardActionKey = Microsoft.Xna.Framework.Input.Keys.Z;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            if (!_shouldMaintainFocusedElement)
            {
                if (DefaultElement != _lastFocusedElement)
                {
                    Stage.SetGamepadFocusElement(DefaultElement);
                }
            }

            //IGamepadFocusable nextElement = null;

            //Stage.SetGamepadFocusElement(null);

            //if (_lastFocusedElement != null)
            //{
            //    nextElement = _lastFocusedElement;
            //}
            //else if (DefaultElement != null)
            //{
            //    nextElement = DefaultElement;
            //}

            //if (nextElement != null)
            //{
            //    Stage.SetGamepadFocusElement(nextElement);
            //    if (_shouldMaintainFocusedElement)
            //    {
            //        _lastFocusedElement = nextElement;
            //    }
            //}
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            //Stage.SetGamepadFocusElement(null);
        }

        public void OnMenuButtonFocused(Button button)
        {
            _lastFocusedElement = button;
        }
    }
}
