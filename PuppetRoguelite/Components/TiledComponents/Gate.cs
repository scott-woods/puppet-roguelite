﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
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
                    case "vertical":
                        var vTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Dungeon_prison_gate_vertical);
                        _renderer = Entity.AddComponent(new SpriteRenderer(vTexture));
                        _collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, 16));
                        _collider.SetLocalOffset(new Vector2(0, (TmxObject.Height / 2) - 8));
                        _origin = Entity.AddComponent(new OriginComponent(_collider));
                        _ySorter = Entity.AddComponent(new YSorter(_renderer, _origin));
                        break;
                    case "horizontal":
                        var hTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Dungeon_prison_gate_horizontal);
                        _renderer = Entity.AddComponent(new SpriteRenderer(hTexture));
                        _collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, TmxObject.Height));
                        if (TmxObject.Properties.TryGetValue("Flip", out var flip))
                        {
                            _renderer.FlipX = flip == "true";
                        }
                        _origin = Entity.AddComponent(new OriginComponent(_collider));
                        _ySorter = Entity.AddComponent(new YSorter(_renderer, _origin));
                        break;
                }
            }

            //_collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
            //Entity.AddComponent(_collider);
            _renderer.SetEnabled(false);
            _collider.SetEnabled(false);

            //TmxObject.Visible = false;
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
