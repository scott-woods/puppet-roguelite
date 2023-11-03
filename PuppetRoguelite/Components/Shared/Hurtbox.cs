using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
using Nez.Sprites;
using Nez.Systems;
using Nez.Timers;
using Nez.Tweens;
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

        public Collider Collider;

        float _recoveryTime;

        ITimer _recoveryTimer;

        List<string> _recentAttackIds = new List<string>();

        string _damageSound;

        /// <summary>
        /// Recovery time is how long before entity can be hit again.
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="stunTime"></param>
        /// <param name="recoveryTime"></param>
        public Hurtbox(Collider collider, float recoveryTime, string damageSound = "")
        {
            Collider = collider;
            _recoveryTime = recoveryTime;
            _damageSound = damageSound;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            if (Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            }
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            Collider.SetEnabled(true);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Collider.SetEnabled(false);
        }

        public void Update()
        {
            if (!IsInRecovery)
            {
                //var renderer = Entity.GetComponent<SpriteRenderer>();
                //renderer.Color = Color.White;

                var hitboxes = Physics.BoxcastBroadphaseExcludingSelf(Collider, Collider.CollidesWithLayers);
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
            if (!String.IsNullOrWhiteSpace(_damageSound))
            {
                Game1.AudioManager.PlaySound(_damageSound, .5f);
            }

            //start recovery timer if necessary
            if (_recoveryTime > 0)
            {
                IsInRecovery = true;
                //var renderer = Entity.GetComponent<SpriteRenderer>();
                //renderer.Color = Color.White * .5f;
                _recoveryTimer = Core.Schedule(_recoveryTime, timer =>
                {
                    //renderer.Color = Color.White;
                    IsInRecovery = false;
                });
            }

            //emit hit signal
            var collider = hitbox as Collider;
            if (collider.CollidesWith(Collider, out CollisionResult collisionResult))
            {
                Emitter.Emit(HurtboxEventTypes.Hit, new HurtboxHit(collisionResult, hitbox));
            }
        }

        void OnHealthDepleted(HealthComponent hc)
        {
            SetEnabled(false);
        }
    }

    public enum HurtboxEventTypes
    {
        Hit
    }
}
