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
        /// <summary>
        /// the tmx object this was made from
        /// </summary>
        public TmxObject TmxObject { get; set; }

        /// <summary>
        /// the entity of the map that this belongs to
        /// </summary>
        public Entity MapEntity { get; set; }

        public TiledComponent(TmxObject tmxObject, Entity mapEntity)
        {
            TmxObject = tmxObject;
            MapEntity = mapEntity;
        }
    }
}
