using Nez;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class Melee : Component
    {
        Collider _collider;
        int _damage;
        DamageComponent _damageComponent;

        public Melee(Collider collider, int damage)
        {
            _collider = collider;
            _damage = damage;
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(_collider);
            _collider.IsTrigger = true;
            _collider.PhysicsLayer = (int)PhysicsLayers.Damage;
            //_collider.SetEnabled(false);

            _damageComponent = Entity.AddComponent(new DamageComponent(_damage));
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
