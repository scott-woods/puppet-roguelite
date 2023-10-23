using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class PolygonHitbox : PolygonCollider, IHitbox
    {
        #region IHitbox

        int _damage;
        public int Damage { get => _damage; set => _damage = value; }

        float _pushForce;
        public float PushForce { get => _pushForce; set => _pushForce = value; }

        Vector2 _direction;
        public Vector2 Direction { get => _direction; set => _direction = value; }

        #endregion

        /// <summary>
		/// If the points are not centered they will be centered with the difference being applied to the localOffset.
		/// </summary>
		/// <param name="points">Points.</param>
		public PolygonHitbox(int damage, Vector2[] points) : base(points)
        {
            Damage = damage;
        }

        public PolygonHitbox(int damage, int vertCount, float radius) : base(vertCount, radius)
        {
            Damage = damage;
        }

        public PolygonHitbox(int damage) : base()
        {
            Damage = damage;
        }
    }
}
