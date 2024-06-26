﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Tools;
using System.Linq;

namespace PuppetRoguelite.SceneComponents
{
    public class PlayerSpawner : SceneComponent
    {
        Entity _playerEntity;

        public override void OnEnabled()
        {
            base.OnEnabled();

            Game1.SceneManager.Emitter.AddObserver(GlobalManagers.SceneEvents.TransitionEnded, OnTransitionEnded);
        }

        public Entity CreatePlayerEntity()
        {
            _playerEntity = TransferManager.Instance.GetEntityToTransfer();
            if (_playerEntity != null)
            {
                TransferManager.Instance.ClearEntityToTransfer();
                _playerEntity.AttachToScene(Scene);
            }
            else
            {
                _playerEntity = Scene.AddEntity(new Entity("player"));
                _playerEntity.AddComponent(new PlayerController());
            }

            return _playerEntity;
        }

        public void SpawnPlayer(Entity mapEntity)
        {
            var spawnPosition = Vector2.Zero;
            var playerSpawnPoints = Scene.FindComponentsOfType<PlayerSpawnPoint>();
            if (playerSpawnPoints != null && playerSpawnPoints.Count > 0)
            {
                var spawn = playerSpawnPoints.FirstOrDefault(s => s.MapEntity == mapEntity && s.Id == Game1.SceneManager.TargetEntranceId);
                if (spawn != null)
                {
                    spawnPosition = spawn.Entity.Position;
                }
            }
            
            _playerEntity.SetPosition(spawnPosition);
            _playerEntity.SetEnabled(true);

            if (_playerEntity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                animator.OnEntityTransformChanged(Transform.Component.Position);
            }

            if (Game1.SceneManager.State == GlobalManagers.SceneManagerState.Transitioning)
            {
                if (_playerEntity.TryGetComponent<PlayerController>(out var player))
                {
                    player.WaitingForSceneTransition = true;
                }
            }
        }

        public void SpawnPlayer(Vector2 position)
        {
            _playerEntity.SetPosition(position);
            _playerEntity.SetEnabled(true);

            if (_playerEntity.TryGetComponent<SpriteAnimator>(out var animator))
            {
                animator.OnEntityTransformChanged(Transform.Component.Position);
            }

            if (Game1.SceneManager.State == GlobalManagers.SceneManagerState.Transitioning)
            {
                if (_playerEntity.TryGetComponent<PlayerController>(out var player))
                {
                    player.WaitingForSceneTransition = true;
                }
            }
        }

        void OnTransitionEnded()
        {
            if (_playerEntity.TryGetComponent<PlayerController>(out var player))
            {
                player.WaitingForSceneTransition = false;
            }
        }
    }
}
