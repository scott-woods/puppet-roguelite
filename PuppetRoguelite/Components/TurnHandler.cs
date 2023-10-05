using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
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

        public Queue<PlayerAction> ActionQueue = new Queue<PlayerAction>();
        Vector2 _initialPlayerPosition;
        Vector2 _finalPlayerPosition;

        CombatManager _combatManager;
        ActionSequenceSimulator _sequenceSimulator = new ActionSequenceSimulator();
        Entity _playerSimEntity;

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
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionPrepCanceled, OnActionPrepCanceled);

            //store player's position at start of turn
            _initialPlayerPosition = Player.Instance.Entity.Position;
            _finalPlayerPosition = Player.Instance.Entity.Position;

            //add the sim player
            _playerSimEntity = Entity.Scene.CreateEntity("player-sim");
            _playerSimEntity.AddComponent(new PlayerSim(Player.Instance.Direction));
            _playerSimEntity.SetPosition(_initialPlayerPosition);

            //begin selection
            StartNewSelection();
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Emitters.PlayerActionEmitter.RemoveObserver(PlayerActionEvents.ActionFinishedPreparing, OnActionFinishedPreparing);
            Emitters.PlayerActionEmitter.RemoveObserver(PlayerActionEvents.ActionFinishedExecuting, OnActionFinishedExecuting);
            Emitters.PlayerActionEmitter.RemoveObserver(PlayerActionEvents.ActionPrepCanceled, OnActionPrepCanceled);
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
            Core.Schedule(.1f, timer =>
            {
                //if sim and real player would overlap, hide real player
                if (_finalPlayerPosition == _initialPlayerPosition)
                {
                    Player.Instance.Entity.SetEnabled(false);
                }

                StateMachine.ChangeState<SelectingFromMenu>();
                var actionSelector = new ActionsSelector(Player.Instance.Entity.Position, this);
                OpenMenu(actionSelector);
            });
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
            Core.Schedule(.1f, timer =>
            {
                //disable last menu
                _menuStack.Peek().SetEnabled(false);

                //change state
                StateMachine.ChangeState<PreparingAction>();

                //hide original player if it will overlap sim player
                if (_finalPlayerPosition == _initialPlayerPosition)
                {
                    Player.Instance.Entity.SetEnabled(false);
                }

                //hide sim player
                if (_playerSimEntity.TryGetComponent<SpriteAnimator>(out var animator))
                {
                    animator.SetEnabled(false);
                }

                //create action instance, add to sim player, and prepare it
                var action = Activator.CreateInstance(actionType) as PlayerAction;
                _playerSimEntity.AddComponent(action);
                action.Prepare();
            });
        }

        public void OnActionFinishedPreparing(PlayerAction action)
        {
            Core.Schedule(.1f, timer =>
            {
                //remove action from the sim
                _playerSimEntity.RemoveComponent(action);

                //add it to the queue, to be added to the real player later
                ActionQueue.Enqueue(action);
                //_sequenceSimulator.UpdateQueue(ActionQueue);

                //clear menu and start new selection process
                _menuStack.Clear();
                _turnMenuEntity.RemoveAllComponents();
                StartNewSelection();

                //reenable player and sim player
                if (_finalPlayerPosition != _initialPlayerPosition)
                {
                    Player.Instance.Entity.SetEnabled(true);
                }
                if (_playerSimEntity.TryGetComponent<SpriteAnimator>(out var animator))
                {
                    animator.SetEnabled(true);
                }
            });
        }

        public void OnActionPrepCanceled(PlayerAction action)
        {
            Core.Schedule(.1f, timer =>
            {
                //remove action from the sim
                _playerSimEntity.RemoveComponent(action);

                //reenable player and sim player
                if (_finalPlayerPosition != _initialPlayerPosition)
                {
                    Player.Instance.Entity.SetEnabled(true);
                }
                if (_playerSimEntity.TryGetComponent<SpriteAnimator>(out var animator))
                {
                    animator.SetEnabled(true);
                }

                //if cancelled, just go back to last menu
                _menuStack.Peek().SetEnabled(true);
                StateMachine.ChangeState<SelectingFromMenu>();
            });
        }

        public void ExecuteActions()
        {
            //destroy ui menus
            _turnMenuEntity.Destroy();

            //destory sim player
            _playerSimEntity.Destroy();

            //reenable real player
            Player.Instance.Entity.SetEnabled(true);
            if (Player.Instance.Entity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                animator.SetEnabled(false);
            }

            //change state
            Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseExecuting);

            var action = ActionQueue.Dequeue();
            Player.Instance.Entity.AddComponent(action);
            action.Execute();
            Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionExecuting, action);
        }

        public void OnActionFinishedExecuting(PlayerAction action)
        {
            //remove action from player entity
            Player.Instance.Entity.RemoveComponent(action);

            //if there are more actions in the queue, execute the next one
            if (ActionQueue.TryDequeue(out var nextAction))
            {
                Player.Instance.Entity.AddComponent(nextAction);
                nextAction.Execute();
            }
            else //otherwise, emit turn phase finished
            {
                if (Player.Instance.Entity.TryGetComponent<SpriteAnimator>(out var animator))
                {
                    animator.SetEnabled(true);
                }
                Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseCompleted);
            }
        }

        public void ShowSequenceSim()
        {
            //hide menu
            _menuStack.Peek().SetEnabled(false);

            //hide sim player
            if (_playerSimEntity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                animator.SetEnabled(false);
            }

            //start sim
            _sequenceSimulator.StartSim(ActionQueue, _playerSimEntity, _playerSimEntity.Position);

            //change state
            StateMachine.ChangeState<ShowingSim>();
        }

        public void HideSequenceSim()
        {
            //stop sim
            _sequenceSimulator.StopSim();

            //show sim player
            if (_playerSimEntity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                animator.SetEnabled(true);
            }

            //show menu
            _menuStack.Peek().SetEnabled(true);

            //change state
            StateMachine.ChangeState<SelectingFromMenu>();
        }
    }

    #region STATE MACHINE

    public class TurnHandlerStateMachine : StateMachine<TurnHandler>
    {
        public TurnHandlerStateMachine(TurnHandler context, State<TurnHandler> initialState) : base(context, initialState)
        {
            AddState(new SelectingFromMenu());
            AddState(new PreparingAction());
            AddState(new ShowingSim());
        }
    }

    public class SelectingFromMenu : State<TurnHandler>
    {
        VirtualButton _cancelInput;
        VirtualButton _simButton;

        public override void Begin()
        {
            base.Begin();

            _cancelInput = new VirtualButton();
            _cancelInput.AddKeyboardKey(Keys.X);

            _simButton = new VirtualButton();
            _simButton.AddKeyboardKey(Keys.C);
        }

        public override void Update(float deltaTime)
        {
            if (_simButton.IsPressed)
            {
                if (_context.ActionQueue.Count > 0)
                {
                    _context.ShowSequenceSim();
                }
            }
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

    public class ShowingSim : State<TurnHandler>
    {
        VirtualButton _backButton;

        public override void Begin()
        {
            base.Begin();

            _backButton = new VirtualButton();
            _backButton.AddKeyboardKey(Keys.C);
            _backButton.AddKeyboardKey(Keys.X);
        }

        public override void Update(float deltaTime)
        {
            if (_backButton.IsPressed)
            {
                _context.HideSequenceSim();
            }
        }
    }

    #endregion
}
