using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    /// <summary>
    /// contains Level, Cost to get to this Level, and the Value of this Level
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpgradeLevel<T>
    {
        public int Level;
        public int Cost;
        public T Value;

        public UpgradeLevel(int level, int cost, T value)
        {
            Level = level;
            Cost = cost;
            Value = value;
        }
    }
}
