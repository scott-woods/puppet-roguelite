using Nez;
using Nez.AI.BehaviorTrees;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite.Components.EnemyActions
{
    public abstract class EnemyAction<T> : Component where T : Enemy
    {
        protected enum EnemyActionState
        {
            NotStarted,
            Running,
            Succeeded,
            Failed
        }

        protected EnemyActionState _state = EnemyActionState.NotStarted;

        protected T _enemy { get; }

        protected List<Component> _components = new List<Component>();
        protected ICoroutine _executionCoroutine;

        public EnemyAction(T enemy)
        {
            _enemy = enemy;
        }

        /// <summary>
        /// Execute this action. Returns true if execution has completed, otherwise false
        /// </summary>
        /// <returns></returns>
        public TaskStatus Execute()
        {
            switch (_state)
            {
                case EnemyActionState.NotStarted:
                    _executionCoroutine = Game1.StartCoroutine(StartAction());
                    _state = EnemyActionState.Running;
                    return TaskStatus.Running;
                case EnemyActionState.Running:
                    return TaskStatus.Running;
                case EnemyActionState.Succeeded:
                    _state = EnemyActionState.NotStarted;
                    return TaskStatus.Success;
                case EnemyActionState.Failed:
                    _state = EnemyActionState.NotStarted;
                    return TaskStatus.Failure;
                default:
                    throw new InvalidOperationException("Unknown state.");
            }
        }

        protected abstract IEnumerator StartAction();

        /// <summary>
        /// stops the execution coroutine
        /// </summary>
        public virtual void Abort()
        {
            _state = EnemyActionState.NotStarted;
            _executionCoroutine?.Stop();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            if (_executionCoroutine != null)
            {
                _executionCoroutine.Stop();
            }

            foreach (var component in _components)
            {
                component.SetEnabled(false);
            }
        }
    }
}
