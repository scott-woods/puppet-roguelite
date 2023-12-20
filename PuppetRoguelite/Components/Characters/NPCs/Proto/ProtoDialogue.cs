using Nez;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Support;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities;
using PuppetRoguelite.Models;
using PuppetRoguelite.SaveData.Upgrades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.NPCs.Proto
{
    public class ProtoDialogue
    {
        public static List<List<DialogueLine>> DefaultLines = new List<List<DialogueLine>>()
        {
            new List<DialogueLine>()
            {
                new DialogueLine("Using your Turn State can be more useful than just dealing out tons of Damage."),
                new DialogueLine("You can easily stun an attacking enemy by hitting them with your own Actions."),
                new DialogueLine("That's a lot easier than having to get up close with your Melee!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Remember, you can't be hit during your Action Execution."),
                new DialogueLine("So don't be afraid to be aggressive with it!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("While it may be tempting to activate your Turn as soon as you've got a couple AP..."),
                new DialogueLine("I'd recommend waiting until you've got a good amount."),
                new DialogueLine("This will allow you to perform even better combos, and get even more Dollahs!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Try your best to leave enemies alive to be dealt with during the Turn."),
                new DialogueLine("Enemies defeated by your Actions will drop a lot more Dollahs than with your Melee.")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Think carefully about the order of your Actions!"),
                new DialogueLine("Once you queue one up, you can't take it back."),
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Make sure you put some thought into when you activate your Turn."),
                new DialogueLine("You'll want to be in a good position to capitalize on your Actions."),
            },
            new List<DialogueLine>()
            {
                new DialogueLine("You can't keep any unused AP from one turn to the next."),
                new DialogueLine("Once you've activated the Turn, it's always better to do as much as you can in that Turn!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Any AP you have left at the end of an Encounter is discarded."),
                new DialogueLine("So don't try to save up your AP for the next room, since it'll be gone!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Once you start getting more Actions and Upgrades, your possibilities expand greatly."),
                new DialogueLine("You could even specialize in a certain area."),
                new DialogueLine("For example, you could just have a bunch of Support abilities."),
                new DialogueLine("Sure, you won't get as many Dollahs, but you'll be super powerful in normal combat!")
            }
        };

        public static List<DialogueLine> GetUpgradeRecommendation()
        {
            var hpUpgradeLines = new List<DialogueLine>()
            {
                new DialogueLine("Max HP is a pretty good idea. Especially if you're dying a lot."),
                new DialogueLine("You start with 8, but can get all the way up to 20."),
                new DialogueLine("Even just a few levels can help you out quite a bit.")
            };
            var apUpgradeLines = new List<DialogueLine>()
            {
                new DialogueLine("The Max AP Upgrade will greatly improve your combat ability."),
                new DialogueLine("Your combo opportunities will increase dramatically."),
                new DialogueLine("This should make fights a bit easier, not to mention more profitable!")
            };
            var attackSlotsLines = new List<DialogueLine>()
            {
                new DialogueLine("Getting more Attack Slots is pretty crucial, I'd say."),
                new DialogueLine("Of course, this is useless if you don't have any additional Attacks to use."),
                new DialogueLine("Make sure you check with the Action Shop first to see if you have multiple Attacks.")
            };
            var utilitySlotsLines = new List<DialogueLine>()
            {
                new DialogueLine("More Utility Slots could be a good idea, especially if you have more available."),
                new DialogueLine("Make sure you check with the Action Shop to see what you have access to first.")
            };
            var supportSlotsLines = new List<DialogueLine>()
            {
                new DialogueLine("More Support Slots could work, if you want to take a more buff-centric approach."),
                new DialogueLine("Make sure to check with the Action Shop to see which Support abilities you have available.")
            };

            var possibleLines = new List<List<DialogueLine>>();
            if (PlayerUpgradeData.Instance.MaxHpUpgrade.CurrentLevel == 0)
                possibleLines.Add(hpUpgradeLines);
            if (PlayerUpgradeData.Instance.MaxApUpgrade.CurrentLevel == 0)
                possibleLines.Add(apUpgradeLines);
            if (PlayerUpgradeData.Instance.AttackSlotsUpgrade.CurrentLevel == 0)
                possibleLines.Add(attackSlotsLines);
            if (PlayerUpgradeData.Instance.UtilitySlotsUpgrade.CurrentLevel == 0)
                possibleLines.Add(utilitySlotsLines);
            if (PlayerUpgradeData.Instance.SupportSlotsUpgrade.CurrentLevel == 0)
                possibleLines.Add(supportSlotsLines);

            return possibleLines.RandomItem();
        }

        public static List<DialogueLine> GetLinesForUnlock(PlayerActionType playerActionType)
        {
            List<DialogueLine> lines = new List<DialogueLine>();

            var type = playerActionType.ToType();

            switch (type.Name)
            {
                case nameof(DashAttack):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("Hey, you unlocked the Dash Attack!"),
                        new DialogueLine("That's a super versatile ability."),
                        new DialogueLine("Not only do you do decent damage, but it can help you get around, too!")
                    };
                    break;
                case nameof(Whirlwind):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("You unlocked Whirlwind, huh?"),
                        new DialogueLine("It may have a short range, but it deals decent damage around you, and it's cheap, too."),
                        new DialogueLine("It has tons of knockback, so keep that in mind.")
                    };
                    break;
                case nameof(ChainLightning):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("Looks like you unlocked the schematic for Chain Lightning!"),
                        new DialogueLine("That's one of my favorites. If you set it up right, it can clear entire rooms by itself."),
                    };
                    break;
                case nameof(Quickshot):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("You found a gun??"),
                        new DialogueLine("Just kidding, I know about your Quickshot ability."),
                        new DialogueLine("That's one of your few ranged options, and it's a lot of fun. Try it out!")
                    };
                    break;
                case nameof(Teleport):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("You found the Teleport schematic out there?"),
                        new DialogueLine("That's a super useful ability to have, really helps your other Actions shine.")
                    };
                    break;
                case nameof(StasisField):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("You found the Stasis Field schematics?"),
                        new DialogueLine("Nice! That can lock down enemies to help you better use your other abilities.")
                    };
                    break;
                case nameof(HealingAura):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("Ahh, you found the Healing Aura!"),
                        new DialogueLine("That's one of the only ways you can ever get some HP back, so I'd say it's quite vital!")
                    };
                    break;
                case nameof(AttackSpeedBoost):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("You found some kind of Attack Speed schematic?"),
                        new DialogueLine("Very cool! You can already attack pretty fast, so this oughta be.. interesting.")
                    };
                    break;
                case nameof(MoveSpeedBoost):
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("You found a Move Speed Boost schematic?"),
                        new DialogueLine("That's interesting! Moving around super fast has its uses, I'm sure.")
                    };
                    break;
                default:
                    lines = new List<DialogueLine>()
                    {
                        new DialogueLine("I don't recognize this... are you cheating?")
                    };
                    break;
            }

            return lines;
        }
    }
}
