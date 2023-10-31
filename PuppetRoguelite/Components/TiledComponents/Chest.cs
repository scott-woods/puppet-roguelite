using Nez.Sprites;
using Nez.Textures;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Items;
using System.Collections;
using PuppetRoguelite.Models;

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
