﻿using Nez;
using Nez.UI;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ListButton : TextButton, IInputListener
    {
        public ListButton(string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            if (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02, 1.2f);
            }
        }

        void IInputListener.OnMouseEnter()
        {
            OnFocused();
        }

        bool IInputListener.OnLeftMousePressed(Microsoft.Xna.Framework.Vector2 mousePos)
        {
            OnActionButtonPressed();
            return !_isDisabled;
        }

        protected override void OnActionButtonPressed()
        {
            base.OnActionButtonPressed();

            if (!_isDisabled)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select, .3f);
            }
            else
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
            }
        }
    }
}
