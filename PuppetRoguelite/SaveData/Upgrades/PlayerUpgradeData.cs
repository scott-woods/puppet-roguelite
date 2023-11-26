using Nez;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData.Upgrades
{
    public class PlayerUpgradeData
    {
        private static PlayerUpgradeData _instance;

        public MaxHpUpgrade MaxHpUpgrade = new MaxHpUpgrade();
        public MaxApUpgrade MaxApUpgrade = new MaxApUpgrade();
        public AttackSlotsUpgrade AttackSlotsUpgrade = new AttackSlotsUpgrade();
        public UtilitySlotsUpgrade UtilitySlotsUpgrade = new UtilitySlotsUpgrade();
        public SupportSlotsUpgrade SupportSlotsUpgrade = new SupportSlotsUpgrade();

        private PlayerUpgradeData()
        {
            Core.Emitter.AddObserver(CoreEvents.Exiting, OnExiting);
        }

        public static PlayerUpgradeData Instance
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
            File.WriteAllText("Data/playerUpgradeData.json", json);
        }

        private static PlayerUpgradeData LoadData()
        {
            if (File.Exists("Data/playerUpgradeData.json"))
            {
                var json = File.ReadAllText("Data/playerUpgradeData.json");
                _instance = Json.FromJson<PlayerUpgradeData>(json);
            }
            else
            {
                _instance = new PlayerUpgradeData();
            }

            return _instance;
        }

        void OnExiting()
        {
            SaveData();
        }
    }
}
