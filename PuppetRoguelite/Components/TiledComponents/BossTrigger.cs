using Nez;
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
        public BossTrigger(TmxObject tmxTriggerObject, Entity mapEntity) : base(tmxTriggerObject, mapEntity)
        {
        }

        public override void HandleTriggered()
        {
            Entity.Destroy();
        }
    }
}
