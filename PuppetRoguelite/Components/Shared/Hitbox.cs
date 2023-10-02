using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Hitbox : Component
    {
        Collider _collider;
        int _damage;
        public int Damage
        {
            get { return _damage; }
        }

        public Hitbox(Collider collider, int damage)
        {
            _collider = collider;
            _damage = damage;
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider.IsTrigger = true;
        }

        public void Enable()
        {
            _collider.Enabled = true;
        }

        public void Disable()
        {
            _collider.Enabled = false;
        }
    }
}
