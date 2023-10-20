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

        ITimer _recoveryTimer;
        ITimer _stunTimer;

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
                    HandleHit(colliders.First());
                }
            }
        }

        void HandleHit(Collider collider)
        {
            if (collider.Entity.TryGetComponent<Hitbox>(out var hitbox))
            {
                if (_recoveryTime > 0)
                {
                    IsInRecovery = true;
                    _recoveryTimer = Core.Schedule(_recoveryTime, timer =>
                    {
                        IsInRecovery = false;
                    });
                }
                if (_stunTime > 0)
                {
                    //if already stunned, reset stun timer and stun again
                    if (IsStunned)
                    {
                        _stunTimer.Reset();
                    }
                    else
                    {
                        IsStunned = true;
                        _stunTimer = Core.Schedule(_stunTime, timer =>
                        {
                            IsStunned = false;
                        });
                    }
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
