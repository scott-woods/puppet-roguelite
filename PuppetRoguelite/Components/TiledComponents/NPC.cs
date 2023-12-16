using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.NPCs.Proto;
using PuppetRoguelite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class NPC : TiledComponent
    {
        public NPC(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if (TmxObject.Properties.TryGetValue("Name", out var name))
            {
                var npcEntity = Entity.Scene.AddEntity(new PausableEntity("npc"));
                npcEntity.SetPosition(Entity.Position);
                switch (name)
                {
                    case "Proto":
                        npcEntity.AddComponent(new Proto());
                        break;
                }
            }

            Entity.Destroy();
        }
    }
}
