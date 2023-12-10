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
    public class IdleState : PlayerState
    {
        public override void Update(float deltaTime)
        {
            _context.Idle();
        }

        public override void Reason()
        {
            base.Reason();

            if (TryTriggerTurn()) return;
            if (TryShowStats()) return;
            if (TryCheck()) return;
            if (TryMove()) return;
            if (TryMelee()) return;
        }
    }
}
