using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Shadow : Component, IUpdatable
    {
        //components
        SpriteRenderer _renderer;

        //passed in components
        SpriteRenderer _baseRenderer;

        //entity
        Entity _shadowEntity;

        //misc
        Vector2 _offset;
        Vector2 _scale;

        public Shadow(SpriteRenderer baseRenderer, Vector2 offset, Vector2 scale)
        {
            _baseRenderer = baseRenderer;
            _offset = offset;
            _scale = scale;
        }

        public override void Initialize()
        {
            base.Initialize();

            //create separate entity for shadow and parent it to main entity
            //_shadowEntity = Entity.Scene.CreateEntity("shadow");
            //_shadowEntity.SetParent(Entity);

            //create renderer and add it to shadow entity
            var texture = Game1.Content.LoadTexture(Nez.Content.Textures.Effects.Shadow);
            _renderer = Entity.AddComponent(new SpriteRenderer(texture));
            _renderer.SetLocalOffset(_offset);
            //_renderer.Transform.SetLocalScale(_scale);
            _renderer.SetRenderLayer(_baseRenderer.RenderLayer + 1);
        }

        public void Update()
        {
            _renderer.SetRenderLayer(_baseRenderer.RenderLayer + 1);
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            _renderer?.SetEnabled(true);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            _renderer?.SetEnabled(false);
        }
    }
}
