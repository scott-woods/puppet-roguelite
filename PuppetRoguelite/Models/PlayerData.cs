using Nez.Persistence;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Components.PlayerActions.Attacks;
using PuppetRoguelite.Components.PlayerActions.Support;
using PuppetRoguelite.Components.PlayerActions.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class PlayerData
    {
        private static PlayerData _instance;

        public int Dollahs = 0;
        public List<PlayerActionType> AttackActions = new List<PlayerActionType>()
        { 
            PlayerActionType.FromType(typeof(ChainLightning)),
            PlayerActionType.FromType(typeof(DashAttack)),
            PlayerActionType.FromType(typeof(Whirlwind))
        };
        public List<PlayerActionType> UtilityActions = new List<PlayerActionType>()
        {
            PlayerActionType.FromType(typeof(Teleport)),
            PlayerActionType.FromType(typeof(StasisField))
        };
        public List<PlayerActionType> SupportActions = new List<PlayerActionType>()
        {
            PlayerActionType.FromType(typeof(HealingAura)),
            PlayerActionType.FromType(typeof(MoveSpeedBoost)),
            PlayerActionType.FromType(typeof(AttackSpeedBoost))
        };
        public int MaxDashes = 1;
        public float MovementSpeed = 150f;

        private PlayerData()
        {
            Game1.Emitter.AddObserver(Nez.CoreEvents.Exiting, OnExiting);
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
