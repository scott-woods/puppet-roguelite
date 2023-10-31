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
        OriginComponent _originComponent;

        public YSorter(IRenderable renderable, OriginComponent originComponent)
        {
            _renderable = renderable;
            _originComponent = originComponent;
        }

        public void Update()
        {
            if (_renderable != null)
            {
                _renderable.RenderLayer = -(int)(_originComponent.Origin.Y);
            }
        }

        public void SetOriginComponent(OriginComponent originComponent)
        {
            _originComponent = originComponent;
        }
    }
}
