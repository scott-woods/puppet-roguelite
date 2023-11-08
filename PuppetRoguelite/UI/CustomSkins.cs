using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
            skin.AddSprites(Game1.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style3/Atlas/icons_style_3.atlas"));
            skin.AddSprites(Game1.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style4/Atlas/icons_style_4.atlas"));
            skin.AddSprites(Game1.Content.LoadSpriteAtlas("Content/Textures/UI/Menu/Style3/Atlas/menu_style_3.atlas"));
            skin.Add("font_abaddon_light", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light));

            var subTexture = skin.GetSprite("Inventory_01");
            var ninePatch = new NinePatchDrawable(subTexture, 16, 16, 16, 16);
            skin.Add("np_inventory_01", ninePatch);

            //action buttons
            skin.Add("attackActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 005"),
                skin.GetDrawable("Style 3 Icon 005"), skin.GetDrawable("Style 3 Icon 005")));
            skin.Add("toolActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 289"),
                skin.GetDrawable("Style 3 Icon 289"), skin.GetDrawable("Style 3 Icon 289")));
            skin.Add("itemActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 155"),
                skin.GetDrawable("Style 3 Icon 155"), skin.GetDrawable("Style 3 Icon 155")));
            skin.Add("executeButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 324"),
                skin.GetDrawable("Style 3 Icon 324"), skin.GetDrawable("Style 3 Icon 324")));

            //progress bar sprites
            var progressBarTextures = Game1.Content.LoadTexture(Nez.Content.Textures.UI.Sliders);
            var progressBarSprites = Sprite.SpritesFromAtlas(progressBarTextures, 35, 16);
            for (int i = 0; i < 10; i++)
            {
                skin.Add($"progressBarBackground_{i}", progressBarSprites[i]);
            }

            //text buttons
            skin.Add("listButton", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light"),
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
                Font = skin.GetFont("font_abaddon_light"),
                FontColor = Color.White
            });

            skin.Add("plusButton", new ButtonStyle()
            {
                Up = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Icons.Plus_icon_up)),
                Down = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Icons.Plus_icon_down)),
                Over = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Icons.Plus_icon_down)),
            });

            //progress bar
            for (int i = 0; i < 10; i++)
            {
                var progressBarStyle = new ProgressBarStyle
                {
                    Background = skin.GetSpriteDrawable($"progressBarBackground_{i}")
                    //KnobBefore = skin.GetSpriteDrawable("progressBarKnobBefore"),
                    //KnobAfter = skin.GetSpriteDrawable("progressBarKnobAfter"),
                    //Background = new PrimitiveDrawable(0, 5, Color.LightGray),
                    //KnobBefore = new PrimitiveDrawable(0, 5, Color.Green)
                };
                skin.Add($"progressBar_{i}", progressBarStyle);
            }

            return skin;
        }
    }
}
