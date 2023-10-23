using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Hitbox : CircleCollider
    {
        public Collider Collider;
        public int Damage;
        public float PushForce = 1f;
        public Vector2 Direction = Vector2.Zero;

        public Hitbox(Collider collider, int damage)
        {
            Collider = collider;
            Damage = damage;
        }

        public override void Initialize()
        {
            base.Initialize();

            Collider.IsTrigger = true;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            Collider.SetEnabled(true);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Collider.SetEnabled(false);
        }
    }
}
