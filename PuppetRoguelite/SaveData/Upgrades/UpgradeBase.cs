using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public abstract class UpgradeBase
    {
        /// <summary>
        /// the label of the Upgrade
        /// </summary>
        [JsonExclude]
        public string Name;

        /// <summary>
        /// current level (starts at 1)
        /// </summary>
        public int CurrentLevel = 1;

        protected UpgradeBase(string name)
        {
            Name = name;
        }

        public abstract string GetValueString(int? level = null);

        public abstract bool IsMaxLevel();
        public abstract int GetCostToUpgrade();
        public abstract void ApplyUpgrade();
    }
}
