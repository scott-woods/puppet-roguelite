using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models.Upgrades
{
    public class MaxApUpgrade : Upgrade
    {
        public Dictionary<int, int> MaxApValues = new Dictionary<int, int>()
        {
            {0, 5 },
            {1, 6 },
            {2, 7 },
            {3, 8 },
            {4, 9 },
            {5, 10 },
        };

        public MaxApUpgrade() : base("Max AP")
        {

        }

        public int GetCurrentMaxAp()
        {
            return MaxApValues[CurrentLevel];
        }

        protected override void DefineLevels()
        {
            Levels.Add(0, 100);
            Levels.Add(1, 200);
            Levels.Add(2, 300);
            Levels.Add(3, 400);
            Levels.Add(4, 500);
            Levels.Add(5, 600);
        }

        public override void ApplyUpgrade()
        {
            base.ApplyUpgrade();

            if (PlayerController.Instance.Entity.TryGetComponent<ActionPointComponent>(out var ac))
            {
                ac.MaxActionPoints = GetCurrentMaxAp();
            }
        }

        public override string GetValueString()
        {
            return GetCurrentMaxAp().ToString();
        }
    }
}
