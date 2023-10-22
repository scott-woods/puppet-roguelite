using Nez.AI.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class TurnState : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            _context.Idle();
        }
    }
}
