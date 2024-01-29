using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Helpers;
using PuppetRoguelite.Models;
using PuppetRoguelite.StaticData;
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
            var dir = Controls.Instance.DirectionalInput.Value;
            //var dir = new Vector2(Controls.Instance.XAxisInput.Value, Controls.Instance.YAxisInput.Value);
            dir.Normalize();
            _context.VelocityComponent.SetDirection(dir);

            //animation
            string animation = "";
            if (dir.Y < 0 && Math.Abs(dir.X) < .1f)
                animation = "RunUp";
            else if (dir.Y > 0 && Math.Abs(dir.X) < .1f)
                animation = "RunDown";
            else
                animation = "Run";

            if (!_context.SpriteAnimator.IsAnimationActive(animation))
            {
                _context.SpriteAnimator.Play(animation);
            }

            //move
            _context.VelocityComponent.Move(_context.MoveSpeed);
        }

        public override void Reason()
        {
            base.Reason();

            if (TryShowStats())
                return;
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
