using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerComponents;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public class MaxApUpgrade : Upgrade<int>
    {
        public MaxApUpgrade() : base("Max AP")
        {

        }

        protected override void DefineLevels()
        {
            Levels.Add(new UpgradeLevel<int>(1, 0, 5));
            Levels.Add(new UpgradeLevel<int>(2, 75, 6));
            Levels.Add(new UpgradeLevel<int>(3, 150, 7));
            Levels.Add(new UpgradeLevel<int>(4, 225, 8));
            Levels.Add(new UpgradeLevel<int>(5, 300, 9));
            Levels.Add(new UpgradeLevel<int>(6, 400, 10));
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<ActionPointComponent>(out var ac))
            {
                ac.MaxActionPoints = GetCurrentValue();
            }
        }
    }
}
