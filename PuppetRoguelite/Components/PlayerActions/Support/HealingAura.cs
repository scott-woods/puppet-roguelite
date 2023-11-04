using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Support
{
    [PlayerActionInfo("Healing Aura", 2, PlayerActionCategory.Support)]
    public class HealingAura : PlayerAction
    {
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
