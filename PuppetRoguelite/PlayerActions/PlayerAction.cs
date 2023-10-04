using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.PlayerActions
{
    public abstract class PlayerAction : Component, IPlayerAction, IUpdatable
    {
        protected bool _isSimulation;
        protected bool _isPreparing = false;

        public virtual void Prepare()
        {
            _isPreparing = true;
        }

        public virtual void Execute(bool isSimulation = false)
        {
            _isSimulation = isSimulation;
            var type = isSimulation ? PlayerActionEvents.SimActionExecuting : PlayerActionEvents.ActionExecuting;
            Emitters.PlayerActionEmitter.Emit(type, this);
        }

        public virtual void Update()
        {
            if (_isPreparing)
            {
                if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
                {
                    _isPreparing = false;
                    Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionPrepCanceled, this);
                }
            }
        }
    }
}
