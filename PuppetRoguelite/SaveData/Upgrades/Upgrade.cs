using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public abstract class Upgrade
    {
        public string Name;
        public Dictionary<int, int> Levels;
        public int CurrentLevel = 0;

        protected Upgrade(string name)
        {
            Name = name;
            Levels = new Dictionary<int, int>();
            DefineLevels();
        }

        public int GetCurrentCost()
        {
            return Levels[CurrentLevel];
        }

        protected abstract void DefineLevels();
        public abstract string GetValueString();

        public virtual void ApplyUpgrade()
        {
            CurrentLevel += 1;
        }
    }
}
