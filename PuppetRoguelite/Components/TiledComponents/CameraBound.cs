using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class CameraBound : TiledComponent
    {
        public CameraBound(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
            Debug.Log($"camera bound created");
        }
    }
}
