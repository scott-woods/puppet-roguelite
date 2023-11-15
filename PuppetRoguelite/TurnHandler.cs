using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.UI.Elements;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PuppetRoguelite
{
    public class TurnHandler
    {
        //protected TurnHandlerStateMachine StateMachine;

        //menus
        Entity _turnMenuEntity;
        Stack<UIMenu> _menuStack = new Stack<UIMenu>();

        public Queue<PlayerAction> ActionQueue = new Queue<PlayerAction>();

        Entity _playerSimEntity;

        DeadzoneFollowCamera _camera;

        ActionTypeSelector _actionsSelector;
        ActionMenu _attackMenu, _utilityMenu, _supportMenu;

        /// <summary>
        /// called once at the beginning of the turn phase
        /// </summary>
        public void BeginTurn()
        {
            Debug.Log("turn handler beginning turn");
            //create sim player
            _playerSimEntity = Game1.Scene.CreateEntity("player-sim");
            _playerSimEntity.AddComponent(new PlayerSim(Vector2.One));
            var animator = _playerSimEntity.GetComponent<SpriteAnimator>();
            animator.SetColor(new Color(Color.White, 128));
            _playerSimEntity.SetPosition(PlayerController.Instance.Entity.Position);

            //set camera to follow sim
            _camera = Game1.Scene.Camera.GetComponent<DeadzoneFollowCamera>();
            _camera.SetFollowTarget(_playerSimEntity);

            //create menu entity
            Debug.Log("creating turn menu entity");
            _turnMenuEntity = Game1.Scene.CreateEntity("turn-menu");
            _actionsSelector = _turnMenuEntity.AddComponent(new ActionTypeSelector(OnActionTypeSelected, GoBack));
            _actionsSelector.Hide();
            _attackMenu = _turnMenuEntity.AddComponent(new ActionMenu(PlayerController.Instance.ActionsManager.AttackActions, "", OnActionSelected, GoBack));
            _attackMenu.Hide();
            _utilityMenu = _turnMenuEntity.AddComponent(new ActionMenu(PlayerController.Instance.ActionsManager.UtilityActions, "", OnActionSelected, GoBack));
            _utilityMenu.Hide();
            _supportMenu = _turnMenuEntity.AddComponent(new ActionMenu(PlayerController.Instance.ActionsManager.SupportActions, "", OnActionSelected, GoBack));
            _supportMenu.Hide();

            //start selection
            StartNewSelection();
        }

        /// <summary>
        /// should be called at beginning or after each successful action queued
        /// </summary>
        void StartNewSelection()
        {
            if (_playerSimEntity.Position == PlayerController.Instance.Entity.Position)
            {
                _playerSimEntity.SetEnabled(false);
            }
            else _playerSimEntity.SetEnabled(true);

            //clear menu stack
            _menuStack.Clear();

            OpenMenu(_actionsSelector);
        }

        public void OpenMenu(UIMenu menu)
        {
            //hide previous menu if any
            if (_menuStack.Count > 0)
            {
                _menuStack.Peek().Hide();
            }

            _menuStack.Push(menu);
            menu.Show(_playerSimEntity.Position);
        }

        public void GoBack()
        {
            if (_menuStack.Count > 1)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
                var currentMenu = _menuStack.Pop();
                currentMenu.Hide();
                _menuStack.Peek().Show(_playerSimEntity.Position);
            }
        }

        void OnActionTypeSelected(Button button)
        {
            var actionButton = button as ActionButton;
            switch (actionButton.Type)
            {
                case ActionButtonType.Attack:
                    OpenMenu(_attackMenu);
                    break;
                case ActionButtonType.Utility:
                    OpenMenu(_utilityMenu);
                    break;
                case ActionButtonType.Support:
                    OpenMenu(_supportMenu);
                    break;
                case ActionButtonType.Execute:
                    StartExecution();
                    break;
            }
        }

        void OnActionSelected(Type actionType)
        {
            //disable last menu
            _menuStack.Peek().Hide();

            //create action instance
            var action = Activator.CreateInstance(actionType) as PlayerAction;
            action.Emitter.AddObserver(PlayerActionEvent.PrepCanceled, OnActionPrepCanceled);
            action.Emitter.AddObserver(PlayerActionEvent.PrepFinished, OnActionPrepFinished);
            action.Emitter.AddObserver(PlayerActionEvent.ExecutionFinished, OnActionExecutionFinished);
            Game1.Scene.AddEntity(action);
            action.SetPosition(_playerSimEntity.Position);

            //disable sim player
            _playerSimEntity.SetEnabled(false);

            //if still in original position, disable real player as well
            if (PlayerController.Instance.Entity.Position ==  _playerSimEntity.Position)
            {
                PlayerController.Instance.Entity.SetEnabled(false);
            }

            //prepare action
            action.Prepare();
        }

        void OnActionPrepFinished(PlayerAction action)
        {
            //add action to queue and detach from scene
            ActionQueue.Enqueue(action);
            action.SetEnabled(false);

            //move sim player to new final position
            _playerSimEntity.SetPosition(action.FinalPosition);

            //reenable sim and real player
            _playerSimEntity.SetEnabled(true);
            PlayerController.Instance.Entity.SetEnabled(true);

            //start new selection
            StartNewSelection();
        }

        void OnActionPrepCanceled(PlayerAction action)
        {
            //destroy action
            action.Destroy();

            //reenable sim and real player
            _playerSimEntity.SetEnabled(true);
            PlayerController.Instance.Entity.SetEnabled(true);

            //reenable last menu
            _menuStack.Peek().Show(_playerSimEntity.Position);
        }

        void OnActionExecutionFinished(PlayerAction action)
        {
            Debug.Log($"Finished executing Action: ${PlayerActionUtils.GetName(action.GetType())}");

            //get final position
            var finalPos = action.FinalPosition;

            //destroy action
            if (!action.ShouldDestroySelf)
            {
                action.Destroy();
            }

            //try to execute next action. if false, that means none left, execution is over
            if (!ExecuteNextAction())
            {
                EndTurn(finalPos);
            }
        }

        void StartExecution()
        {
            //destroy turn menu
            Debug.Log("destroying turn menu entity");
            _turnMenuEntity.Destroy();

            //destroy player sim
            _playerSimEntity.Destroy();

            //disable player
            PlayerController.Instance.Entity.SetEnabled(false);

            //emit turn phase executing signal
            Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseExecuting);

            //dequeue action
            if (!ExecuteNextAction())
            {
                EndTurn(PlayerController.Instance.Entity.Position);
            }
        }

        bool ExecuteNextAction()
        {
            Debug.Log("trying to execute next action");

            if (ActionQueue.Count < 1)
            {
                return false;
            }

            var action = ActionQueue.Dequeue();
            Debug.Log($"Executing Action: {PlayerActionUtils.GetName(action.GetType())}");
            action.SetEnabled(true);
            _camera.SetFollowTarget(action);
            action.Execute();
            return true;
        }

        void EndTurn(Vector2 finalPosition)
        {
            Debug.Log("finished executing actions");

            PlayerController.Instance.Entity.SetEnabled(true);
            PlayerController.Instance.Entity.SetPosition(finalPosition);
            _camera.SetFollowTarget(PlayerController.Instance.Entity);

            Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseCompleted);
        }
    }
}
