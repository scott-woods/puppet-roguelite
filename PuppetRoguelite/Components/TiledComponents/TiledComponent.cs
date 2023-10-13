using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    /// <summary>
    /// base class for any component deriving from Tiled
    /// </summary>
    public abstract class TiledComponent : Component
    {
        public TmxObject TmxObject { get; set; }
        public string MapId { get; set; }

        public TiledComponent(TmxObject tmxObject, string mapId)
        {
            TmxObject = tmxObject;
            MapId = mapId;
        }
    }
}
