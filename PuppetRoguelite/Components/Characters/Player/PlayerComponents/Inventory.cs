using Nez;
using PuppetRoguelite.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerComponents
{
    public class Inventory : Component
    {
        List<Item> _items = new List<Item>();

        public void AddItem(Item item)
        {
            _items.Add(item);
        }

        public void RemoveItem(Item item)
        {
            _items.Remove(item);
        }

        public List<Item> GetItems()
        {
            return _items;
        }
    }
}
