using Nez;
using Nez.AI.BehaviorTrees;
using PuppetRoguelite.Components.Characters.Enemies;
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

        public EnemyAction(T enemy)
        {
            _enemy = enemy;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
            }
        }

        /// <summary>
        /// Execute this action. Returns true if execution has completed, otherwise false
        /// </summary>
        /// <returns></returns>
        public TaskStatus Execute()
        {
            if (_enemy.Entity.TryGetComponent<StatusComponent>(out var sc))
            {
                if (sc.CurrentStatus.Type != Status.StatusType.Normal)
                {
                    Abort();
                    return TaskStatus.Failure;
                }
            }
            switch (_state)
            {
                case EnemyActionState.NotStarted:
                    _state = EnemyActionState.Running;
                    StartAction();
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

        protected abstract void StartAction();

        protected virtual void HandleActionFinished()
        {
            _state = EnemyActionState.Succeeded;
        }

        /// <summary>
        /// stops the action
        /// </summary>
        public virtual void Abort()
        {
            _state = EnemyActionState.NotStarted;
        }

        void OnHealthDepleted(HealthComponent hc)
        {
            Abort();
        }
    }
}
