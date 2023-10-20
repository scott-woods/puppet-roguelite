using Nez;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class BossCombatHandler : Component
    {
        public bool IsCombatStarted = false;
        public bool IsCleared = false;
        public List<Enemy> Enemies = new List<Enemy>();
        public int Turns = 0;

        public Entity TurnHandlerEntity;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //connect to boss triggers
            var bossSpawnTriggers = Entity.Scene.FindComponentsOfType<BossTrigger>().Where(t => t.MapEntity == Entity).ToList();
            foreach (var trigger in bossSpawnTriggers)
            {
                trigger.Emitter.AddObserver(TriggerEventTypes.Triggered, OnBossTriggered);
            }
        }

        void OnBossTriggered()
        {
            IsCombatStarted = true;

            //lock gates
            var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapEntity == Entity).ToList();
            foreach (var gate in gates)
            {
                gate.Lock();
            }
        }
    }
}
