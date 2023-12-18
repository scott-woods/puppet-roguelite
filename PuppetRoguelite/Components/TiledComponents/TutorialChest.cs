using Nez.Textures;
using Nez.Tiled;
using Nez;
using PuppetRoguelite.Components.Characters.Player.PlayerComponents;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models.Items;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.GlobalManagers;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class TutorialChest : Chest
    {
        public event Action OnOpened;

        Sprite _openSprite;
        Sprite _closedSprite;

        bool _isOpened = false;

        public TutorialChest(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(-16, 0, 32, 16));
            _collider.PhysicsLayer = 0;
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Environment);

            //load sprites
            var chestTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Large_chest);
            _openSprite = new Sprite(chestTexture, new RectangleF(TmxObject.Width, 0, TmxObject.Width, TmxObject.Height));
            _closedSprite = new Sprite(chestTexture, new RectangleF(0, 0, TmxObject.Width, TmxObject.Height));
            _renderer.SetSprite(_closedSprite);
            //_renderer.SetRenderLayer(-(int)_collider.Bounds.Center.Y);

            var origin = Entity.AddComponent(new OriginComponent(_collider));
            Entity.AddComponent(new YSorter(_renderer, origin));
        }

        public override IEnumerator HandleInteraction()
        {
            if (!_isOpened)
            {
                _isOpened = true;
                _renderer.SetSprite(_openSprite);
                _interactable.Active = false;

                yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                {
                    new DialogueLine("You open up the chest, and find a Key inside!"),
                    new DialogueLine("...but you throw it aside, and grab the Cereal box inside, like the feral cryptid you are.")
                });

                if (PlayerController.Instance.Entity.TryGetComponent<Inventory>(out var inventory))
                {
                    inventory.AddItem(new CerealBox("Prayno's"));
                }

                OnOpened?.Invoke();
            }
        }
    }
}
