using Nez;
using Nez.Sprites;
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
            skin.AddSprites(Game1.Scene.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style3/Atlas/icons_style_3.atlas"));
            skin.AddSprites(Game1.Scene.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style4/Atlas/icons_style_4.atlas"));

            skin.Add("attackActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 005"),
                skin.GetDrawable("Style 3 Icon 005"), skin.GetDrawable("Style 3 Icon 005")));
            skin.Add("toolActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 299"),
                skin.GetDrawable("Style 3 Icon 299"), skin.GetDrawable("Style 3 Icon 299")));

            //skin.Add("button", new ButtonStyle(skin.GetDrawable("UI_Flat_Button_Small_Lock_01a2"),
            //    skin.GetDrawable("UI_Flat_Button_Small_Lock_01a4"), null));

            return skin;
        }
    }
}
