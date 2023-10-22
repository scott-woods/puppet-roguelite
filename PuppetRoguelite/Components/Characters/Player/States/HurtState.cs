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
        public override void Begin()
        {
            base.Begin();

            var hurtAnimation = _context.VelocityComponent.Direction.X >= 0 ? "HurtRight" : "HurtLeft";
            _context.SpriteAnimator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            _context.SpriteAnimator.OnAnimationCompletedEvent += HandleAnimationCompleted;
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            _context.SpriteAnimator.OnAnimationCompletedEvent -= HandleAnimationCompleted;
        }

        void HandleAnimationCompleted(string animationName)
        {
            if (animationName == "HurtRight" || animationName == "HurtLeft")
            {
                _machine.ChangeState<IdleState>();
            }
        }
    }
}
