using Microsoft.Xna.Framework;
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
        public HealingAura(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler) : base(actionPrepFinishedHandler, actionPrepCanceledHandler, executionFinishedHandler)
        {
        }

        public override void Prepare()
        {
            ActionPrepFinishedHandler?.Invoke(this, Position);
        }

        public override void Execute()
        {
            ExecutionFinishedHandler?.Invoke(this);
        }
    }
}
