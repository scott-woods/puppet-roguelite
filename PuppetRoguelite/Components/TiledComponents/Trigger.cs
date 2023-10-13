using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.Tiled;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    /// <summary>
    /// trigger created from tiled object
    /// </summary>
    public class Trigger : TiledComponent, ITriggerListener
    {
        public Emitter<TriggerEventTypes> Emitter = new Emitter<TriggerEventTypes>();

        //components
        BoxCollider _collider;

        public Trigger(TmxObject tmxTriggerObject, string mapId) : base(tmxTriggerObject, mapId)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            //collider
            _collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
            _collider.LocalOffset = new Vector2(TmxObject.Width / 2, TmxObject.Height / 2);
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Collider);
            Entity.AddComponent(_collider);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == 1 << (int)PhysicsLayers.Collider)
            {
                if (TmxObject.Properties != null)
                {
                    if (TmxObject.Properties.TryGetValue("TriggerType", out var triggerType))
                    {
                        switch (triggerType)
                        {
                            case nameof(TriggerType.EnemySpawn):
                                Emitter.Emit(TriggerEventTypes.EnemySpawnTriggered);
                                break;
                        }
                    }
                }
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            //throw new NotImplementedException();
        }
    }

    public enum TriggerEventTypes
    {
        EnemySpawnTriggered
    }
}
