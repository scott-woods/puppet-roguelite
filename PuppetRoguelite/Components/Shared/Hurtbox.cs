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
                    if (colliders.First().Entity.TryGetComponent<Hitbox>(out var hitbox))
                    {
                        if (_recoveryTime > 0)
                        {
                            IsInRecovery = true;
                            Core.Schedule(_recoveryTime, timer => IsInRecovery = false);
                        }
                        if (_stunTime > 0)
                        {
                            IsStunned = true;
                            Core.Schedule(_stunTime, timer => IsStunned = false);
                        }

                        Emitter.Emit(HurtboxEventTypes.Hit, hitbox);
                    }
                }
            }
        }
    }

    public enum HurtboxEventTypes
    {
        Hit
    }
}
