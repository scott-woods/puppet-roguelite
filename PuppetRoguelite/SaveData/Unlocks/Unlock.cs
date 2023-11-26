using Nez.Persistence;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Unlocks
{
    public class Unlock
    {
        public PlayerActionType Action;
        public bool IsUnlocked;

        //public Unlock(PlayerActionType action, bool isUnlocked)
        //{
        //    Action = action;
        //    IsUnlocked = isUnlocked;
        //}
    }
}
