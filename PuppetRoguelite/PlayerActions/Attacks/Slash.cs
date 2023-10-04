using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.PlayerActions.Attacks
{
    [PlayerActionInfo("Slash", 1)]
    public class Slash : PlayerAttack
    {
        Entity _entity;

        public override void Execute(bool isSimulation = false)
        {
            base.Execute(isSimulation);

            _entity.Destroy();
            var type = _isSimulation ? PlayerActionEvents.SimActionFinishedExecuting : PlayerActionEvents.ActionFinishedExecuting;
            Emitters.PlayerActionEmitter.Emit(type, this);
        }

        public override void Prepare()
        {
            _entity = Core.Scene.CreateEntity("slash-entity");
            _entity.AddComponent(new PrototypeSpriteRenderer(32, 32));
            _entity.SetPosition(Player.Instance.Entity.Position + new Vector2(20, 20));
            //Core.Schedule(2, timer => Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this));
            Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this);
        }
    }
}
