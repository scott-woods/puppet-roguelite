using Nez;
using PuppetRoguelite.Models;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class GlobalTextboxManager : GlobalManager
    {
        Entity _textboxEntity;
        DialogueBox _dialogueBox;

        public void ShowTextbox()
        {
            if (_textboxEntity != null)
                _textboxEntity.SetEnabled(true);
            else
            {
                _textboxEntity = Game1.Scene.CreateEntity("textbox-ui");
                _dialogueBox = _textboxEntity.AddComponent(new DialogueBox());
            }
        }

        public IEnumerator DisplayText(List<DialogueLine> lines)
        {
            yield return _dialogueBox.DisplayText(lines);
        }

        public IEnumerator DisplayChoices(List<string> choices, Action<int> onChoiceSelected)
        {
            yield return _dialogueBox.DisplayChoices(choices, onChoiceSelected);
        }

        public void HideTextbox()
        {
            _textboxEntity?.SetEnabled(false);
        }
    }
}
