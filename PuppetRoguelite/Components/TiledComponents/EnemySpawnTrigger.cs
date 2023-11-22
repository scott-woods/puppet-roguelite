using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Characters.Ghoul;
using PuppetRoguelite.Components.Characters.Spitter;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class EnemySpawnTrigger : Trigger
    {
        List<Type> _enemyTypes = new List<Type>() { typeof(ChainBot), typeof(Spitter), typeof(Ghoul) };

        public EnemySpawnTrigger(TmxObject tmxTriggerObject, Entity mapEntity) : base(tmxTriggerObject, mapEntity)
        {
        }

        public override void HandleTriggered()
        {
            //disable self
            SetEnabled(false);

            //destroy other enemy spawn triggers
            var triggers = Entity.Scene.FindComponentsOfType<EnemySpawnTrigger>().Where(t => t.MapEntity == MapEntity).ToList();
            foreach (var trigger in triggers)
            {
                if (trigger != this)
                {
                    trigger.Entity.Destroy();
                }
            }

            //start encounter coroutine
            Game1.StartCoroutine(HandleEncounterStart());
        }

        IEnumerator HandleEncounterStart()
        {
            var combatManager = Entity.Scene.GetOrCreateSceneComponent<CombatManager>();

            //lock gates
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Gate_close);
            var gates = Entity.Scene.FindComponentsOfType<Gate>().Where(g => g.MapEntity == MapEntity).ToList();
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            //wait 1 second before spawning enemies
            yield return Coroutine.WaitForSeconds(1f);

            //emit combat started
            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);

            //spawn enemies
            var enemySpawns = Entity.Scene.FindComponentsOfType<EnemySpawnPoint>().Where(e => e.MapEntity == MapEntity).ToList();
            int i = 0;
            while (i < enemySpawns.Count)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Enemy_spawn);
                var spawn = enemySpawns[i];
                combatManager.AddEnemy(spawn.SpawnEnemy(_enemyTypes.RandomItem()));
                i++;
                yield return Coroutine.WaitForSeconds(.2f);
            }
            //combatManager.AddEnemy(enemySpawns[0].SpawnEnemy(typeof(ChainBot)));
            //combatManager.AddEnemy(enemySpawns[1].SpawnEnemy(typeof(ChainBot)));

            //destroy entity
            Entity.Destroy();
        }
    }
}
