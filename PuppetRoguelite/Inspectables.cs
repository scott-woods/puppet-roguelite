using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public static class Inspectables
    {
        private static Dictionary<string, InspectableModel> _inspectables = new Dictionary<string, InspectableModel>();

        public static void Initialize()
        {
            _inspectables.Add("hub_player_bed", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("Go to sleep?"),
                    new DialogueLine("Just kidding, there's no animation for that, silly.")
                }
            }));

            _inspectables.Add("hub_player_nightstand", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("There's an incredibly vile, almost toxic looking liquid on your nightstand."),
                    new DialogueLine("Oh wait. It's just Mountain Dew Code Red...")
                },
                new List<DialogueLine>
                {
                    new DialogueLine("Don't you know better than to drink soda before bed?")
                }
            }));

            _inspectables.Add("hub_player_bookshelf", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("There's no books on your bookshelf."),
                    new DialogueLine("No cereal either.")
                }
            }));

            _inspectables.Add("hub_dining_table", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("The table is in great condition, except for the pizza cutter clipping through the top."),
                }
            }));

            _inspectables.Add("hub_empty_chest", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("Sadly, the chest is lootless."),
                    new DialogueLine("You're just a lootless little guy, huh?")
                },
            }));

            _inspectables.Add("hub_upgrade_shop_bed", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("It seems the shopkeeper prefers to sleep directly on the foundation instead of a mattress..."),
                    new DialogueLine("Looks like he still has some pillows, awkwardly positioned at the top of the bed."),
                    new DialogueLine("It's as if it was, somehow, physically impossible to position them in any other way."),
                    new DialogueLine("That must've been really frustrating.")
                },
            }));

            _inspectables.Add("hub_upgrade_shop_bookshelf", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("Plenty of shelf space, but not a single book. Some people just want to look smart."),
                    new DialogueLine("Huh? There's a crumpled up box of cereal on the floor, about a millimeter taller than the shelf gap.")
                },
            }));

            _inspectables.Add("hub_upgrade_shop_desk", new TextInspectable(new List<List<DialogueLine>>
            {
                new List<DialogueLine>
                {
                    new DialogueLine("The desk is covered in scratches and littered with various sheets of paper."),
                },
            }));
        }

        public static InspectableModel GetInspectableById(string id)
        {
            if (_inspectables.TryGetValue(id, out InspectableModel inspectableModel))
            {
                return inspectableModel;
            }
            else return null;
        }
    }
}
