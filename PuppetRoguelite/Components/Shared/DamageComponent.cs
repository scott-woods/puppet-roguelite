using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class DamageComponent : Component
    {
        public int Damage = 0;

        public DamageComponent(int damage)
        {
            Damage = damage;
        }
    }
}
