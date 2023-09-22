using Nez;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class Hitbox : Component, ITriggerListener
    {
        public Emitter<HitboxEventTypes, Collider> Emitter;

        public override void Initialize()
        {
            base.Initialize();

            Emitter = new Emitter<HitboxEventTypes, Collider>();
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            Emitter.Emit(HitboxEventTypes.Hit, other);
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
