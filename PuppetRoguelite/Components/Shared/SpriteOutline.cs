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
    public class SpriteOutline : RenderableComponent
    {
        public override float Width => _width;
        public override float Height => _height;

        float _width;
        float _height;

        SpriteRenderer _renderer;
        Color _color;
        int _offset;

        public SpriteOutline(SpriteRenderer renderer, Color color, int offset = 1)
        {
            _renderer = renderer;
            _color = color;
            _offset = offset;

            _width = _renderer.Width;
            _height = _renderer.Height;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            _renderer.DrawOutline(batcher, camera, _color, _offset);
        }
    }
}
