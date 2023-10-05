using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
using Nez.Systems;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Hurtbox : Component, IUpdatable
    {
        public Emitter<HurtboxEventTypes, Hitbox> Emitter;

        Collider _collider;
        HealthComponent _healthComponent;
        int[] _damageLayers;

        float _recoveryTime;
        bool _inRecovery = false;

        public Hurtbox(Collider collider, float recoveryTime, int[] damageLayers)
        {
            _collider = collider;
            _recoveryTime = recoveryTime;
            _damageLayers = damageLayers;
        }

        public override void Initialize()
        {
            base.Initialize();

            Emitter = new Emitter<HurtboxEventTypes, Hitbox>();

            _collider.IsTrigger = true;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _healthComponent = Entity.GetComponent<HealthComponent>();
        }

        public void Update()
        {
            var colliders = Physics.BoxcastBroadphase(_collider.Bounds);
            //var damageCollider = colliders.FirstOrDefault(collider => collider.PhysicsLayer == (int)PhysicsLayers.EnemyDamage);
            var damageCollider = colliders.FirstOrDefault(c => _damageLayers.Contains(c.PhysicsLayer));
            if (damageCollider != null)
            {
                HandleHit(damageCollider);
            }
        }

        //public void OnTriggerEnter(Collider other, Collider local)
        //{
        //    if (other.PhysicsLayer == (int)PhysicsLayers.Damage)
        //    {
        //        HandleHit(other);
        //    }
        //}

        //public void OnTriggerExit(Collider other, Collider local)
        //{

        //}

        public void HandleHit(Collider damageCollider)
        {
            if (!_inRecovery)
            {
                if (damageCollider.Entity.TryGetComponent<Hitbox>(out var hitbox))
                {
                    _inRecovery = true;
                    Core.Schedule(_recoveryTime, timer => _inRecovery = false);
                    Emitter.Emit(HurtboxEventTypes.Hit, hitbox);
                }
            }
        }
    }

    public enum HurtboxEventTypes
    {
        Hit = 1
    }
}
