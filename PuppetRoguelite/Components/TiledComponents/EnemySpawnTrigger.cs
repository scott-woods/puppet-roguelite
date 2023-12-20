using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Enemies.ChainBot;
using PuppetRoguelite.Components.Characters.Enemies.Ghoul;
using PuppetRoguelite.Components.Characters.Enemies.OrbMage;
using PuppetRoguelite.Components.Characters.Enemies.Spitter;
using PuppetRoguelite.SceneComponents.CombatManager;
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
        List<Type> _enemyTypes = new List<Type>() { typeof(ChainBot), typeof(Spitter), typeof(Ghoul), typeof(OrbMage) };

        public EnemySpawnTrigger(TmxObject tmxTriggerObject, Entity mapEntity) : base(tmxTriggerObject, mapEntity)
        {
        }

        public override void HandleTriggered()
        {
            if (Entity != null)
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
            var typesPicked = new List<Type>();
            while (i < enemySpawns.Count)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Enemy_spawn);
                var spawn = enemySpawns[i];

                Type enemyType;

                //get enemy types that have been picked less than 3 times
                var possibleTypes = _enemyTypes.Where(t => typesPicked.Where(x => x == t).Count() < 3).ToList();

                //if any possible types, get a random from that list
                if (possibleTypes.Any())
                    enemyType = possibleTypes.RandomItem();
                else //if all types are already at max, just pick another random one
                    enemyType = _enemyTypes.RandomItem();

                typesPicked.Add(enemyType);

                combatManager.AddEnemy(spawn.SpawnEnemy(enemyType));
                i++;
                yield return Coroutine.WaitForSeconds(.2f);
            }
            //combatManager.AddEnemy(enemySpawns[0].SpawnEnemy(typeof(Spitter)));
            //combatManager.AddEnemy(enemySpawns[1].SpawnEnemy(typeof(ChainBot)));

            //destroy entity
            Entity.Destroy();
        }
    }
}
