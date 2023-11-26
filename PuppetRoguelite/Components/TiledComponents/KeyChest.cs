using Nez;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerComponents;
using PuppetRoguelite.Models;
using PuppetRoguelite.Models.Items;
using PuppetRoguelite.SceneComponents;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class KeyChest : Chest
    {
        Dictionary<string, string> _cerealNames = new Dictionary<string, string>()
        {
            { "Reese's Puffs", "It's a box of Reese's Puffs! Peanut buttah chocolate flavor, dude." },
            { "Captain Crunch", "It's some Cap'n Crunch!" },
            { "Lucky Charms", "It's a box of Lucky Charms! Magically delicious!" },
            { "Count Chochochocula", "It's.. Count Chochochocula? Did they change the name..?" },
            { "Cheerios", "It's some Cheerios! You must be a bit of a normie..." },
            { "Frosted Shredded Wheat", "'Frosted Shredded Wheat'? Guess they were out of Frosted Flakes." },
            { "Lucky Shapes", "There's a box of Lucky Shapes! How wondrously piquant!" },
            { "Crispy Hexagons", "It's Crispy Hexagons! I prefer the Nonagons, myself." },
            { "Honey Bunches of Oats", "It's Honey Bunches of Oats, honey!" }
        };

        Sprite _openSprite;
        Sprite _closedSprite;

        bool _isOpened = false;

        public KeyChest(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(-8, 0, 16, 16));

            //load sprites
            var chestTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Chest);
            _openSprite = new Sprite(chestTexture, new RectangleF(0, 0, TmxObject.Width, TmxObject.Height));
            _closedSprite = new Sprite(chestTexture, new RectangleF(TmxObject.Width, 0, TmxObject.Width, TmxObject.Height));
            _renderer.SetSprite(_closedSprite);
            _renderer.SetRenderLayer(-(int)_collider.Bounds.Center.Y);
        }

        public override IEnumerator HandleInteraction()
        {
            if (!_isOpened)
            {
                _isOpened = true;
                _renderer.SetSprite(_openSprite);
                _interactable.Active = false;

                //add item to player inventory
                var cerealName = _cerealNames.ElementAt(Nez.Random.NextInt(_cerealNames.Count));
                var cerealBox = new CerealBox(cerealName.Key);
                if (PlayerController.Instance.Entity.TryGetComponent<Inventory>(out var inventory))
                {
                    inventory.AddItem(cerealBox);
                }

                //display text
                var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();
                var lines = new List<DialogueLine>()
            {
                new DialogueLine("You found a key!"),
                new DialogueLine($"...what's this? Looks like there's something else in there..."),
                new DialogueLine(cerealName.Value)
            };
                yield return textboxManager.DisplayTextbox(lines);
            }
        }
    }
}
