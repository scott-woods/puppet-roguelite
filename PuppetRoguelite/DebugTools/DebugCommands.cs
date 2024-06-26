﻿using Nez.Console;
using PuppetRoguelite.Components.Characters.Enemies;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.SaveData.Unlocks;
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

        [Command("tgm", "Disable player's hurtbox")]
        static void ToggleGodMode()
        {
            PlayerController.Instance.Hurtbox.SetEnabled(!PlayerController.Instance.Hurtbox.Enabled);
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

        [Command("unlock-all-actions", "unlocks every possible action")]
        static void UnlockAllActions()
        {
            foreach (var unlock in ActionUnlockData.Instance.Unlocks)
            {
                if (!unlock.IsUnlocked)
                {
                    ActionUnlockData.Instance.UnlockAction(unlock.Action);
                    ActionUnlockData.Instance.UpdateAndSave();
                }
            }
        }

        [Command("add-dollahs", "adds the specified amount of dollahs")]
        static void AddDollahs(int amount)
        {
            PlayerController.Instance.DollahInventory.AddDollahs(amount);
        }

        [Command("free-actions", "toggles actions to have 0 cost")]
        static void ToggleFreeActions()
        {
            Game1.DebugSettings.FreeActions = !Game1.DebugSettings.FreeActions;
        }

        [Command("set-hp", "set player's hp to the given amount")]
        static void SetPlayerHealth(int health)
        {
            PlayerController.Instance.HealthComponent.Health = health;
        }

        [Command("unlock-boss", "unlock the boss gate")]
        static void UnlockBossGate()
        {
            var bossGate = Game1.Scene.FindComponentOfType<BossGate>();
            if (bossGate != null)
            {
                bossGate.AddKey();
                bossGate.AddKey();
            }
        }
    }
}
#endif