using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class DyingState : State<PlayerController>
    {
        public override void Begin()
        {
            base.Begin();

            _context.SaveData();

            Game1.AudioManager.StopMusic();

            _context.DeathComponent.OnDeathFinished += OnDeathFinished;
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            _context.DeathComponent.OnDeathFinished -= OnDeathFinished;
        }

        void OnDeathFinished(Entity entity)
        {
            Game1.GameStateManager.ReturnToHubAfterDeath();
        }
    }
}
