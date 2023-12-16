using Nez;
using Nez.Sprites;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections;

namespace PuppetRoguelite.Components.TiledComponents
{
    public abstract class Chest : TiledComponent
    {
        protected SpriteRenderer _renderer;
        protected Collider _collider;
        protected Interactable _interactable;

        public Chest(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _renderer = Entity.AddComponent(new SpriteRenderer());

            _interactable = Entity.AddComponent(new Interactable(HandleInteraction));
        }

        public abstract IEnumerator HandleInteraction();
    }
}
