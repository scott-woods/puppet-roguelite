using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
using Nez.Sprites;
using Nez.Systems;
using Nez.Timers;
using Nez.Tweens;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Effects;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class Hurtbox : Component, IUpdatable
    {
        //emitter
        public Emitter<HurtboxEventTypes, HurtboxHit> Emitter = new Emitter<HurtboxEventTypes, HurtboxHit>();

        //constants
        const float _attackLifespan = 2f;

        //components
        public Collider Collider;

        //misc
        bool _isInRecovery;
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
            Collider.IsTrigger = true;
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
            //only check for collisions if not in recovery
            if (!_isInRecovery)
            {
                //get collisions
                var hitboxes = Physics.BoxcastBroadphaseExcludingSelf(Collider, Collider.CollidesWithLayers);
                foreach (IHitbox hitbox in hitboxes)
                {
                    //if hit by something else already, break for loop
                    if (_isInRecovery) break;

                    //if hasn't already been hit by this hitbox's attack
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
            //play sound
            if (!string.IsNullOrWhiteSpace(_damageSound))
                Game1.AudioManager.PlaySound(_damageSound);

            //start recovery timer if necessary
            if (_recoveryTime > 0)
            {
                //animator should be slightly transparent while in recovery
                if (Entity.TryGetComponent<SpriteAnimator>(out var animator))
                    animator.SetColor(new Color(255, 255, 255, 128));

                _isInRecovery = true;
                _recoveryTimer = Core.Schedule(_recoveryTime, timer =>
                {
                    if (Entity.TryGetComponent<SpriteAnimator>(out var animator))
                        animator.SetColor(new Color(255, 255, 255, 255));

                    _isInRecovery = false;
                });
            }

            //get collision result
            var collider = hitbox as Collider;
            if (collider.CollidesWith(Collider, out CollisionResult collisionResult))
            {
                //get angle from normal
                var angle = (float)Math.Atan2(collisionResult.Normal.Y, collisionResult.Normal.X);

                //choose hit effect
                var effects = new List<HitEffect>() { HitEffects.Hit1, HitEffects.Hit2, HitEffects.Hit3 };
                var effect = effects.RandomItem();

                //effect color
                Color color = Color.White;
                if (Entity == PlayerController.Instance.Entity)
                    color = Color.Red;

                //hit effect
                var effectEntity = Entity.Scene.CreateEntity("hit-effect", collisionResult.Point);
                effectEntity.SetRotation(angle);
                var effectComponent = effectEntity.AddComponent(new HitEffectComponent(effect, color));

                //emit hit signal
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
