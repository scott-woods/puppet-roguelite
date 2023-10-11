using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class KeyChest : Component
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

        SpriteRenderer _renderer;
        Collider _collider;
        Interactable _interactable;

        Sprite _openSprite;
        Sprite _closedSprite;

        public override void Initialize()
        {
            base.Initialize();

            _renderer = Entity.AddComponent(new SpriteRenderer());
            var chestTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Chest);
            _openSprite = new Sprite(chestTexture, 0, 0, 16, 32);
            _closedSprite = new Sprite(chestTexture, 16, 0, 16, 32);
            _renderer.SetSprite(_closedSprite);

            _collider = Entity.AddComponent(new BoxCollider(16, 16));
            _collider.LocalOffset += new Vector2(0, _renderer.Height / 4);
            _collider.PhysicsLayer = (int)PhysicsLayers.Collider;

            _interactable = Entity.AddComponent(new Interactable());
            _interactable.Emitter.AddObserver(InteractableEvents.Interacted, OnInteracted);
        }

        void OnInteracted()
        {
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
            textboxManager.DisplayTextbox(lines);
        }
    }
}
