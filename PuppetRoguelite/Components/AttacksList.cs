﻿using Nez;
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
        public List<Type> AvailableAttackTypes;

        public AttacksList(List<Type> startingAttacks)
        {
            foreach (var type in startingAttacks)
            {
                if (!typeof(IPlayerAttack).IsAssignableFrom(type))
                    throw new ArgumentException($"Type {type.FullName} does not implement IPlayerAttack");
            }
            AvailableAttackTypes = startingAttacks;
        }

        public void AddAttack(Type attack)
        {
            if (!typeof(IPlayerAttack).IsAssignableFrom(attack))
                throw new ArgumentException($"Type {attack.FullName} does not implement IPlayerAttack");
            AvailableAttackTypes.Add(attack);
        }

        public void RemoveAttack(Type attack)
        {
            AvailableAttackTypes.Remove(attack);
        }
    }
}
