using Nez;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class TextboxManager : SceneComponent
    {
        public Emitter<TextboxEvents> Emitter = new Emitter<TextboxEvents>();

        public bool IsActive = false;
        int _page = 0;
        List<DialogueLine> _dialogueLines = new List<DialogueLine>();

        Textbox _textbox;

        public IEnumerator DisplayTextbox(List<DialogueLine> lines)
        {
            if (!IsActive)
            {
                //set active
                IsActive = true;

                //dialogue line list
                _dialogueLines = lines;

                //create textbox
                _textbox = Scene.Camera.Entity.AddComponent(new Textbox());

                //emit started event
                Emitter.Emit(TextboxEvents.TextboxOpened);

                //read
                while (_page < _dialogueLines.Count)
                {
                    //read and wait until finished
                    yield return _textbox.ReadLine(_dialogueLines[_page]);

                    //wait until z key is pressed
                    while (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                    {
                        yield return null;
                    }

                    //increment page
                    _page += 1;
                }

                //close
                CloseTextbox();
            }
        }

        public void CloseTextbox()
        {
            //reset everything
            IsActive = false;
            //_dialogueLines.Clear();
            Scene.Camera.Entity.RemoveComponent(_textbox);
            _textbox = null;
            _page = 0;

            //emit closed event
            Emitter.Emit(TextboxEvents.TextboxClosed);
        }

        public override void Update()
        {
            base.Update();

            if (IsActive && !_textbox.IsFinishedReading)
            {
                if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                {
                    _textbox.SkipText();
                }
                //if (_textbox.IsFinishedReading && Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Z))
                //{
                //    _page += 1;
                //    if (_page < _dialogueLines.Count) //if there are more pages, read next page
                //    {
                //        _textbox.ReadLine(_dialogueLines[_page]);
                //    }
                //    else //otherwise, close textbox
                //    {
                //        CloseTextbox();
                //    }
                //}
                //else if (!_textbox.IsFinishedReading && Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.X))
                //{
                //    _textbox.SkipText();
                //}
            }
        }
    }

    public enum TextboxEvents
    {
        TextboxOpened,
        TextboxClosed
    }
}
