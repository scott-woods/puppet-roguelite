using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class SpecialChest : Chest
    {
        Sprite _openSprite, _closedSprite;

        public SpecialChest(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            //load sprites
            var chestTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Large_chest);
            _openSprite = new Sprite(chestTexture, new RectangleF(32, 0, 32, 32));
            _closedSprite = new Sprite(chestTexture, new RectangleF(0, 0, 32, 32));
            _renderer.SetSprite(_closedSprite);
            _renderer.SetRenderLayer(-(int)_renderer.Bounds.Center.Y);

            _collider = Entity.AddComponent(new BoxCollider(-16, 0, 32, 16));
        }

        public override IEnumerator HandleInteraction()
        {
            _renderer.SetSprite(_openSprite);
            _interactable.Active = false;

            //display text
            var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();
            var lines = new List<DialogueLine>()
            {
                new DialogueLine("You look inside the box.."),
                new DialogueLine($"Huh, you wanted an item?"),
                new DialogueLine($"SUCK"),
                new DialogueLine($"ME")
            };
            yield return textboxManager.DisplayTextbox(lines);

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Space_ship_3);
            Game1.SceneManager.ChangeScene(typeof(Bedroom), "0", Color.White, 4f, 4f, 1f);
        }
    }
}
