using Nez.Console;
using PuppetRoguelite.Components.Characters.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
namespace PuppetRoguelite.DebugTools
{
    public partial class DebugConsole
    {
        [Command("tcl", "Disable player's collision")]
        static void TogglePlayerCollider()
        {
            PlayerController.Instance.Collider.SetEnabled(!PlayerController.Instance.Collider.Enabled);
        }
    }
}
#endif