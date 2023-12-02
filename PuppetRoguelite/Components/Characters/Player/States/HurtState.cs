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

            //var hurtAnimation = _context.VelocityComponent.Direction.X >= 0 ? "HurtRight" : "HurtLeft";
            _context.SpriteAnimator.Play("Idle");

            _timer = Game1.Schedule(.5f, timer =>
            {
                _machine.ChangeState<IdleState>();
            });
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            _timer.Stop();
        }
    }
}
