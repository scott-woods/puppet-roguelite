using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.StaticData
{
    public class UpgradeShopDialogue
    {
        public static List<List<DialogueLine>> DefaultLines = new List<List<DialogueLine>>()
        {
            new List<DialogueLine>()
            {
                new DialogueLine("Need to make some modifications?")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("I know it's in these pants somewhere... Oh! Hi! What can I upgrade for you?")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("How're you holdin' up, kid?")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Gettin' too tough out there? Let's see if we can fix that.")
            },
        };

        public static List<DialogueLine> ExitNoPurchase = new List<DialogueLine>()
        {
            new DialogueLine("Come on back if you wanna make some improvements!")
        };

        public static List<DialogueLine> ExitWithPurchase = new List<DialogueLine>()
        {
            new DialogueLine("Thanks for the Dollahs, kid! Now don't let my hard work go to waste, got it?")
        };

        public static List<DialogueLine> HighWealth = new List<DialogueLine>()
        {
            new DialogueLine("Someone's been savin' up, huh? With that kinda money, I can get you runnin' in tip-top shape!")
        };

        public static List<DialogueLine> MediumWealth = new List<DialogueLine>()
        {
            new DialogueLine("Looks like you've got a few extra Dollahs on ya, you wanna get something nice?")
        };

        public static List<DialogueLine> LowWealth = new List<DialogueLine>()
        {
            new DialogueLine("Yeesh, kid. You're lookin' a bit light on Dollahs there..."),
            new DialogueLine("Let me check the Bargin Bin, see if I've got anything..")
        };
    }
}
