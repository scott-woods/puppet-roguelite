using Nez;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Hitbox : Component
    {
        public Collider Collider;
        int _damage;

        public int Damage
        {
            get { return _damage; }
        }

        public Hitbox(Collider collider, int damage)
        {
            Collider = collider;
            _damage = damage;
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
