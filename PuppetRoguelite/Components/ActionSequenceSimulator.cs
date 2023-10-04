using PuppetRoguelite.PlayerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class ActionSequenceSimulator
    {
        public Queue<IPlayerAction> ActionQueue = new Queue<IPlayerAction>();

        int _currentIndex = 0;
        bool _started = false;

        public ActionSequenceSimulator()
        {
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.SimActionFinishedExecuting, OnSimActionFinishedExecuting);
        }

        public void UpdateQueue(Queue<IPlayerAction> actionQueue)
        {
            ActionQueue = actionQueue;
            if (!_started)
            {
                _started = true;
                ExecuteActions();
            }
        }

        public void ExecuteActions()
        {
            var nextAction = ActionQueue.ElementAt(_currentIndex);
            nextAction.Execute(true);
        }

        void OnSimActionFinishedExecuting(IPlayerAction action)
        {
            if (_currentIndex < ActionQueue.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            var nextAction = ActionQueue.ElementAt(_currentIndex);
            nextAction.Execute(true);
        }
    }
}
