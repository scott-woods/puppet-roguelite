using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
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
    public class Chest : Component
    {
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

            var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();
            var lines = new List<DialogueLine>()
            {
                new DialogueLine("Biggah bonah, what do you think about that? Dookie nukem.")
            };
            textboxManager.DisplayTextbox(lines);
        }
    }
}
