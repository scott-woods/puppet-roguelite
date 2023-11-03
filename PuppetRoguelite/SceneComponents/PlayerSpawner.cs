using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class PlayerSpawner : SceneComponent
    {
        public Entity CreatePlayerEntity()
        {
            var playerEntity = Scene.FindEntity("player");
            if (playerEntity != null)
            {
                playerEntity.AttachToScene(Scene);
                var components = playerEntity.GetComponents<Component>().Where(c => c is ITransferrable).ToList();
                for (int i = 0; i < components.Count; i++)
                {
                    var comp = components[i] as ITransferrable;
                    comp.AddObservers();
                }
            }
            else
            {
                playerEntity = Scene.CreateEntity("player");
                playerEntity.AddComponent(new PlayerController());
            }

            return playerEntity;
        }

        public void SpawnPlayer(Entity playerEntity, Entity mapEntity)
        {
            var spawn = Scene.FindComponentsOfType<PlayerSpawnPoint>().First(s => s.MapEntity == mapEntity && s.Id == Game1.SceneManager.TargetEntranceId);
            playerEntity.SetPosition(spawn.Entity.Position);
            playerEntity.SetEnabled(true);
        }
    }
}
