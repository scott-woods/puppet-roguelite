using Nez.Persistence;
using Nez;
using PuppetRoguelite.Models.Upgrades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Components.PlayerActions.Attacks;
using PuppetRoguelite.Components.PlayerActions.Support;
using PuppetRoguelite.Components.PlayerActions.Utilities;

namespace PuppetRoguelite.Models
{
    public class ActionUnlockData
    {
        private static ActionUnlockData _instance;

        public List<Unlock> Unlocks = new List<Unlock>
        {
            new Unlock { Action = PlayerActionType.FromType(typeof(DashAttack)), IsUnlocked = true },
            new Unlock { Action = PlayerActionType.FromType(typeof(ChainLightning)), IsUnlocked = false },
            new Unlock { Action = PlayerActionType.FromType(typeof(Whirlwind)), IsUnlocked = false },
            new Unlock { Action = PlayerActionType.FromType(typeof(Teleport)), IsUnlocked = true },
            new Unlock { Action = PlayerActionType.FromType(typeof(StasisField)), IsUnlocked = false },
            new Unlock { Action = PlayerActionType.FromType(typeof(HealingAura)), IsUnlocked = true },
            new Unlock { Action = PlayerActionType.FromType(typeof(MoveSpeedBoost)), IsUnlocked = true },
            new Unlock { Action = PlayerActionType.FromType(typeof(AttackSpeedBoost)), IsUnlocked = true },
            //new Unlock(PlayerActionType.FromType(typeof(ChainLightning)), false),
            //new Unlock(PlayerActionType.FromType(typeof(Whirlwind)), false),
            //new Unlock(PlayerActionType.FromType(typeof(Teleport)), true),
            //new Unlock(PlayerActionType.FromType(typeof(StasisField)), false),
            //new Unlock(PlayerActionType.FromType(typeof(HealingAura)), true),
        };

        private ActionUnlockData()
        {
            Game1.Emitter.AddObserver(CoreEvents.Exiting, OnExiting);
        }

        public static ActionUnlockData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadData();
                }
                return _instance;
            }
        }

        public void UpdateAndSave()
        {
            SaveData();
        }

        public void SaveData()
        {
            var settings = JsonSettings.HandlesReferences;
            settings.TypeNameHandling = TypeNameHandling.All;

            var json = Json.ToJson(this, settings);
            File.WriteAllText("Data/actionUnlockData.json", json);
        }

        private static ActionUnlockData LoadData()
        {
            if (File.Exists("Data/actionUnlockData.json"))
            {
                var json = File.ReadAllText("Data/actionUnlockData.json");
                _instance = Json.FromJson<ActionUnlockData>(json);
            }
            else
            {
                _instance = new ActionUnlockData();
            }

            return _instance;
        }

        void OnExiting()
        {
            SaveData();
        }
    }
}
