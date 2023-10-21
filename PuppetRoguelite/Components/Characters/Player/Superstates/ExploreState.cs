using Nez.AI.FSM;
using PuppetRoguelite.Components.Characters.Player.Substates;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.Superstates
{
    public class ExploreState : State<PlayerController>
    {
        StateMachine<PlayerController> _subStateMachine;

        public override void OnInitialized()
        {
            base.OnInitialized();

            _subStateMachine = new StateMachine<PlayerController>(_context, new IdleState());
            _subStateMachine.AddState(new MoveState());
            _subStateMachine.AddState(new AttackState());
            _subStateMachine.AddState(new HurtState());
            _subStateMachine.AddState(new DyingState());
        }

        public override void Begin()
        {
            base.Begin();

            _context.HealthComponent.Emitter.AddObserver(Shared.HealthComponentEventType.DamageTaken, OnDamageTaken);
            _context.HealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
        }

        public override void Update(float deltaTime)
        {
            _subStateMachine.Update(deltaTime);
        }

        public override void End()
        {
            base.End();

            _context.HealthComponent.Emitter.RemoveObserver(Shared.HealthComponentEventType.DamageTaken, OnDamageTaken);
            _context.HealthComponent.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
        }

        void OnDamageTaken(HealthComponent healthComponent)
        {
            _subStateMachine.ChangeState<HurtState>();
        }

        void OnHealthDepleted(HealthComponent healthComponent)
        {
            _subStateMachine.ChangeState<DyingState>();
        }
    }
}
