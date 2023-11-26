using Nez;
using Nez.Systems;
using PuppetRoguelite.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerComponents
{
    public class DollahInventory : Component
    {
        public Emitter<DollahInventoryEvents, DollahInventory> Emitter = new Emitter<DollahInventoryEvents, DollahInventory>();

        int _dollahs;
        public int Dollahs
        {
            get => _dollahs;
            set
            {
                _dollahs = value;
                PlayerData.Instance.Dollahs = value;
                PlayerData.Instance.UpdateAndSave();
                Emitter.Emit(DollahInventoryEvents.DollahsChanged, this);
            }
        }

        public DollahInventory(int dollahs)
        {
            Dollahs = dollahs;
        }

        public void AddDollahs(int amount)
        {
            Dollahs += amount;
        }
    }

    public enum DollahInventoryEvents
    {
        DollahsChanged
    }
}
