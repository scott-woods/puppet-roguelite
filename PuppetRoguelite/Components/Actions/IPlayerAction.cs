using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Actions
{
    public interface IPlayerAction
    {
        string Name { get; }
        int ApCost { get; }
        void Prepare();
        void Execute();
    }
}
