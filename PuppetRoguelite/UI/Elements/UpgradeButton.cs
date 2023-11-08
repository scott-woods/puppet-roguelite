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
    public class UpgradeButton : Button
    {
        public Upgrade Upgrade;
        public int RowIndex;

        public UpgradeButton(Upgrade upgrade, int rowIndex, Skin skin, string styleName = null) : base(skin, styleName)
        {
            Upgrade = upgrade;
            RowIndex = rowIndex;
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            if (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02, 1.2f);
            }
        }
    }
}
