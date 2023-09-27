using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public static class CustomSkins
    {
        public static Skin CreateBasicSkin()
        {
            var skin = new Skin();
            skin.AddSprites(Game1.Scene.Content.LoadSpriteAtlas("Content/UI/basic_ui.atlas"));

            skin.Add("button", new ButtonStyle(skin.GetDrawable("UI_Flat_Button_Small_Lock_01a2"),
                skin.GetDrawable("UI_Flat_Button_Small_Lock_01a4"), null));

            return skin;
        }
    }
}
