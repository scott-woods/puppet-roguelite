using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    /// <summary>
    /// includes a collider with a trigger to transition to a new scene
    /// </summary>
    public class ExitArea : TiledComponent, IUpdatable
    {
        public Emitter<ExitAreaEvents> Emitter = new Emitter<ExitAreaEvents>();

        Collider _collider;

        public Type TargetSceneType;
        string _targetEntranceId;
        bool _triggered = false;

        public ExitArea(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
            if (tmxObject.Properties.TryGetValue("TargetScene", out var targetScene))
            {
                TargetSceneType = Type.GetType("PuppetRoguelite.Scenes." + targetScene);
            }
            if (tmxObject.Properties.TryGetValue("TargetEntrance", out var targetEntranceId))
            {
                _targetEntranceId = targetEntranceId;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            //collider
            _collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.Trigger);
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.PlayerCollider);
            Entity.AddComponent(_collider);
        }

        public void Update()
        {
            if (!_triggered)
            {
                var colliders = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
                if (colliders.Count > 0)
                {
                    _triggered = true;
                    Emitter.Emit(ExitAreaEvents.Triggered);
                    TransferManager.Instance.SetEntityToTransfer(PlayerController.Instance.Entity);
                    Game1.SceneManager.ChangeScene(TargetSceneType, _targetEntranceId);
                    Entity.Destroy();
                }
            }
        }
    }

    public enum ExitAreaEvents
    {
        Triggered
    }
}
