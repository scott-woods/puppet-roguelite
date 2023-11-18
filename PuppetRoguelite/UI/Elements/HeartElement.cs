using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class HeartElement : Image
    {
        List<SpriteDrawable> _sprites;

        public HeartElement(List<SpriteDrawable> sprites, int fillLevel) : base(sprites[fillLevel])
        {
            _sprites = sprites;

            SetScaleX(Game1.ResolutionScale.X * 1.25f);
            SetScaleY(Game1.ResolutionScale.Y * 1.25f);
        }

        /// <summary>
        /// set the heart image by fill level
        /// </summary>
        /// <param name="fillLevel"></param>
        public void SetFillLevel(int fillLevel)
        {
            fillLevel = Math.Clamp(fillLevel, 0, _sprites.Count - 1);
            SetDrawable(_sprites[fillLevel]);
        }
    }
}
