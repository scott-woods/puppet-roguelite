﻿using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.StaticData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class Inspectable : TiledComponent
    {
        public BoxCollider Collider;
        public Interactable Interactable;

        public Inspectable(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
            
        }

        public override void Initialize()
        {
            base.Initialize();

            Collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, TmxObject.Height));
            Collider.IsTrigger = true;
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
            Interactable = Entity.AddComponent(new Interactable(OnInteracted));
        }

        IEnumerator OnInteracted()
        {
            if (TmxObject.Properties.TryGetValue("Id", out var id))
            {
                var model = Inspectables.GetInspectableById(id);
                if (model != null)
                {
                    yield return model.HandleInspection(Interactable.InteractionCount);
                }
            }

            yield break;
        }
    }
}
