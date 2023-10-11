using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public abstract class Item
    {
        public ItemType Type { get; set; }
        public string Name { get; set; }
    }
    
    public enum ItemType
    {
        Consumable,
        Key
    }
}
