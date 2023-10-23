using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
using Nez.Systems;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Hurtbox : Component, IUpdatable
    {
        public Emitter<HurtboxEventTypes, HurtboxHit> Emitter = new Emitter<HurtboxEventTypes, HurtboxHit>();

        public bool IsInRecovery { get; set; }

        Collider _collider;

        float _recoveryTime;

        ITimer _recoveryTimer;

        /// <summary>
        /// Stun time is how long entity is frozen. Recovery time is how long before entity can be hit again.
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="stunTime"></param>
        /// <param name="recoveryTime"></param>
        public Hurtbox(Collider collider, float recoveryTime)
        {
            _collider = collider;
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
            if (Entity.HasComponent<Enemy>())
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._14_Impact_flesh_01, .5f);
            }

            if (_recoveryTime > 0)
            {
                IsInRecovery = true;
                _recoveryTimer = Core.Schedule(_recoveryTime, timer =>
                {
                    IsInRecovery = false;
                });
            }

            if (collider.CollidesWith(_collider, out CollisionResult collisionResult))
            {
                Emitter.Emit(HurtboxEventTypes.Hit, new HurtboxHit(collisionResult, collider as IHitbox));
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
