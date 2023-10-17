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
                Game1.StartCoroutine(HandleExecution());
                return false;
            }
        }

        protected abstract IEnumerator HandleExecution();
    }
}
