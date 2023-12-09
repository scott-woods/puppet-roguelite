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
    public class UtilitySlotsUpgrade : Upgrade
    {
        public Dictionary<int, int> Values = new Dictionary<int, int>()
        {
            {0, 1 },
            {1, 2 },
            {2, 3 }
        };

        public UtilitySlotsUpgrade() : base("Utility Slots")
        {

        }

        public int GetCurrentValue()
        {
            return Values[CurrentLevel];
        }

        protected override void DefineLevels()
        {
            Levels.Add(0, 150);
            Levels.Add(1, 300);
            Levels.Add(2, 500);
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<ActionsManager>(out var manager))
            {
                manager.MaxUtilitySlots = GetCurrentValue();
            }
        }

        public override string GetValueString()
        {
            return GetCurrentValue().ToString();
        }
    }
}
