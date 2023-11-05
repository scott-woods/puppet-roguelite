//using Microsoft.Xna.Framework;
//using Nez;
//using PuppetRoguelite.Components.PlayerActions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PuppetRoguelite.Components
//{
//    public class ActionSequenceSimulator : Component
//    {
//        //public Queue<PlayerAction> ActionQueue = new Queue<PlayerAction>();
//        Queue<PlayerAction> _actionQueue;
//        Entity _playerSimEntity;
//        PlayerAction _currentAction;
//        Vector2 _returnPosition;

//        int _currentIndex = 0;
//        bool _active = false;

//        public override void OnAddedToEntity()
//        {
//            base.OnAddedToEntity();

//            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.SimActionFinishedExecuting, OnSimActionFinishedExecuting);
//        }

//        public override void OnRemovedFromEntity()
//        {
//            base.OnRemovedFromEntity();

//            Emitters.PlayerActionEmitter.RemoveObserver(PlayerActionEvents.SimActionFinishedExecuting, OnSimActionFinishedExecuting);
//        }

//        public void StartSim(Queue<PlayerAction> actionQueue, Entity playerSimEntity, Vector2 returnPosition)
//        {
//            _actionQueue = actionQueue;
//            _playerSimEntity = playerSimEntity;
//            _returnPosition = returnPosition;

//            _active = true;

//            PlayNextAction();
//        }

//        public void StopSim()
//        {
//            _active = false;

//            _playerSimEntity.RemoveComponent(_currentAction);
//            _playerSimEntity.SetPosition(_returnPosition);

//            _currentIndex = 0;
//            _currentAction = null;
//        }

//        void PlayNextAction()
//        {
//            _currentAction = _actionQueue.ElementAt(_currentIndex);
//            _playerSimEntity.AddComponent(_currentAction);
//            _currentAction.Execute(true);
//        }

//        void OnSimActionFinishedExecuting(IPlayerAction action)
//        {
//            if (_active)
//            {
//                if (_currentIndex < _actionQueue.Count - 1) //not at end of queue
//                {
//                    _currentIndex++;
//                }
//                else //reached end of queue
//                {
//                    _currentIndex = 0;
//                }

//                var nextAction = _actionQueue.ElementAt(_currentIndex);

//                if (nextAction != _currentAction)
//                {
//                    _playerSimEntity.RemoveComponent(_currentAction);
//                    _currentAction = nextAction;

//                    _playerSimEntity.AddComponent(_currentAction);
//                    _currentAction.Execute(true);
//                }
//                else
//                {
//                    _currentAction.Execute(true);
//                }
//            }
//        }
//    }
//}
