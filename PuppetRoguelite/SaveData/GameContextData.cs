using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData
{
    public class GameContextData
    {
        private static GameContextData _instance;

        public bool HasCompletedIntro = false;

        private GameContextData()
        {
            Game1.Emitter.AddObserver(Nez.CoreEvents.Exiting, OnExiting);
        }

        public static GameContextData Instance
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
            File.WriteAllText("Data/gameContextData.json", json);
        }

        public void UpdateAndSave()
        {
            SaveData();
        }

        private static GameContextData LoadData()
        {
            if (File.Exists("Data/gameContextData.json"))
            {
                var json = File.ReadAllText("Data/gameContextData.json");
                _instance = Json.FromJson<GameContextData>(json);
            }
            else
            {
                _instance = new GameContextData();
            }

            return _instance;
        }

        void OnExiting()
        {
            SaveData();
        }
    }
}
