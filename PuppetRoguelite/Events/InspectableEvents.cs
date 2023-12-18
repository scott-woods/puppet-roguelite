using Nez;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Events
{
    public class InspectableEvents
    {
        public static IEnumerator HubShelf()
        {
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Would you like to revisit the Tutorial?")
            });

            var choice = -1;
            yield return GlobalTextboxManager.DisplayChoices(new List<string> { "Yes", "No" }, choiceIndex => choice = choiceIndex);

            switch (choice)
            {
                case 0:
                    yield return Coroutine.WaitForSeconds(.5f);
                    Game1.SceneManager.ChangeScene(typeof(TutorialRoom), "0");
                    break;
                case 1:
                    GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                    {
                        new DialogueLine("Just come on back if you need a refresher!")
                    });
                    break;
            }
        }
    }
}
