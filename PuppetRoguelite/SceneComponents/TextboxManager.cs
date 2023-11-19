using Nez;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.Models;
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
                _textbox = Scene.CreateEntity("textbox-ui").AddComponent(new Textbox());
                //_textbox = Scene.Camera.AddComponent(new Textbox());

                //emit started event
                Emitter.Emit(TextboxEvents.TextboxOpened);

                yield return _textbox.ReadLine(_dialogueLines[_page]);

                while (IsActive)
                {
                    yield return null;
                }

                //read
                //while (_page < _dialogueLines.Count)
                //{
                //    //read and wait until finished
                //    yield return _textbox.ReadLine(_dialogueLines[_page]);

                //    //wait until z key is pressed
                //    while (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                //    {
                //        yield return null;
                //    }

                //    //increment page
                //    _page += 1;
                //}

                ////close
                //CloseTextbox();
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

            if (IsActive && Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
            {
                if (_textbox.IsFinishedReading)
                {
                    _page++;
                    if (_page < _dialogueLines.Count)
                    {
                        Game1.StartCoroutine(_textbox.ReadLine(_dialogueLines[_page]));
                    }
                    else
                    {
                        CloseTextbox();
                    }
                }
                else
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
