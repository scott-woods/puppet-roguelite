using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Utilities
{
    [PlayerActionInfo("Teleport", 1, PlayerActionCategory.Utility)]
    public class Teleport : PlayerAction
    {
        const float _maxRadius = 64f;

        public override void Prepare()
        {
            base.Prepare();
        }

        public override void Execute(bool isSimulation = false)
        {
            base.Execute(isSimulation);
            var type = _isSimulation ? PlayerActionEvents.SimActionFinishedExecuting : PlayerActionEvents.ActionFinishedExecuting;
            Emitters.PlayerActionEmitter.Emit(type, this);
        }

        public override void Update()
        {
            if (_isPreparing)
            {
                if (Input.LeftMouseButtonPressed)
                {
                    _isPreparing = false;
                    Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this);
                    return;
                }
            }
        }
    }
}
