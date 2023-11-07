using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Components.PlayerActions.Attacks;
using PuppetRoguelite.Components.PlayerActions.Support;
using PuppetRoguelite.Components.PlayerActions.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class PlayerData
    {
        public int Dollahs = 0;
        public int MaxHp = 8;
        public List<PlayerActionType> AttackActions = new List<PlayerActionType>() { PlayerActionType.FromType(typeof(DashAttack)) };
        public List<PlayerActionType> UtilityActions = new List<PlayerActionType>() { PlayerActionType.FromType(typeof(Teleport)) };
        public List<PlayerActionType> SupportActions = new List<PlayerActionType>() { PlayerActionType.FromType(typeof(HealingAura)) };
        public int MaxAttackSlots = 1;
        public int MaxUtilitySlots = 1;
        public int MaxSupportSlots = 1;
        public int MaxAp = 5;
        public int MaxDashes = 1;
        public float MovementSpeed = 130f;
    }
}
