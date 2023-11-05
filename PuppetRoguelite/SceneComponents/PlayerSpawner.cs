using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Interfaces;
using PuppetRoguelite.Tools;
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
            var playerEntity = TransferManager.Instance.GetEntityToTransfer();
            if (playerEntity != null)
            {
                TransferManager.Instance.ClearEntityToTransfer();
                playerEntity.AttachToScene(Scene);
            }
            else
            {
                playerEntity = Scene.AddEntity(new Entity("player"));
                playerEntity.AddComponent(new PlayerController());
            }

            return playerEntity;
        }

        public void SpawnPlayer(Entity playerEntity, Entity mapEntity)
        {
            var spawnPosition = Vector2.Zero;
            var spawn = Scene.FindComponentsOfType<PlayerSpawnPoint>().First(s => s.MapEntity == mapEntity && s.Id == Game1.SceneManager.TargetEntranceId);
            if (spawn != null)
            {
                spawnPosition = spawn.Entity.Position;
            }
            playerEntity.SetPosition(spawnPosition);
            playerEntity.SetEnabled(true);
        }

        public void SpawnPlayer(Entity playerEntity, Vector2 position)
        {
            playerEntity.SetPosition(position);
            playerEntity.SetEnabled(true);
        }
    }
}
