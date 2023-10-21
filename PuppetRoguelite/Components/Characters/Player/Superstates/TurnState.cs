using Nez.AI.FSM;
using PuppetRoguelite.Components.Characters.Player.Substates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.Superstates
{
    public class TurnState : State<PlayerController>
    {
        StateMachine<PlayerController> _subStateMachine;

        public override void OnInitialized()
        {
            base.OnInitialized();

            _subStateMachine = new StateMachine<PlayerController>(_context, new IdleState());
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }
    }
}
