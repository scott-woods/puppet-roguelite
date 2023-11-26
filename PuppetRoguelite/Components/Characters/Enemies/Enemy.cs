using Nez;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies
{
    public abstract class Enemy : Component
    {
        public Entity MapEntity { get; set; }

        public Enemy(Entity mapEntity)
        {
            MapEntity = mapEntity;
        }
    }
}
