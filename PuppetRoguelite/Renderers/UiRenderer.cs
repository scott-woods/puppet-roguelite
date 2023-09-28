using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Renderers
{
    public class UiRenderer : ScreenSpaceRenderer
    {
        public UiRenderer(int renderOrder, params int[] renderLayers) : base(renderOrder, renderLayers)
        {
        }
    }
}
