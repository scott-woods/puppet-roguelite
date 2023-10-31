using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class BossGate : TiledComponent
    {
        int _keysAdded = 0;

        Sprite _openSprite, _closedSprite;

        SpriteRenderer _renderer;
        Collider _collider;
        ExitArea _exitArea;

        public BossGate(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            var bossGateTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Objects.Boss_gate);
            _openSprite = new Sprite(bossGateTexture, 32, 0, 32, 48);
            _closedSprite = new Sprite(bossGateTexture, 0, 0, 32, 48);

            _collider = Entity.AddComponent(new BoxCollider());
            _collider.SetEnabled(true);

            _renderer = Entity.AddComponent(new SpriteRenderer());
            _renderer.SetSprite(_closedSprite);
            _renderer.RenderLayer = -(int)_collider.Bounds.Center.Y;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _exitArea = Entity.Scene.FindComponentsOfType<ExitArea>().Where(e => e.MapEntity == MapEntity).FirstOrDefault();
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
