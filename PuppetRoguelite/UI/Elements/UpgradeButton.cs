using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Models.Upgrades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class UpgradeButton : ImageButton, IInputListener
    {
        public Upgrade Upgrade;
        public int RowIndex;

        public UpgradeButton(Upgrade upgrade, int rowIndex, Skin skin, string styleName = null) : base(skin, styleName)
        {
            Upgrade = upgrade;
            RowIndex = rowIndex;
            GetImage().SetScaleX(Game1.ResolutionScale.X);
            GetImage().SetScaleY(Game1.ResolutionScale.Y);
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
    }
}
