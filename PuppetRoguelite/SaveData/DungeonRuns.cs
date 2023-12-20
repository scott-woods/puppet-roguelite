using Nez;
using Nez.Persistence;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SaveData
{
    public class DungeonRuns
    {
        public List<DungeonRun> Runs = new List<DungeonRun>();

        private DungeonRun _currentRun;
        private float _startTime;

        private static DungeonRuns _instance;
        public static DungeonRuns Instance
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

        public DungeonRuns()
        {
            Core.Emitter.AddObserver(CoreEvents.Exiting, OnExiting);
        }

        public void SaveData()
        {
            var settings = JsonSettings.HandlesReferences;
            settings.TypeNameHandling = TypeNameHandling.All;

            var json = Json.ToJson(this, settings);
            File.WriteAllText("Data/dungeonRuns.json", json);
        }

        public void UpdateAndSave()
        {
            SaveData();
        }

        private static DungeonRuns LoadData()
        {
            if (File.Exists("Data/dungeonRuns.json"))
            {
                var json = File.ReadAllText("Data/dungeonRuns.json");
                _instance = Json.FromJson<DungeonRuns>(json);
            }
            else
            {
                _instance = new DungeonRuns();
            }

            return _instance;
        }

        public void StartNewRun()
        {
            _currentRun = new DungeonRun();
            _startTime = Time.TotalTime;
        }

        public void EndRun()
        {
            if (IsRunInProgress())
            {
                //get duration
                var duration = Time.TotalTime - _startTime;
                _currentRun.Duration = duration;

                //add run to list
                Runs.Add(_currentRun);

                //null everything out
                _currentRun = null;
                _startTime = 0;

                //save
                SaveData();
            }
        }

        public bool IsRunInProgress()
        {
            return _currentRun != null;
        }

        public void AddDurationToRun(float duration)
        {
            if (IsRunInProgress())
            {
                _currentRun.Duration += duration;
            }
        }

        public void AddDollahsToRun(int  dollahs)
        {
            if (IsRunInProgress())
            {
                _currentRun.DollahsEarned += dollahs;
            }
        }

        public void SetRunUnlockedAction(PlayerActionType playerActionType)
        {
            if (IsRunInProgress())
            {
                _currentRun.UnlockedAction = playerActionType;
            }
        }

        void OnExiting()
        {
            EndRun();
        }
    }

    public class DungeonRun
    {
        public float Duration;
        public int DollahsEarned;
        public PlayerActionType UnlockedAction;
    }
}
