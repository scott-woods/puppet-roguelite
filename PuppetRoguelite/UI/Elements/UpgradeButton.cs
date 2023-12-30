using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.SaveData.Upgrades;
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
        public UpgradeBase Upgrade;
        public int RowIndex;

        public event Action<UpgradeBase> OnButtonFocused;
        public event Action<UpgradeBase> OnButtonUnfocused;

        public UpgradeButton(UpgradeBase upgrade, int rowIndex, Skin skin, string styleName = null) : base(skin, styleName)
        {
            Upgrade = upgrade;
            RowIndex = rowIndex;
            GetImage().SetScaleX(Game1.ResolutionScale.X);
            GetImage().SetScaleY(Game1.ResolutionScale.Y);
        }

        protected override void OnActionButtonPressed()
        {
            base.OnActionButtonPressed();

            if (_isDisabled)
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            if (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);
            }

            OnButtonFocused?.Invoke(Upgrade);
        }

        protected override void OnUnfocused()
        {
            base.OnUnfocused();

            OnButtonUnfocused?.Invoke(Upgrade);
        }

        void IInputListener.OnMouseEnter()
        {
            GetStage().SetGamepadFocusElement(null);
            GetStage().SetGamepadFocusElement(this);
        }

        void IInputListener.OnMouseExit()
        {

        }

        bool IInputListener.OnLeftMousePressed(Microsoft.Xna.Framework.Vector2 mousePos)
        {
            OnActionButtonPressed();
            return !_isDisabled;
        }
    }
}
