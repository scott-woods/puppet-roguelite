using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class HurtState : State<PlayerController>
    {
        ITimer _timer;

        public override void Begin()
        {
            base.Begin();

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._64_Get_hit_03);

            //shake screen
            if (_context.Entity.Scene.Camera.Entity.TryGetComponent<CameraShake>(out var cameraShake))
            {
                cameraShake.Shake(20f, 3f);
            }

            //_timer = Game1.Schedule(.5f, timer =>
            //{
            //    _machine.ChangeState<IdleState>();
            //});
        }

        public override void Reason()
        {
            base.Reason();

            if (_context.Entity.TryGetComponent<KnockbackComponent>(out var knockbackComponent))
            {
                if (!knockbackComponent.IsStunned)
                    _machine.ChangeState<IdleState>();
            }
            else
                _machine.ChangeState<IdleState>();
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            //_timer.Stop();
        }
    }
}
