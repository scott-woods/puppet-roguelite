using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Actions;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Attacks
{
    [PlayerActionInfo("Slash", 1)]
    public class Slash : IPlayerAttack
    {
        Entity _entity;

        public void Execute()
        {
            _entity.Destroy();
            Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedExecuting, this);
        }

        public void Prepare()
        {
            _entity = Game1.Scene.CreateEntity("slash-entity");
            _entity.AddComponent(new PrototypeSpriteRenderer(32, 32));
            _entity.SetPosition(Player.Instance.Entity.Position + new Vector2(20, 20));
            //Core.Schedule(2, timer => Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this));
            Emitters.PlayerActionEmitter.Emit(PlayerActionEvents.ActionFinishedPreparing, this);
        }
    }
}
