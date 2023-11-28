using Nez.Console;
using PuppetRoguelite.Components.Characters.Enemies;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
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

        [Command("kill-all", "kills all enemies")]
        static void KillAllEnemies()
        {
            var enemies = Game1.Scene.FindComponentsOfType<EnemyBase>();

            foreach (var enemy in enemies)
            {
                if (enemy.Entity.TryGetComponent<HealthComponent>(out var hc))
                {
                    hc.Health = 0;
                }
            }
        }
    }
}
#endif