using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Components.Shared;
using System.Collections;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class WallLever : TiledComponent
    {
        public bool CanBeInteractedWith = true;
        public event Action OnLeverPulled;

        bool _leverPulled = false;

        SpriteRenderer _renderer;
        BoxCollider _collider;
        Interactable _interactable;

        Sprite _upSprite;
        Sprite _downSprite;

        public WallLever(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Tilesets.Dungeon_prison_props);
            _upSprite = new Sprite(texture, new Rectangle(64, 112, 16, 32));
            _downSprite = new Sprite(texture, new Rectangle(96, 112, 16, 32));
            _renderer = Entity.AddComponent(new SpriteRenderer());
            _renderer.SetSprite(_upSprite);

            _collider = Entity.AddComponent(new BoxCollider());
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);

            _interactable = Entity.AddComponent(new Interactable(OnInteracted));
        }

        IEnumerator OnInteracted()
        {
            if (CanBeInteractedWith)
            {
                if (!_leverPulled)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Gate_close);
                    _renderer.SetSprite(_downSprite);
                    OnLeverPulled?.Invoke();

                    _leverPulled = true;
                }
            }

            yield break;
        }
    }
}
