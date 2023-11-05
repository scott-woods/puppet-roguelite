using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions
{
    public interface IPlayerAction
    {
        void Prepare();
        void Execute(bool isSimulation = false);
    }

    
}
