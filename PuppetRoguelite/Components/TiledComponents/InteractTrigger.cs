using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class InteractTrigger : TiledComponent
    {
        //components
        Collider _collider;
        Interactable _interactable;

        //misc
        string _eventName;
        bool _shouldDestroy;

        public InteractTrigger(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            //collider
            _collider = new BoxCollider(TmxObject.Width, TmxObject.Height);
            _collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.PlayerCollider);
            Entity.AddComponent(_collider);

            //interactable
            _interactable = Entity.AddComponent(new Interactable(OnInteracted));

            if (TmxObject.Properties != null && TmxObject.Properties.Count > 0)
            {
                if (TmxObject.Properties.TryGetValue("EventName", out string eventName))
                {
                    _eventName = eventName;
                }
                if (TmxObject.Properties.TryGetValue("ShouldDestroy", out string shouldDestroy))
                {
                    _shouldDestroy = true;
                }
            }
        }

        IEnumerator OnInteracted()
        {
            yield return HandleTriggered();
            if (_shouldDestroy)
                Entity.Destroy();
        }

        public virtual IEnumerator HandleTriggered()
        {
            if (!string.IsNullOrWhiteSpace(_eventName))
            {
                yield return Game1.EventManager.PlayEvent(_eventName);
            }
        }
    }
}
