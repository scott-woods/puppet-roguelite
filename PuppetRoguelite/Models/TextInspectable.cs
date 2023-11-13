using Nez;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    public class TextInspectable : InspectableModel
    {
        List<List<DialogueLine>> _dialogueTree;

        public TextInspectable(List<List<DialogueLine>> dialogueTree)
        {
            _dialogueTree = dialogueTree;
        }

        public override IEnumerator HandleInspection(int interactionCount)
        {
            //get which list of lines to display
            var linesIndex = Math.Min(_dialogueTree.Count - 1, interactionCount);
            var lines = _dialogueTree[linesIndex];
            var textboxManager = Game1.Scene.GetOrCreateSceneComponent<TextboxManager>();
            yield return textboxManager.DisplayTextbox(lines);
        }
    }
}
