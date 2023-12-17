using Nez;
using Nez.UI;
using PuppetRoguelite.Models;
using PuppetRoguelite.StaticData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class DialogueBox : CustomCanvas
    {
        //elements
        Table _mainTable;
        WindowTable _windowTable;
        Table _innerTable;

        //skin
        Skin _basicSkin;

        //sizes
        float _boxWidth { get => Game1.UIResolution.X * .7f; }
        float _boxHeight { get => Game1.UIResolution.Y * .3f; }

        //coroutines
        ICoroutine _readTextCoroutine;

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;
            Stage.KeyboardActionKey = Microsoft.Xna.Framework.Input.Keys.E;

            //base table
            _mainTable = Stage.AddElement(new Table()).Bottom();
            _mainTable.SetWidth(Game1.UIResolution.X);
            _mainTable.SetHeight(Game1.UIResolution.Y);

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            ArrangeElements();
        }

        void ArrangeElements()
        {
            //textbox table
            _windowTable = new WindowTable(_basicSkin);
            _windowTable.SetSize(_mainTable.GetWidth() * .7f, _mainTable.GetHeight() * .3f);
            var vPad = Game1.UIResolution.Y * .05f;
            _mainTable.Add(_windowTable).Expand().Bottom().SetPadBottom(vPad).Width(Value.PercentWidth(1f)).Height(Value.PercentHeight(1f));

            //inner table
            _innerTable = new Table();
            _windowTable.Add(_innerTable).Grow().Pad(Value.PercentWidth(.025f, _windowTable));
        }

        #region TEXT DISPLAY

        public IEnumerator DisplayText(List<DialogueLine> lines)
        {
            //asterisk
            var asterisk = new Label("*", _basicSkin, "default_xxxl");
            _innerTable.Add(asterisk).SetExpandY().Top().Left();

            //text section
            var text = new Label("", _basicSkin, "default_xxxl").SetAlignment(Align.TopLeft);
            _innerTable.Add(text).Grow().Top().Left().SetSpaceLeft(5f);

            _innerTable.Pack();
            var maxWidth = _mainTable.GetCell(_windowTable).GetMaxWidth() - _windowTable.GetCell(_innerTable).GetPadX() - _innerTable.GetWidth();

            foreach (var line in lines)
            {
                //create a copy label with the text
                var labelCopy = new Label("", text.GetStyle());
                labelCopy.SetVisible(false);

                //Loop through text and keep track of last space
                var lastSpaceIndex = 0;
                for (int i = 1; i < line.Text.Length; i++)
                {
                    if (line.Text[i - 1] == ' ')
                    {
                        lastSpaceIndex = i - 1;
                    }

                    //Set text and pack to get new width
                    labelCopy.SetText(line.Text.Substring(0, i));
                    labelCopy.Pack();

                    //If we go past the label cell's max width, add a line break at the last space
                    if (labelCopy.GetWidth() > maxWidth)
                    {
                        line.Text = line.Text.Remove(lastSpaceIndex, 1).Insert(lastSpaceIndex, "\n");
                    }
                }

                _readTextCoroutine = Game1.StartCoroutine(ReadText(line.Text, text));
                yield return _readTextCoroutine;

                yield return null;

                while (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                    yield return null;

                //clear everything
                _innerTable.Clear();
            }
        }

        IEnumerator ReadText(string text, Label textLabel)
        {
            int count = 1;
            float characterInterval = .05f;
            float timer = 0;

            //yield one frame so we don't immediately skip text
            yield return null;

            while (count <= text.Length)
            {
                if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                {
                    textLabel.SetText(text);
                    break;
                }

                timer += Time.DeltaTime;

                if (timer >= characterInterval)
                {
                    textLabel.SetText(text.Substring(0, count));

                    //play sound
                    Game1.AudioManager.PlaySound(Content.Audio.Sounds.Default_text);

                    //set timer to 0 and increment counter
                    timer = 0;
                    count++;

                    //adjust characterInterval if the last character was a comma for extra pause
                    characterInterval = count < text.Length && text[count - 1] == ',' ? .3f : .06f;
                }

                yield return null;
            }
        }

        #endregion

        #region OPTIONS

        public IEnumerator DisplayChoices(List<string> choices, Action<int> onChoiceSelected)
        {
            //yield one frame so we don't immediatley select
            yield return null;

            List<BulletPointSelector> selectors = new List<BulletPointSelector>();
            foreach (var choice in choices)
            {
                var selector = new BulletPointSelector(choice, _basicSkin, "default_xxxl");
                selector.OnSelected += OnChoiceSelected;
                _innerTable.Add(selector).Expand();
                selectors.Add(selector);
            }

            Stage.SetGamepadFocusElement(selectors.First());

            var index = -1;
            void OnChoiceSelected(string selectedText)
            {
                index = choices.IndexOf(selectedText);
            }

            while (index == -1)
                yield return null;

            onChoiceSelected.Invoke(index);

            _innerTable.Clear();
        }

        #endregion
    }
}
