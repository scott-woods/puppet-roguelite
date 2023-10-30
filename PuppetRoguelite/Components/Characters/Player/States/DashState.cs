using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Enums;
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

            //allow movement through enemies
            Flags.UnsetFlag(ref _context.Collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);

            //disable hurtbox
            _context.Hurtbox.SetEnabled(false);

            _context.ExecuteDash(OnDashCompleted);
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            //reenable collisions with enemies
            Flags.SetFlag(ref _context.Collider.CollidesWithLayers, (int)PhysicsLayers.EnemyCollider);

            //reenable hurtbox
            _context.Hurtbox.SetEnabled(true);

            _context.Dash.Abort();
        }

        void OnDashCompleted()
        {
            _machine.ChangeState<IdleState>();
        }
    }
}
