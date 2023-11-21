using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class CustomStage : Stage
    {
        public override Vector2 GetMousePosition()
        {
            return Entity != null && !IsFullScreen ? Input.ScaledMousePosition : Input.ScaledMousePosition * Game1.ResolutionScale;
        }
    }
}
