using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class CombatComponent : Component
    {
        public bool IsInCombat = false;

        public override void Initialize()
        {
            base.Initialize();

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
        }

        void OnEncounterStarted()
        {
            IsInCombat = true;
        }
    }
}
