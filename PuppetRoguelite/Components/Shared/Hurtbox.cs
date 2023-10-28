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
        const float _attackLifespan = 2f;

        public Emitter<HurtboxEventTypes, HurtboxHit> Emitter = new Emitter<HurtboxEventTypes, HurtboxHit>();

        public bool IsInRecovery { get; set; }

        Collider _collider;

        float _recoveryTime;

        ITimer _recoveryTimer;

        List<string> _recentAttackIds = new List<string>();

        /// <summary>
        /// Recovery time is how long before entity can be hit again.
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
                var hitboxes = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
                foreach (IHitbox hitbox in hitboxes)
                {
                    if (IsInRecovery) break;
                    if (!_recentAttackIds.Contains(hitbox.AttackId))
                    {
                        var id = hitbox.AttackId;
                        _recentAttackIds.Add(id);
                        Game1.Schedule(_attackLifespan, timer => _recentAttackIds.Remove(id));
                        HandleHit(hitbox);
                    }
                }
            }
        }

        void HandleHit(IHitbox hitbox)
        {
            //play enemy hit sound
            if (Entity.HasComponent<Enemy>())
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._14_Impact_flesh_01, .5f);
            }

            //start recovery timer if necessary
            if (_recoveryTime > 0)
            {
                IsInRecovery = true;
                _recoveryTimer = Core.Schedule(_recoveryTime, timer =>
                {
                    IsInRecovery = false;
                });
            }

            //emit hit signal
            var collider = hitbox as Collider;
            if (collider.CollidesWith(_collider, out CollisionResult collisionResult))
            {
                Emitter.Emit(HurtboxEventTypes.Hit, new HurtboxHit(collisionResult, hitbox));
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
