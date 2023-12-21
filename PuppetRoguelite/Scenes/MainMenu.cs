using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Scenes
{
    public class MainMenu : BaseScene
    {
        Entity _uiEntity;

        public override void Initialize()
        {
            base.Initialize();

            CreateEntity("mouse-cursor").AddComponent(new MouseCursor());
        }

        public override void Begin()
        {
            base.Begin();

            Game1.StartCoroutine(SplashScreen());
        }

        IEnumerator SplashScreen()
        {
            var texture = Content.LoadTexture(Nez.Content.Textures.UI.Cat);
            var sprite = new Sprite(texture);
            var splash = CreateEntity("splash").AddComponent(new SpriteRenderer(texture));
            splash.Entity.SetScale(.2f);
            splash.Entity.SetPosition(Game1.DesignResolution.X / 2, Game1.DesignResolution.Y / 2);
            splash.SetColor(Color.Transparent);

            var susboTexture = Content.LoadTexture(Nez.Content.Textures.UI.Susbo);
            var susboRenderer = CreateEntity("susbo").AddComponent(new SpriteRenderer(susboTexture));
            susboRenderer.Entity.SetScale(.2f);
            var offset = new Vector2(0, ((texture.Height / 2) * .2f) + ((susboTexture.Height / 2) * .2f) + 10f);
            susboRenderer.Entity.SetPosition(splash.Entity.Position + offset);
            susboRenderer.SetColor(Color.Transparent);

            yield return Coroutine.WaitForSeconds(1f);

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Music.Logo_sound);

            var splashTween = splash.TweenColorTo(Color.White, .5f);
            splashTween.Start();

            var susboTween = susboRenderer.TweenColorTo(Color.White, .5f);
            susboTween.Start();

            while (splashTween.IsRunning() || susboTween.IsRunning())
                yield return null;

            yield return Coroutine.WaitForSeconds(2f);

            splashTween = splash.TweenColorTo(Color.Transparent, 1f);
            splashTween.Start();

            susboTween = susboRenderer.TweenColorTo(Color.Transparent, 1f);
            susboTween.Start();

            while (splashTween.IsRunning() || susboTween.IsRunning())
                yield return null;

            yield return Coroutine.WaitForSeconds(1f);

            splash.Entity.Destroy();
            susboRenderer.Entity.Destroy();

            Game1.AudioManager.PlayMusic(Songs.Menu);

            _uiEntity = CreateEntity("ui");
            _uiEntity.AddComponent(new MainMenuUI());
        }
    }
}
