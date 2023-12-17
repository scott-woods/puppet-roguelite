using Nez;
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
            Game1.GlobalTextboxManager.ShowTextbox();
            yield return Game1.GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Would you like to revisit the Tutorial?")
            });

            var choice = -1;
            yield return Game1.GlobalTextboxManager.DisplayChoices(new List<string> { "Yes", "No" }, choiceIndex => choice = choiceIndex);

            switch (choice)
            {
                case 0:
                    Game1.GlobalTextboxManager.HideTextbox();
                    yield return Coroutine.WaitForSeconds(.5f);
                    Game1.SceneManager.ChangeScene(typeof(TutorialRoom), "0");
                    break;
                case 1:
                    Game1.GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                    {
                        new DialogueLine("Just come on back if you need a refresher!")
                    });
                    Game1.GlobalTextboxManager.HideTextbox();
                    break;
            }
        }
    }
}
