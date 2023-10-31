using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class OriginComponent : Component
    {
        public Vector2 Origin
        {
            get
            {
                if (_collider != null)
                {
                    return _collider.Bounds.Center;
                }
                else return Entity.Position + _offset;
            }
        }

        Vector2 _offset;
        Collider _collider;

        /// <summary>
        /// constructor that takes an offset, this will be added to the entity's position to get the Origin
        /// </summary>
        /// <param name="offset"></param>
        public OriginComponent(Vector2 offset)
        {
            _offset = offset;
        }

        /// <summary>
        /// take a collider and use its center as the origin
        /// </summary>
        /// <param name="collider"></param>
        public OriginComponent(Collider collider)
        {
            _collider = collider;
        }
    }
}
