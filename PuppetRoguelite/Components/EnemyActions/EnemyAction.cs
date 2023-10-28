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

        protected T _enemy { get; set; }

        protected List<Component> _components = new List<Component>();
        protected ICoroutine _executionCoroutine;

        /// <summary>
        /// Execute this action. Returns true if execution has completed, otherwise false
        /// </summary>
        /// <returns></returns>
        public TaskStatus Execute(T enemy)
        {
            _enemy = enemy;
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
            //if (_isExecutionCompleted)
            //{
            //    return true;
            //}
            //else if (_isExecuting)
            //{
            //    return false;
            //}
            //else
            //{
            //    _isExecuting = true;
            //    _executionCoroutine = Game1.StartCoroutine(HandleExecution());
            //    return false;
            //}
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
