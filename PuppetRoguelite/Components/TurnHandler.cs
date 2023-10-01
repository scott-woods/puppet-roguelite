using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Systems;
using PuppetRoguelite.Components.Actions;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class TurnHandler : Component, IUpdatable
    {
        protected TurnHandlerStateMachine StateMachine;
        
        //menus
        Stack<UIMenu> _menuStack = new Stack<UIMenu>();
        AttacksMenu _attacksMenu;
        ActionsSelector _actionsSelector;

        Queue<IPlayerAction> _actionQueue = new Queue<IPlayerAction>();

        CombatManager _combatManager;

        public TurnHandler(CombatManager combatManager)
        {
            _combatManager = combatManager;
        }

        public override void Initialize()
        {
            base.Initialize();

            //state machine
            StateMachine = new TurnHandlerStateMachine(this, new SelectingFromMenu());

            //setup menus
            var turnMenuEntity = Entity.Scene.CreateEntity("turn-menu");
            _attacksMenu = turnMenuEntity.AddComponent(new AttacksMenu(Player.Instance.Entity.Position, this));
            _actionsSelector = turnMenuEntity.AddComponent(new ActionsSelector(Player.Instance.Entity.Position, this, _attacksMenu, true));
            OpenMenu(_actionsSelector);

            //observe emitter
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionFinishedPreparing, OnActionFinishedPreparing);
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionFinishedExecuting, OnActionFinishedExecuting);
        }

        public void Update()
        {
            StateMachine.Update(Time.DeltaTime);
        }

        public void OpenMenu(UIMenu menu)
        {
            if (_menuStack.Count > 0)
            {
                _menuStack.Peek().SetEnabled(false);
            }
            _menuStack.Push(menu);
            menu.SetEnabled(true);
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

        public void HandleActionSelected(Type actionType)
        {
            _menuStack.Peek().SetEnabled(false);
            StateMachine.ChangeState<PreparingAction>();
            var action = Activator.CreateInstance(actionType) as IPlayerAction;
            action.Prepare();
        }

        public void OnActionFinishedPreparing(IPlayerAction action)
        {
            //action will be null if cancelled
            if (action == null)
            {
                _menuStack.Peek().SetEnabled(true);
                StateMachine.ChangeState<SelectingFromMenu>();
            }
            else
            {
                _actionQueue.Enqueue(action);
                _menuStack.Clear();
                Core.Schedule(.1f, timer =>
                {
                    OpenMenu(_actionsSelector);
                    StateMachine.ChangeState<SelectingFromMenu>();
                });
            }
        }

        public void ExecuteActions()
        {
            //destroy ui menus
            _actionsSelector.Entity.Destroy();
            _attacksMenu.Entity.Destroy();

            Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseExecuting);
            var action = _actionQueue.Dequeue();
            action.Execute();
            Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionExecuting, action);
        }

        public void OnActionFinishedExecuting(IPlayerAction action)
        {
            //if there are more actions in the queue, execute the next one
            if (_actionQueue.TryDequeue(out var nextAction))
            {
                _actionQueue.Dequeue().Execute();
            }
            else //otherwise, emit turn phase finished
            {
                Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseCompleted);
            }
        }
    }

    #region STATE MACHINE

    public class TurnHandlerStateMachine : StateMachine<TurnHandler>
    {
        public TurnHandlerStateMachine(TurnHandler context, State<TurnHandler> initialState) : base(context, initialState)
        {
            AddState(new SelectingFromMenu());
            AddState(new PreparingAction());
        }
    }

    public class SelectingFromMenu : State<TurnHandler>
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

    public class PreparingAction : State<TurnHandler>
    {
        public override void Update(float deltaTime)
        {

        }
    }

    #endregion
}
