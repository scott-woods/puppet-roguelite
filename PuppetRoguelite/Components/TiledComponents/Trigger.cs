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
    public abstract class Trigger : TiledComponent, ITriggerListener
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
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Collider);
            Entity.AddComponent(_collider);
        }

        public virtual void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == 1 << (int)PhysicsLayers.Collider)
            {
                Emitter.Emit(TriggerEventTypes.Triggered);
                HandleTriggered();
            }
        }

        public virtual void OnTriggerExit(Collider other, Collider local)
        {
            //throw new NotImplementedException();
        }

        public abstract void HandleTriggered();
    }

    public enum TriggerEventTypes
    {
        Triggered
    }
}
