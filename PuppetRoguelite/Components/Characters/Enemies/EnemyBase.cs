using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Enemies
{
    public abstract class EnemyBase : Component
    {
        public Entity MapEntity { get; set; }
    }
}
