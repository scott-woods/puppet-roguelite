using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Entities;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    /// <summary>
    /// Handles the flow of combat in a scene
    /// </summary>
    public class CombatManager : SceneComponent
    {
        protected CombatManagerStateMachine StateMachine;

        Stack<Component> _menuStack = new Stack<Component>();

        public CombatManager()
        {
            StateMachine = new CombatManagerStateMachine(this, new DodgePhase());
            Game1.GameEventsEmitter.AddObserver(GameEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
        }

        public override void Update()
        {
            base.Update();

            StateMachine.Update(Time.DeltaTime);
        }

        public void OnTurnPhaseTriggered()
        {
            //freeze entities
            foreach(var entity in Scene.EntitiesOfType<PausableEntity>())
            {
                entity.TogglePaused();
            }

            //setup menus
            var turnMenuEntity = Scene.CreateEntity("turn-menu");
            var attacksMenu = turnMenuEntity.AddComponent(new AttacksMenu(Player.Instance.Entity.Position, this));
            var actionSelector = turnMenuEntity.AddComponent(new ActionsSelector(Player.Instance.Entity.Position, this, attacksMenu, true));
            OpenMenu(actionSelector);

            //change to turn phase
            StateMachine.ChangeState<TurnPhase>();
        }

        public void OpenMenu(Component component)
        {
            if (_menuStack.Count > 0)
            {
                _menuStack.Peek().SetEnabled(false);
            }
            _menuStack.Push(component);
            component.SetEnabled(true);
        }

        public void GoBack()
        {
            if (_menuStack.Count > 1)
            {
                var currentMenu = _menuStack.Pop();
                currentMenu.SetEnabled(false);
                _menuStack.Peek().SetEnabled(true);
            }
        }
    }

    #region STATE MACHINE

    public class CombatManagerStateMachine : StateMachine<CombatManager>
    {
        public CombatManagerStateMachine(CombatManager context, State<CombatManager> initialState) : base(context, initialState)
        {
            AddState(new DodgePhase());
            AddState(new TurnPhase());
        }
    }

    public class DodgePhase : State<CombatManager>
    {
        public override void Update(float deltaTime)
        {

        }
    }

    public class TurnPhase : State<CombatManager>
    {
        VirtualButton _cancelInput;

        public override void Begin()
        {
            base.Begin();

            _cancelInput = new VirtualButton();
            _cancelInput.AddKeyboardKey(Keys.X);
        }

        public override void Update(float deltaTime)
        {
            if (_cancelInput.IsPressed)
            {
                _context.GoBack();
            }
        }
    }

    #endregion
}
