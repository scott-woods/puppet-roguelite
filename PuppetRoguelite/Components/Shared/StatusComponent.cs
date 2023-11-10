using Nez;
using Nez.AI.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class StatusComponent : Component
    {
        private Stack<Status> _stateStack = new Stack<Status>();

        public Status CurrentStatus => _stateStack.Peek();

        public StatusComponent(Status status)
        {
            _stateStack.Push(status);
        }

        public override void Initialize()
        {
            base.Initialize();

            SetUpdateOrder(int.MinValue);
        }

        public void PushStatus(Status status)
        {
            if (_stateStack.Count == 0 || status.Priority > CurrentStatus.Priority)
            {
                _stateStack.Push(status);
            }
        }

        public void PopStatus(Status status)
        {
            if (CurrentStatus == status)
            {
                _stateStack.Pop();
            }
        }
    }

    public class Status
    {
        public enum StatusType { Normal, Stunned, Frozen, Death }
        public StatusType Type { get; private set; }
        public int Priority { get; private set; }

        public Status(StatusType type, int priority)
        {
            Type = type;
            Priority = priority;
        }
    }

    public enum StatusPriority
    {
        Normal = 0,
        Stunned = 1,
        Frozen = 2,
        Death = 3,
    }
}
