using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Systems;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.PlayerActions;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.UI.Menus;
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
        Entity _turnMenuEntity;
        Stack<UICanvas> _menuStack = new Stack<UICanvas>();

        public Queue<IPlayerAction> ActionQueue = new Queue<IPlayerAction>();

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

            //setup turn menu entity
            _turnMenuEntity = Entity.Scene.CreateEntity("turn-menu");

            //observe emitter
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionFinishedPreparing, OnActionFinishedPreparing);
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionFinishedExecuting, OnActionFinishedExecuting);

            //begin selection
            StartNewSelection();
        }

        public void Update()
        {
            StateMachine.Update(Time.DeltaTime);
        }

        /// <summary>
        /// should be called at beginning or after each successful action queued
        /// </summary>
        void StartNewSelection()
        {
            StateMachine.ChangeState<SelectingFromMenu>();
            var actionSelector = new ActionsSelector(Player.Instance.Entity.Position, this);
            OpenMenu(actionSelector);
        }

        public void OpenMenu(UICanvas menu)
        {
            //hide previous menu if any
            if (_menuStack.Count > 0)
            {
                _menuStack.Peek().SetEnabled(false);
            }

            //add new menu to entity
            _turnMenuEntity?.AddComponent(menu);

            //add new menu to stack
            _menuStack.Push(menu);
        }

        public void GoBack()
        {
            if (_menuStack.Count > 1)
            {
                var currentMenu = _menuStack.Pop();
                _turnMenuEntity.RemoveComponent(currentMenu);
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
                //if cancelled, just go back to last menu
                _menuStack.Peek().SetEnabled(true);
                StateMachine.ChangeState<SelectingFromMenu>();
            }
            else
            {
                ActionQueue.Enqueue(action);
                _menuStack.Clear();
                _turnMenuEntity.RemoveAllComponents();
                StartNewSelection();
            }
        }

        public void ExecuteActions()
        {
            //destroy ui menus
            _turnMenuEntity.Destroy();

            Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseExecuting);
            var action = ActionQueue.Dequeue();
            action.Execute();
            Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionExecuting, action);
        }

        public void OnActionFinishedExecuting(IPlayerAction action)
        {
            //if there are more actions in the queue, execute the next one
            if (ActionQueue.TryDequeue(out var nextAction))
            {
                ActionQueue.Dequeue().Execute();
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
