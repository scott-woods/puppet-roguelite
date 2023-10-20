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
        public Emitter<HurtboxEventTypes, Hitbox> Emitter = new Emitter<HurtboxEventTypes, Hitbox>();

        public bool IsStunned { get; set; }
        public bool IsInRecovery { get; set; }

        Collider _collider;

        float _recoveryTime;
        float _stunTime;

        /// <summary>
        /// Stun time is how long entity is frozen. Recovery time is how long before entity can be hit again.
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="stunTime"></param>
        /// <param name="recoveryTime"></param>
        public Hurtbox(Collider collider, float stunTime, float recoveryTime)
        {
            _collider = collider;
            _stunTime = stunTime;
            _recoveryTime = recoveryTime;
        }

        public void Update()
        {
            if (!IsInRecovery)
            {
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    Debug.Log("colliders found: " + colliders.Count.ToString());
                    HandleHit(colliders.First());
                }
            }
        }

        void HandleHit(Collider collider)
        {
            Debug.Log(collider.Entity.Name);
            if (collider.Entity.TryGetComponent<Hitbox>(out var hitbox))
            {
                Debug.Log(hitbox.Entity.Name);
                if (_recoveryTime > 0)
                {
                    Debug.Log("recovery time not 0");
                    IsInRecovery = true;
                    Core.Schedule(_recoveryTime, timer =>
                    {
                        Debug.Log("recovery ended");
                        IsInRecovery = false;
                    });
                }
                if (_stunTime > 0)
                {
                    Debug.Log("stun time not 0");
                    IsStunned = true;
                    Core.Schedule(_stunTime, timer =>
                    {
                        Debug.Log("stun ended");
                        IsStunned = false;
                    });
                }

                Emitter.Emit(HurtboxEventTypes.Hit, hitbox);
            }
        }

        //public void OnTriggerEnter(Collider other, Collider local)
        //{
        //    if (!IsInRecovery)
        //    {
        //        HandleHit(other);
        //    }
        //}

        //public void OnTriggerExit(Collider other, Collider local)
        //{
        //    //throw new NotImplementedException();
        //}
    }

    public enum HurtboxEventTypes
    {
        Hit
    }
}
