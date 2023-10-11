using Nez;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class TextboxManager : SceneComponent
    {
        public Emitter<TextboxEvents> Emitter = new Emitter<TextboxEvents>();

        bool _active = false;
        int _page = 0;
        List<DialogueLine> _dialogueLines = new List<DialogueLine>();

        Textbox _textbox;

        public void DisplayTextbox(List<DialogueLine> lines)
        {
            if (!_active)
            {
                //set active
                _active = true;

                //dialogue line list
                _dialogueLines = lines;

                //create textbox
                _textbox = Scene.Camera.Entity.AddComponent(new Textbox());

                //start reading first line
                _textbox.ReadLine(_dialogueLines[_page]);

                //emit started event
                Emitter.Emit(TextboxEvents.TextboxOpened);
            }
        }

        public void CloseTextbox()
        {
            //reset everything
            _active = false;
            _dialogueLines.Clear();
            Scene.Camera.Entity.RemoveComponent(_textbox);
            _textbox = null;
            _page = 0;

            //emit closed event
            Emitter.Emit(TextboxEvents.TextboxClosed);
        }

        public override void Update()
        {
            base.Update();

            if (_active)
            {
                if (_textbox.IsFinishedReading && Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z))
                {
                    _page += 1;
                    if (_page < _dialogueLines.Count) //if there are more pages, read next page
                    {
                        _textbox.ReadLine(_dialogueLines[_page]);
                    }
                    else //otherwise, close textbox
                    {
                        CloseTextbox();
                    }
                }
                else if (!_textbox.IsFinishedReading && Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
                {
                    _textbox.SkipText();
                }
            }
        }
    }

    public enum TextboxEvents
    {
        TextboxOpened,
        TextboxClosed
    }
}
