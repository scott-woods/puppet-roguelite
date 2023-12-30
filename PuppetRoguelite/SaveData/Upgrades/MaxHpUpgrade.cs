using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public class MaxHpUpgrade : Upgrade<int>
    {
        public MaxHpUpgrade() : base("Max HP")
        {

        }

        protected override void DefineLevels()
        {
            Levels.Add(new UpgradeLevel<int>(1, 0, 8));
            Levels.Add(new UpgradeLevel<int>(2, 50, 10));
            Levels.Add(new UpgradeLevel<int>(3, 100, 12));
            Levels.Add(new UpgradeLevel<int>(4, 175, 15));
            Levels.Add(new UpgradeLevel<int>(5, 250, 18));
            Levels.Add(new UpgradeLevel<int>(6, 350, 20));
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.SetMaxHealth(GetCurrentValue(), true);
            }
        }
    }
}
