using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class BossTrigger : Trigger
    {
        public BossTrigger(TmxObject tmxTriggerObject, string mapId) : base(tmxTriggerObject, mapId)
        {
        }

        public override void HandleTriggered()
        {
            Entity.Destroy();
        }
    }
}
