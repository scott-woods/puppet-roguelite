using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class PlayerSpawnPoint : TiledComponent
    {
        public string Id { get; set; }

        public PlayerSpawnPoint(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
            if (tmxObject.Properties != null)
            {
                if (tmxObject.Properties.TryGetValue("Id", out var id))
                {
                    Id = id;
                }
            }
        }
    }
}
