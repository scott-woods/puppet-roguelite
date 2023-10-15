using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class YSorter : Component, IUpdatable
    {
        IRenderable _renderable;
        int _offset;

        public YSorter(IRenderable renderable, int offset)
        {
            _renderable = renderable;
            _offset = offset;
        }

        public void Update()
        {
            if (_renderable != null)
            {
                _renderable.RenderLayer = -(int)(Entity.Position.Y + _offset);
            }
        }

        public void SetOffset(int offset)
        {
            _offset = offset;
        }
    }
}
