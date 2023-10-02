using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
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
            //load assets
            var skin = new Skin();
            skin.AddSprites(Game1.Scene.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style3/Atlas/icons_style_3.atlas"));
            skin.AddSprites(Game1.Scene.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style4/Atlas/icons_style_4.atlas"));
            skin.AddSprites(Game1.Scene.Content.LoadSpriteAtlas("Content/Textures/UI/Menu/Style3/Atlas/menu_style_3.atlas"));

            var subTexture = skin.GetSprite("Inventory_01");
            var ninePatch = new NinePatchDrawable(subTexture, 16, 16, 16, 16);
            skin.Add("np_inventory_01", ninePatch);

            //action buttons
            skin.Add("attackActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 005"),
                skin.GetDrawable("Style 3 Icon 005"), skin.GetDrawable("Style 3 Icon 005")));
            skin.Add("toolActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 299"),
                skin.GetDrawable("Style 3 Icon 299"), skin.GetDrawable("Style 3 Icon 299")));
            skin.Add("executeButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 299"),
                skin.GetDrawable("Style 3 Icon 299"), skin.GetDrawable("Style 3 Icon 299")));

            //text buttons
            skin.Add("listButton", new TextButtonStyle()
            {
                Font = Graphics.Instance.BitmapFont,
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });

            //dialog
            skin.Add("actionDialog", new WindowStyle()
            {
                Background = skin.GetNinePatchDrawable("np_inventory_01"),
                TitleFontColor = Color.White,
                TitleFontScaleX = 1.25f,
                TitleFontScaleY = 1.25f
            });

            //label
            skin.Add("defaultLabel", new LabelStyle()
            {
                Font = Graphics.Instance.BitmapFont,
                FontColor = Color.White
            });

            return skin;
        }
    }
}
