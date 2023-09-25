using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class HealthComponent : Component
    {
        public int Health;

        public HealthComponent(int health)
        {
            Health = health;
        }

        public int DecrementHealth(int amount)
        {
            Health = Health - amount > 0 ? Health - amount : 0;
            return Health;
        }

        /// <summary>
        /// returns true if health is 0 or less
        /// </summary>
        /// <returns></returns>
        public bool IsDepleted()
        {
            return Health <= 0;
        }
    }
}
