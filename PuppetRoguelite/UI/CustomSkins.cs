﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
            var skin = new Skin();

            //load sprite atlas assets
            skin.AddSprites(Game1.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style3/Atlas/icons_style_3.atlas"));
            skin.AddSprites(Game1.Content.LoadSpriteAtlas("Content/Textures/UI/Icons/Style4/Atlas/icons_style_4.atlas"));
            skin.AddSprites(Game1.Content.LoadSpriteAtlas("Content/Textures/UI/Menu/Style3/Atlas/menu_style_3.atlas"));

            //load fonts
            skin.Add("font_m57_12", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.M57_12));
            skin.Add("font_abaddon_light_12", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light_12));
            skin.Add("font_abaddon_light_18", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light_18));
            skin.Add("font_abaddon_light_24", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light_24));
            skin.Add("font_abaddon_light_36", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light_36));
            skin.Add("font_abaddon_light_48", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Font_abaddon_light_48));
            skin.Add("font_abaddon_light_60", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light_60));
            skin.Add("font_abaddon_light_72", Game1.Content.LoadBitmapFont(Nez.Content.Fonts.Abaddon_light_72));

            //nine patches
            var subTexture = skin.GetSprite("Inventory_01");
            var ninePatch = new NinePatchDrawable(subTexture, 16, 16, 16, 16);
            skin.Add("np_inventory_01", ninePatch);

            var dark1Texture = Game1.Content.LoadTexture(Nez.Content.Textures.UI.Borders_and_hp_no_glow);
            var dark1Sprite = new Sprite(dark1Texture, 208, 64, 48, 48);
            var dark1NinePatch = new NinePatchDrawable(dark1Sprite, 16, 16, 16, 16);
            skin.Add("np_dark_1", dark1NinePatch);

            //action buttons
            //skin.Add("attackActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 005"),
            //    skin.GetDrawable("Style 3 Icon 005"), skin.GetDrawable("Style 3 Icon 005")));
            //skin.Add("toolActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 289"),
            //    skin.GetDrawable("Style 3 Icon 289"), skin.GetDrawable("Style 3 Icon 289")));
            //skin.Add("itemActionButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 155"),
            //    skin.GetDrawable("Style 3 Icon 155"), skin.GetDrawable("Style 3 Icon 155")));
            //skin.Add("executeButton", new ButtonStyle(skin.GetDrawable("Style 4 Icon 324"),
            //    skin.GetDrawable("Style 3 Icon 324"), skin.GetDrawable("Style 3 Icon 324")));

            skin.Add("attackActionButton", new ImageButtonStyle()
            {
                ImageUp = skin.GetDrawable("Style 4 Icon 005"),
                ImageDown = skin.GetDrawable("Style 3 Icon 005"),
                ImageOver = skin.GetDrawable("Style 3 Icon 005"),
            });
            skin.Add("toolActionButton", new ImageButtonStyle()
            {
                ImageUp = skin.GetDrawable("Style 4 Icon 289"),
                ImageDown = skin.GetDrawable("Style 3 Icon 289"),
                ImageOver = skin.GetDrawable("Style 3 Icon 289"),
            });
            skin.Add("itemActionButton", new ImageButtonStyle()
            {
                ImageUp = skin.GetDrawable("Style 4 Icon 155"),
                ImageDown = skin.GetDrawable("Style 3 Icon 155"),
                ImageOver = skin.GetDrawable("Style 3 Icon 155"),
            });
            skin.Add("executeButton", new ImageButtonStyle()
            {
                ImageUp = skin.GetDrawable("Style 4 Icon 324"),
                ImageDown = skin.GetDrawable("Style 3 Icon 324"),
                ImageOver = skin.GetDrawable("Style 3 Icon 324"),
            });

            //progress bar
            var barTexture = Game1.Content.LoadTexture(Nez.Content.Textures.UI.ProgressBar);
            var barBgSprite = new Sprite(barTexture, new Rectangle(0, 0, 64, 16));
            var barKnobBeforeSprite = new Sprite(barTexture, new Rectangle(0, 16, 64, 16));
            var barBg = new SpriteDrawable(barBgSprite);
            barBg.MinHeight = 32;
            barBg.LeftWidth = 1;
            barBg.RightWidth = 1;
            var barKnobBefore = new SpriteDrawable(barKnobBeforeSprite);
            barKnobBefore.MinHeight = 32;
            barKnobBefore.MinWidth = 0;
            skin.Add("progressBar", new ProgressBarStyle()
            {
                Background = barBg,
                KnobBefore = barKnobBefore,
            });

            var glowBarTexture = Game1.Content.LoadTexture(Nez.Content.Textures.UI.ProgressBarGlow);
            var glowBarBgSprite = new Sprite(glowBarTexture, new Rectangle(0, 0, 64, 16));
            var glowBarKnobBeforeSprite = new Sprite(glowBarTexture, new Rectangle(0, 16, 64, 16));
            var glowBarBg = new SpriteDrawable(glowBarBgSprite);
            glowBarBg.MinHeight = 48;
            glowBarBg.LeftWidth = 1;
            glowBarBg.RightWidth = 1;
            var glowBarKnobBefore = new SpriteDrawable(glowBarKnobBeforeSprite);
            glowBarKnobBefore.MinHeight = 48;
            glowBarKnobBefore.MinWidth = 0;
            skin.Add("glowProgressBar", new ProgressBarStyle()
            {
                Background = glowBarBg,
                KnobBefore = glowBarKnobBefore,
            });

            //progress bar sprites
            var progressBarTextures = Game1.Content.LoadTexture(Nez.Content.Textures.UI.Sliders);
            var progressBarSprites = Sprite.SpritesFromAtlas(progressBarTextures, 35, 16);
            for (int i = 0; i < 10; i++)
            {
                skin.Add($"progressBarBackground_{i}", progressBarSprites[i]);
            }

            //text buttons
            skin.Add("listButton_xs", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_12"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });
            skin.Add("listButton_s", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_18"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });
            skin.Add("listButton_md", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_24"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });
            skin.Add("listButton_lg", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_36"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });
            skin.Add("listButton_xl", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_48"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });
            skin.Add("listButton_xxl", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_60"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });
            skin.Add("listButton_xxxl", new TextButtonStyle()
            {
                Font = skin.GetFont("font_abaddon_light_72"),
                Up = new PrimitiveDrawable(Color.Transparent),
                Down = new PrimitiveDrawable(Color.Yellow),
                Over = new PrimitiveDrawable(Color.Black),
                DisabledFontColor = Color.Gray
            });

            //dialog
            skin.Add("actionDialog", new WindowStyle()
            {
                Background = skin.GetNinePatchDrawable("np_inventory_01"),
                TitleFontColor = Color.White
            });

            //label
            skin.Add("m57_12", new LabelStyle()
            {
                Font = skin.GetFont("font_m57_12"),
                FontColor = Color.White
            });
            skin.Add("default_xs", new LabelStyle()
            {
                Font = skin.GetFont("font_abaddon_light_12"),
                FontColor = Color.White
            });
            skin.Add("default_s", new LabelStyle
            {
                Font = skin.GetFont("font_abaddon_light_18"),
                FontColor = Color.White
            });
            skin.Add("default_md", new LabelStyle
            {
                Font = skin.GetFont("font_abaddon_light_24"),
                FontColor = Color.White
            });
            skin.Add("default_lg", new LabelStyle
            {
                Font = skin.GetFont("font_abaddon_light_36"),
                FontColor = Color.White
            });
            skin.Add("default_xl", new LabelStyle
            {
                Font = skin.GetFont("font_abaddon_light_48"),
                FontColor = Color.White
            });
            skin.Add("default_xxl", new LabelStyle()
            {
                Font = skin.GetFont("font_abaddon_light_60"),
                FontColor = Color.White
            });
            skin.Add("default_xxxl", new LabelStyle()
            {
                Font = skin.GetFont("font_abaddon_light_72"),
                FontColor = Color.White
            });

            skin.Add("plusButton", new ImageButtonStyle()
            {
                ImageUp = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Icons.Plus_icon_up)),
                ImageDown = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Icons.Plus_icon_down)),
                ImageOver = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Icons.Plus_icon_down)),
            });

            skin.Add("tooltip", new TextTooltipStyle()
            {
                LabelStyle = skin.Get<LabelStyle>("default_md"),
                Background = skin.GetNinePatchDrawable("np_inventory_01")
            });

            skin.Add("newGameButton", new ButtonStyle()
            {
                Up = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.New_game_button)),
                Down = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.New_game_button)),
                Over = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.New_game_button)),
            });

            skin.Add("continueButton", new ButtonStyle()
            {
                Up = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Continue_button)),
                Down = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Continue_button)),
                Over = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Continue_button)),
            });

            skin.Add("quitButton", new ButtonStyle()
            {
                Up = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Quit_button)),
                Down = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Quit_button)),
                Over = new SpriteDrawable(Game1.Content.LoadTexture(Nez.Content.Textures.UI.Quit_button)),
            });

            //skin.Add("progressBar", new ProgressBarStyle()
            //{
            //    Background = new PrimitiveDrawable(1, 16, Color.LightGray),
            //    KnobBefore = new PrimitiveDrawable(1, 16, Color.Green)
            //});

            //progress bar
            //for (int i = 0; i < 10; i++)
            //{
            //    var progressBarStyle = new ProgressBarStyle
            //    {
            //        Background = skin.GetSpriteDrawable($"progressBarBackground_{i}")
            //        //KnobBefore = skin.GetSpriteDrawable("progressBarKnobBefore"),
            //        //KnobAfter = skin.GetSpriteDrawable("progressBarKnobAfter"),
            //        //Background = new PrimitiveDrawable(0, 5, Color.LightGray),
            //        //KnobBefore = new PrimitiveDrawable(0, 5, Color.Green)
            //    };
            //    skin.Add($"progressBar_{i}", progressBarStyle);
            //}

            return skin;
        }
    }
}
