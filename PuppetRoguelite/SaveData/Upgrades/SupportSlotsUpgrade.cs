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
    public class SupportSlotsUpgrade : Upgrade
    {
        public Dictionary<int, int> Values = new Dictionary<int, int>()
        {
            {0, 1 },
            {1, 2 },
            {2, 3 }
        };

        public SupportSlotsUpgrade() : base("Support Slots")
        {

        }

        public int GetCurrentValue()
        {
            return Values[CurrentLevel];
        }

        protected override void DefineLevels()
        {
            Levels.Add(0, 250);
            Levels.Add(1, 500);
            Levels.Add(2, 750);
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<ActionsManager>(out var manager))
            {
                manager.MaxSupportSlots = GetCurrentValue();
            }
        }

        public override string GetValueString()
        {
            return GetCurrentValue().ToString();
        }
    }
}
