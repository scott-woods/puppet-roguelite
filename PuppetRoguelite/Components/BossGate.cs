using Nez;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class BossGate : Component
    {
        int _keysAdded = 0;

        Sprite _openSprite, _closedSprite;

        SpriteRenderer _renderer;
        Collider _collider;

        public override void Initialize()
        {
            base.Initialize();

            var bossGateTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Boss_gate);
            _openSprite = new Sprite(bossGateTexture, 32, 0, 32, 48);
            _closedSprite = new Sprite(bossGateTexture, 0, 0, 32, 48);

            _renderer = Entity.AddComponent(new SpriteRenderer());
            _renderer.SetSprite(_closedSprite);

            _collider = Entity.AddComponent(new BoxCollider());
        }

        public void AddKey()
        {
            _keysAdded += 1;

            if (_keysAdded == 2)
            {
                _renderer.SetSprite(_openSprite);
                _collider.SetEnabled(false);
            }
        }
    }
}
