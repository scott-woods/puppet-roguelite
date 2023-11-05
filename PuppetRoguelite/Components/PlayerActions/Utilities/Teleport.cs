using Microsoft.Xna.Framework;
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

        public Teleport(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler) : base(actionPrepFinishedHandler, actionPrepCanceledHandler, executionFinishedHandler)
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
