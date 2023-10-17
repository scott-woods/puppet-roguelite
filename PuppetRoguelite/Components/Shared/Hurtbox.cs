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

        Collider _collider;

        float _recoveryTime;
        bool _inRecovery = false;

        public Hurtbox(Collider collider, float recoveryTime)
        {
            _collider = collider;
            _recoveryTime = recoveryTime;
        }

        public void Update()
        {
            if (!_inRecovery)
            {
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    if (colliders.First().Entity.TryGetComponent<Hitbox>(out var hitbox))
                    {
                        _inRecovery = true;
                        Core.Schedule(_recoveryTime, timer => _inRecovery = false);
                        Emitter.Emit(HurtboxEventTypes.Hit, hitbox);
                    }
                }
            }
        }
    }

    public enum HurtboxEventTypes
    {
        Hit = 1
    }
}
