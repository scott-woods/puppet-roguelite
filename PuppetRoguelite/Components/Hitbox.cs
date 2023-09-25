using Nez;
using Nez.Systems;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class Hitbox : Component, ITriggerListener, IUpdatable
    {
        public Emitter<HitboxEventTypes, Collider> Emitter;

        Collider _collider;

        public Hitbox(Collider collider)
        {
            _collider = collider;
        }

        public override void Initialize()
        {
            base.Initialize();

            Emitter = new Emitter<HitboxEventTypes, Collider>();
        }

        public void Update()
        {
            var colliders = Physics.BoxcastBroadphase(_collider.Bounds);
            var damageCollider = colliders.FirstOrDefault(collider => collider.PhysicsLayer == (int)PhysicsLayers.Damage);
            if (damageCollider != null)
            {
                Emitter.Emit(HitboxEventTypes.Hit, damageCollider);
            }
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (int)PhysicsLayers.Damage)
            {
                Emitter.Emit(HitboxEventTypes.Hit, other);
            }
            //if (other.Entity.TryGetComponent<DamageComponent>(out var damageComponent))
            //{
            //    if (local.Entity.TryGetComponent<HealthComponent>(out var healthComponent))
            //    {
            //        healthComponent.DecrementHealth(damageComponent.Damage);
            //    }
            //}
        }

        public void OnTriggerExit(Collider other, Collider local)
        {

        }
    }

    public enum HitboxEventTypes
    {
        Hit = 1
    }
}
