using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Enums;
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
    public class ExitArea : TiledComponent, ITriggerListener
    {
        public Emitter<ExitAreaEvents> Emitter = new Emitter<ExitAreaEvents>();

        Collider _collider;

        public Type TargetSceneType;
        string _targetEntranceId;

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
            _collider.PhysicsLayer = 1 << (int)PhysicsLayers.Trigger;
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.PlayerCollider);
            Entity.AddComponent(_collider);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.HasComponent<PlayerController>())
            {
                Emitter.Emit(ExitAreaEvents.Triggered);
                Game1.SceneManager.ChangeScene(TargetSceneType, _targetEntranceId);
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            //throw new NotImplementedException();
        }
    }

    public enum ExitAreaEvents
    {
        Triggered
    }
}
