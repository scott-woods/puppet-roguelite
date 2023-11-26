using Nez.Persistence;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Support;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData
{
    public class PlayerData
    {
        private static PlayerData _instance;

        public int Dollahs = 0;
        public List<PlayerActionType> AttackActions = new List<PlayerActionType>()
        {
            PlayerActionType.FromType(typeof(DashAttack)),
        };
        public List<PlayerActionType> UtilityActions = new List<PlayerActionType>()
        {
            PlayerActionType.FromType(typeof(Teleport)),
        };
        public List<PlayerActionType> SupportActions = new List<PlayerActionType>()
        {
            PlayerActionType.FromType(typeof(HealingAura)),
        };
        public int MaxDashes = 1;
        public float MovementSpeed = 150f;

        private PlayerData()
        {
            Nez.Core.Emitter.AddObserver(Nez.CoreEvents.Exiting, OnExiting);
        }

        public static PlayerData Instance
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

        public void SaveData()
        {
            var settings = JsonSettings.HandlesReferences;
            settings.TypeNameHandling = TypeNameHandling.All;

            var json = Json.ToJson(this, settings);
            File.WriteAllText("Data/playerData.json", json);
        }

        public void UpdateAndSave()
        {
            SaveData();
        }

        private static PlayerData LoadData()
        {
            if (File.Exists("Data/playerData.json"))
            {
                var json = File.ReadAllText("Data/playerData.json");
                _instance = Json.FromJson<PlayerData>(json);
            }
            else
            {
                _instance = new PlayerData();
            }

            return _instance;
        }

        void OnExiting()
        {
            SaveData();
        }
    }
}
