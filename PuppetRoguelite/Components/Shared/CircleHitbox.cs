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
    public class CircleHitbox : CircleCollider, IHitbox
    {
        #region IHitbox

        int _damage;
        public int Damage { get => _damage; set => _damage = value; }

        float _pushForce;
        public float PushForce { get => _pushForce; set => _pushForce = value; }

        Vector2 _direction;
        public Vector2 Direction { get => _direction; set => _direction = value; }

        #endregion

        public CircleHitbox(int damage) : base()
        {
            Damage = damage;
        }

        public CircleHitbox(int damage, float radius) : base(radius)
        {
            Damage = damage;
        }

        public override void Initialize()
        {
            base.Initialize();

            IsTrigger = true;
        }
    }
}
