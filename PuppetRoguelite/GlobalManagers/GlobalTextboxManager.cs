using Nez;
using PuppetRoguelite.Models;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nez.Tweens.Easing;

namespace PuppetRoguelite.GlobalManagers
{
    public class GlobalTextboxManager
    {
        public static IEnumerator DisplayText(List<DialogueLine> lines)
        {
            var dialogueBox = Game1.Scene.CreateEntity("textbox-ui").AddComponent(new DialogueBox());
            yield return dialogueBox.DisplayText(lines);
            Game1.Schedule(1 / 240, timer =>
            {
                dialogueBox.Entity.Destroy();
            });
        }

        public static IEnumerator DisplayChoices(List<string> choices, Action<int> onChoiceSelected)
        {
            var dialogueBox = Game1.Scene.CreateEntity("textbox-ui").AddComponent(new DialogueBox());
            yield return Coroutine.WaitForSeconds(.5f);
            yield return dialogueBox.DisplayChoices(choices, onChoiceSelected);
            Game1.Schedule(1 / 240, timer =>
            {
                dialogueBox.Entity.Destroy();
            });
        }
    }
}
