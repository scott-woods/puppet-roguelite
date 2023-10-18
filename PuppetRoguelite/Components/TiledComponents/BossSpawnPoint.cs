using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class BossSpawnPoint : TiledComponent
    {
        public BossSpawnPoint(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }
    }
}
