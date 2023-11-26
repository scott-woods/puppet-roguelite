using Nez;
using Nez.Persistence;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Support;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PuppetRoguelite.SaveData.Unlocks
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
            new Unlock { Action = PlayerActionType.FromType(typeof(MoveSpeedBoost)), IsUnlocked = false },
            new Unlock { Action = PlayerActionType.FromType(typeof(AttackSpeedBoost)), IsUnlocked = false },
            //new Unlock(PlayerActionType.FromType(typeof(ChainLightning)), false),
            //new Unlock(PlayerActionType.FromType(typeof(Whirlwind)), false),
            //new Unlock(PlayerActionType.FromType(typeof(Teleport)), true),
            //new Unlock(PlayerActionType.FromType(typeof(StasisField)), false),
            //new Unlock(PlayerActionType.FromType(typeof(HealingAura)), true),
        };

        private ActionUnlockData()
        {
            Core.Emitter.AddObserver(CoreEvents.Exiting, OnExiting);
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

        public void UnlockAction(PlayerActionType action)
        {
            var unlock = Unlocks.FirstOrDefault(u => u.Action == action);
            if (unlock != null)
            {
                unlock.IsUnlocked = true;
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
