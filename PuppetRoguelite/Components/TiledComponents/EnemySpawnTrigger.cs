using Nez;
using Nez.Tiled;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class EnemySpawnTrigger : Trigger
    {
        public EnemySpawnTrigger(TmxObject tmxTriggerObject, Entity mapEntity) : base(tmxTriggerObject, mapEntity)
        {
        }

        public override void HandleTriggered()
        {
            //destroy other enemy spawn triggers
            var triggers = Entity.Scene.FindComponentsOfType<EnemySpawnTrigger>().Where(t => t.MapEntity == MapEntity).ToList();
            foreach (var trigger in triggers)
            {
                trigger.Entity.Destroy();
            }

            //lock gates
            var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapEntity == MapEntity).ToList();
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            var combatManager = Entity.Scene.GetOrCreateSceneComponent<CombatManager>();

            //spawn enemies
            var enemySpawns = Entity.Scene.FindComponentsOfType<EnemySpawnPoint>().Where(e => e.MapEntity == MapEntity).ToList();
            //foreach (var spawn in enemySpawns)
            //{
            //    combatManager.AddEnemy(spawn.SpawnEnemy());
            //}
            combatManager.AddEnemy(enemySpawns[0].SpawnEnemy());

            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);
            
            Entity.Destroy();
        }
    }
}
