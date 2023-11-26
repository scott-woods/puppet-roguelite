using Nez;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerComponents
{
    /// <summary>
    /// keep track of which attacks the player has available
    /// </summary>
    public class AttacksList : Component
    {
        public List<Type> AvailableAttackTypes;

        public AttacksList(List<Type> startingAttacks)
        {
            foreach (var type in startingAttacks)
            {
                if (!typeof(PlayerAction).IsAssignableFrom(type))
                    throw new ArgumentException($"Type {type.FullName} does not implement IPlayerAttack");
            }
            AvailableAttackTypes = startingAttacks;
        }

        public void AddAttack(Type attack)
        {
            if (!typeof(PlayerAction).IsAssignableFrom(attack))
                throw new ArgumentException($"Type {attack.FullName} does not implement IPlayerAttack");
            AvailableAttackTypes.Add(attack);
        }

        public void RemoveAttack(Type attack)
        {
            AvailableAttackTypes.Remove(attack);
        }
    }
}
