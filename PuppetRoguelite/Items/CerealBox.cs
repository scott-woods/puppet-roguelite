using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Items
{
    public class CerealBox : Item
    {
        public CerealBox(string name)
        {
            Name = name;
            Type = ItemType.Key;
        }
    }
}
