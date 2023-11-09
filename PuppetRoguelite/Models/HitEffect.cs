using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class HitEffect
    {
        public string AnimationPath;
        public int CellWidth;
        public int CellHeight;

        public HitEffect(string animationPath, int cellWidth, int cellHeight)
        {
            AnimationPath = animationPath;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
        }
    }
}
