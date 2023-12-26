using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters
{
    public class DecoyPlayer : PlayerSim
    {
        const float _lifetime = 5f;

        ITimer _timer;

        public DecoyPlayer(Vector2 direction) : base(direction)
        { 
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Entity.SetTag((int)EntityTags.EnemyTarget);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            _timer?.Stop();
        }

        public void StartTimer()
        {
            _timer = Game1.Schedule(_lifetime, OnTimerCompleted);
        }

        void OnTimerCompleted(ITimer timer)
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Decoy_fade);
            Entity.Destroy();
        }

        void OnTurnPhaseTriggered()
        {
            _timer?.Stop();
            Entity.Destroy();
        }
    }
}
