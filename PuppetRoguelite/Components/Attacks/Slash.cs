using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Attacks
{
    public class Slash : IPlayerAttack
    {
        public string Name => "Slash";
        public int ApCost => 1;

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }
    }
}
