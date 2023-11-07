using Microsoft.Xna.Framework;
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
    public class MoveState : PlayerState
    {
        public override void Update(float deltaTime)
        {
            //set movement direction
            var dir = new Vector2(_context.XAxisInput.Value, _context.YAxisInput.Value);
            dir.Normalize();
            _context.VelocityComponent.SetDirection(dir);

            //animation
            var animation = "RunDown";
            if (_context.VelocityComponent.Direction.X != 0)
            {
                animation = _context.VelocityComponent.Direction.X >= 0 ? "RunRight" : "RunLeft";
            }
            else if (_context.VelocityComponent.Direction.Y != 0)
            {
                animation = _context.VelocityComponent.Direction.Y >= 0 ? "RunDown" : "RunUp";
            }
            if (!_context.SpriteAnimator.IsAnimationActive(animation))
            {
                _context.SpriteAnimator.Play(animation);
            }

            //move
            _context.VelocityComponent.Move();
        }

        public override void Reason()
        {
            base.Reason();

            if (TryTriggerTurn())
                return;
            if (TryCheck())
                return;
            if (TryMelee())
                return;
            if (TryDash())
                return;
            if (TryIdle())
                return;
        }
    }
}
