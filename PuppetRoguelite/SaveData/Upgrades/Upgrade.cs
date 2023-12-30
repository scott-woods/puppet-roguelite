using Nez.Persistence;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public abstract class Upgrade<T> : UpgradeBase
    {
        /// <summary>
        /// List of Level data for this upgrade
        /// </summary>
        [JsonExclude]
        public List<UpgradeLevel<T>> Levels;

        protected Upgrade(string name) : base(name)
        {
            Levels = new List<UpgradeLevel<T>>();
            DefineLevels();
        }

        /// <summary>
        /// get cost to upgrade. returns -1 if already at max level
        /// </summary>
        /// <returns></returns>
        public override int GetCostToUpgrade()
        {
            if (!IsMaxLevel())
            {
                return Levels.FirstOrDefault(l => l.Level == CurrentLevel + 1).Cost;
            }
            else return -1;
        }

        /// <summary>
        /// get value for current level
        /// </summary>
        /// <returns></returns>
        public T GetCurrentValue()
        {
            return Levels.FirstOrDefault(l => l.Level == CurrentLevel).Value;
        }

        /// <summary>
        /// Get value by level. returns 0 if level is invalid
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public T GetValueByLevel(int level)
        {
            if (Levels.Any(l => l.Level == level))
                return Levels.FirstOrDefault(l => l.Level == level).Value;
            else return default;
        }

        /// <summary>
        /// increments the CurrentLevel by 1
        /// </summary>
        public override void ApplyUpgrade()
        {
            if (!IsMaxLevel())
            {
                PlayerController.Instance.DollahInventory.Dollahs -= GetCostToUpgrade();
                CurrentLevel += 1;
            }
        }

        public override string GetValueString(int? level = null)
        {
            level ??= CurrentLevel;
            return GetValueByLevel(level.Value).ToString();
        }

        /// <summary>
        /// returns true if upgrade is at max level
        /// </summary>
        /// <returns></returns>
        public override bool IsMaxLevel()
        {
            return !Levels.Any(l => l.Level > CurrentLevel);
        }

        protected abstract void DefineLevels();
    }
}
