using Nez;
using PuppetRoguelite.Components.Attacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    /// <summary>
    /// keep track of which attacks the player has available
    /// </summary>
    public class AttacksList : Component
    {
        public List<IPlayerAttack> AvailableAttacks;

        public AttacksList(List<IPlayerAttack> startingAttacks)
        {
            AvailableAttacks = startingAttacks;
        }

        public void AddAttack(IPlayerAttack attack)
        {
            AvailableAttacks.Add(attack);
        }

        public void RemoveAttack(IPlayerAttack attack)
        {
            AvailableAttacks.Remove(attack);
        }
    }
}
