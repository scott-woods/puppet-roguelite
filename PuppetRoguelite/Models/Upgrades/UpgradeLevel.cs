using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models.Upgrades
{
    public class UpgradeLevel
    {
        public int Level;
        public int Cost { get; set; }

        public UpgradeLevel(int level, int cost)
        {
            Level = level;
            Cost = cost;
        }
    }
}
