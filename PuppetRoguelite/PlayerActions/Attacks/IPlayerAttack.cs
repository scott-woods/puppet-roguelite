using Nez;
using PuppetRoguelite.PlayerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.PlayerActions.Attacks
{
    public abstract class PlayerAttack : IPlayerAction
    {
        protected bool _isSimulation;

        public virtual void Execute(bool isSimulation = false)
        {
            _isSimulation = isSimulation;
            var type = isSimulation ? PlayerActionEvents.SimActionExecuting : PlayerActionEvents.ActionExecuting;
            Emitters.PlayerActionEmitter.Emit(type, this);
        }

        public abstract void Prepare();
    }
}
