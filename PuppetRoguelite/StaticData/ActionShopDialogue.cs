using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.StaticData
{
    public class ActionShopDialogue
    {
        public static List<List<DialogueLine>> DefaultLines = new List<List<DialogueLine>>()
        {
            new List<DialogueLine>()
            {
                new DialogueLine("Howdy sweet pea! Let's take a gander at yer gear...")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Goodness, have you grown since the last time I saw you?")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Ya lookin' to switch things up? Let's go over your options!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("...what's that, dearie? Speak up now!")
            },
        };

        public static List<List<DialogueLine>> ExitLines = new List<List<DialogueLine>>()
        {
            new List<DialogueLine>()
            {
                new DialogueLine("Thanks for stopping by, dear!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("You be careful out there now, y'hear?")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("Let me know if anything breaks!")
            },
            new List<DialogueLine>()
            {
                new DialogueLine("That's a fine looking arsenal of violent abilities you've assembled there, sweetie!")
            }
        };
    }
}
