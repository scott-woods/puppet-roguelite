using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.Substates
{
    public class IdleState : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            //handle animation
            var animation = "IdleDown";
            if (_context.VelocityComponent.Direction.X != 0)
            {
                animation = _context.VelocityComponent.Direction.X >= 0 ? "IdleRight" : "IdleLeft";
            }
            else if (_context.VelocityComponent.Direction.Y != 0)
            {
                animation = _context.VelocityComponent.Direction.Y >= 0 ? "IdleDown" : "IdleUp";
            }
            if (!_context.SpriteAnimator.IsAnimationActive(animation))
            {
                _context.SpriteAnimator.Play(animation);
            }
        }

        public override void Reason()
        {
            base.Reason();

            _context.TryTriggerTurn();
            _context.TryCheck();

            if (_context.CanMove())
            {
                if (_context.XAxisInput.Value != 0 || _context.YAxisInput.Value != 0)
                {
                    _machine.ChangeState<MoveState>();
                }
            }

            if (Input.LeftMouseButtonPressed)
            {
                _machine.ChangeState<AttackState>();
            }
        }
    }
}
