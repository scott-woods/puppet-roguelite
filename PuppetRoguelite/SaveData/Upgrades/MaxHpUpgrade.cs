using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public class MaxHpUpgrade : Upgrade
    {
        public Dictionary<int, int> MaxHpValues = new Dictionary<int, int>()
        {
            {0, 8 },
            {1, 10 },
            {2, 12 },
            {3, 15 },
            {4, 18 },
            {5, 20 },
        };

        public MaxHpUpgrade() : base("Max HP")
        {

        }

        public int GetCurrentMaxHp()
        {
            return MaxHpValues[CurrentLevel];
        }

        protected override void DefineLevels()
        {
            Levels.Add(0, 50);
            Levels.Add(1, 100);
            Levels.Add(2, 175);
            Levels.Add(3, 250);
            Levels.Add(4, 350);
            Levels.Add(5, 500);
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.SetMaxHealth(GetCurrentMaxHp(), true);
            }
        }

        public override string GetValueString()
        {
            return GetCurrentMaxHp().ToString();
        }
    }
}
