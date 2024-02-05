using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class Gate : TiledComponent
    {
        Collider _collider;
        SpriteRenderer _renderer;
        OriginComponent _origin;
        YSorter _ySorter;
        DungeonDoorway _parentDoorway;

        public Gate(TmxObject gateObj, Entity mapEntity) : base(gateObj, mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            if (TmxObject.Properties.TryGetValue("Orientation", out var orientation))
            {
                switch (orientation)
                {
                    case "Horizontal":
                        var hTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Dungeon_prison_gate_horizontal);
                        var hSprite = new NinePatchSprite(hTexture, new Rectangle(0, 0, (int)TmxObject.Width, (int)TmxObject.Height), 16, 16, 0, 0);
                        var nineSliceRenderer = new NineSliceSpriteRenderer(hSprite);
                        _renderer = Entity.AddComponent(nineSliceRenderer);
                        nineSliceRenderer.Width = TmxObject.Width;
                        nineSliceRenderer.Height = hTexture.Height;
                        nineSliceRenderer.SetLocalOffset(new Vector2(-TmxObject.Width / 2, -TmxObject.Height / 2));
                        _collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, 16));
                        _collider.SetLocalOffset(new Vector2(0, (TmxObject.Height / 2) - 8));
                        _origin = Entity.AddComponent(new OriginComponent(new Vector2(0, (TmxObject.Height / 2) - 8)));
                        _ySorter = Entity.AddComponent(new YSorter(_renderer, _origin));
                        break;
                    case "Vertical":
                        var vTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Dungeon_prison_gate_vertical);
                        var vSprite = new NinePatchSprite(vTexture, new Rectangle(0, 0, (int)TmxObject.Width, (int)TmxObject.Height), 0, 0, 16, 16);
                        var hNineSliceRenderer = new NineSliceSpriteRenderer(vSprite);
                        _renderer = Entity.AddComponent(hNineSliceRenderer);
                        hNineSliceRenderer.Width = vTexture.Width;
                        hNineSliceRenderer.Height = TmxObject.Height;
                        hNineSliceRenderer.SetLocalOffset(new Vector2(-TmxObject.Width / 2, -TmxObject.Height / 2));
                        _collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, TmxObject.Height));
                        if (TmxObject.Properties.TryGetValue("Flip", out var flip))
                            _renderer.FlipX = true;
                        _origin = Entity.AddComponent(new OriginComponent(new Vector2(0, (TmxObject.Height / 2) - 8)));
                        _ySorter = Entity.AddComponent(new YSorter(_renderer, _origin));
                        break;
                }
            }

            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.Environment);

            //_collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
            //Entity.AddComponent(_collider);
            _renderer.SetEnabled(false);
            _collider.SetEnabled(false);

            //TmxObject.Visible = false;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<DungeonDoorway>(out var doorway))
            {
                _parentDoorway = doorway;
                _parentDoorway.Gate = this;
            }
        }

        public void Lock()
        {
            _collider.SetEnabled(true);
            _renderer.SetEnabled(true);
            //TmxObject.Visible = true;
        }

        public void Unlock()
        {
            _collider.SetEnabled(false);
            _renderer.SetEnabled(false);
            //TmxObject.Visible = false;
        }
    }
}
