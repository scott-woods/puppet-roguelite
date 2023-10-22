using Nez.AI.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class DashState : State<PlayerController>
    {
        public override void Begin()
        {
            base.Begin();

            _context.ExecuteDash(OnDashCompleted);
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        void OnDashCompleted()
        {
            _machine.ChangeState<IdleState>();
        }
    }
}
