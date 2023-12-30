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
    public class UtilitySlotsUpgrade : Upgrade<int>
    {
        public UtilitySlotsUpgrade() : base("Utility Slots")
        {

        }

        protected override void DefineLevels()
        {
            Levels.Add(new UpgradeLevel<int>(1, 0, 1));
            Levels.Add(new UpgradeLevel<int>(2, 150, 2));
            Levels.Add(new UpgradeLevel<int>(3, 300, 3));
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<ActionsManager>(out var manager))
            {
                manager.MaxUtilitySlots = GetCurrentValue();
            }
        }
    }
}
