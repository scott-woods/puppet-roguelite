using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class SturdyComponent : Component
    {
        //emitter
        public Emitter<SturdyComponentEvents> Emitter = new Emitter<SturdyComponentEvents>();

        //misc
        int _hitsUntilTriggered;
        float _duration;
        float _hitLifespan;
        int _hitCount;
        bool _active;

        public SturdyComponent(int hitsUntilTriggered, float duration, float hitLifespan = 3f)
        {
            _hitsUntilTriggered = hitsUntilTriggered;
            _duration = duration;
            _hitLifespan = hitLifespan;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<Hurtbox>(out var hurtbox))
            {
                hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHurtboxHit);
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            if (Entity.TryGetComponent<Hurtbox>(out var hurtbox))
            {
                hurtbox.Emitter.RemoveObserver(HurtboxEventTypes.Hit, OnHurtboxHit);
            }
        }

        void OnHurtboxHit(HurtboxHit hit)
        {
            //only increment if not already active
            if (!_active)
            {
                //increment counter
                _hitCount++;

                //start timer to remove hit
                Game1.Schedule(_hitLifespan, timer =>
                {
                    _hitCount = Math.Max(_hitCount - 1, 0);
                });

                //activate if reached needed hits
                if (_hitCount >= _hitsUntilTriggered)
                {
                    ActivateSturdy();
                }
            }
        }

        void ActivateSturdy()
        {
            //set active to true
            _active = true;

            //start timer to deactivate
            Game1.Schedule(_duration, OnTimerFinished);

            //emit signal
            Emitter.Emit(SturdyComponentEvents.SturdyActivated);
        }

        private void OnTimerFinished(ITimer timer)
        {
            //set active to false
            _active = false;

            //reset hit counter
            _hitCount = 0;

            //emit signal
            Emitter.Emit(SturdyComponentEvents.SturdyDeactivated);
        }
    }

    public enum SturdyComponentEvents
    {
        SturdyActivated,
        SturdyDeactivated,
    }
}
