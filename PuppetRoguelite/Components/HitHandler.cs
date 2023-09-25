using Nez;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class HitHandler : Component
    {
        public Emitter<HitHandlerEventType> Emitter;

        public override void Initialize()
        {
            base.Initialize();

            Emitter = new Emitter<HitHandlerEventType>();
        }

        public void OnHurtboxHit(Hitbox hitbox)
        {
            if (Entity.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.DecrementHealth(hitbox.Damage);

                if (!healthComponent.IsDepleted())
                {
                    Emitter.Emit(HitHandlerEventType.Hurt);
                }
                else
                {
                    Emitter.Emit(HitHandlerEventType.Killed);
                }
            }
        }
    }

    public enum HitHandlerEventType
    {
        Hurt = 1,
        Killed = 2
    }
}
