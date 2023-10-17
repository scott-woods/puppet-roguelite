using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.EnemyActions
{
    public abstract class EnemyAction : Component
    {
        protected bool _isExecuting = false;
        protected bool _isExecutionCompleted = false;
        protected List<Component> _components = new List<Component>();
        protected ICoroutine _executionCoroutine;

        /// <summary>
        /// Execute this action. Returns true if execution has completed, otherwise false
        /// </summary>
        /// <returns></returns>
        public virtual bool Execute()
        {
            if (_isExecutionCompleted)
            {
                return true;
            }
            else if (_isExecuting)
            {
                return false;
            }
            else
            {
                _isExecuting = true;
                _executionCoroutine = Game1.StartCoroutine(HandleExecution());
                return false;
            }
        }

        protected abstract IEnumerator HandleExecution();

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

            _isExecuting = false;
            _isExecutionCompleted = false;
        }
    }
}
