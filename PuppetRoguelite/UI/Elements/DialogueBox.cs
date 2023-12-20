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
        //constants
        const float _windowTablePad = 50f;

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
            _windowTable.SetSize(_mainTable.GetWidth() * .7f, 216 + (_windowTablePad * 2));
            _windowTable.Pad(_windowTablePad);
            var vPad = Game1.UIResolution.Y * .05f;
            _mainTable.Add(_windowTable).Expand().Bottom().SetPadBottom(vPad).Width(Value.PercentWidth(1f)).Height(Value.PercentHeight(1f));

            //inner table
            _innerTable = new Table();
            _windowTable.Add(_innerTable).Grow();
        }

        #region TEXT DISPLAY

        public IEnumerator DisplayText(List<DialogueLine> lines)
        {
            //asterisk
            var asterisk = new Label("*", _basicSkin, "default_xxxl");
            _innerTable.Add(asterisk).SetExpandY().Top().Left().SetSpaceRight(10f);

            //text section
            var text = new Label("", _basicSkin, "default_xxxl").SetAlignment(Align.TopLeft);
            _innerTable.Add(text).Grow().Top().Left();

            _innerTable.Pack();
            var maxWidth = _windowTable.GetWidth() - _windowTable.GetPadX() - _innerTable.GetWidth();

            foreach (var line in lines)
            {
                //create a copy label with the text
                var labelCopy = new Label("", text.GetStyle());
                labelCopy.SetVisible(false);

                var sb = new StringBuilder();
                var words = line.Text.Split(' ');
                foreach (var word in words)
                {
                    labelCopy.SetText(sb.ToString() + word + " ");
                    labelCopy.Pack();

                    if (labelCopy.GetWidth() < maxWidth)
                        sb.Append(word + " ");
                    else
                        sb.Append("\n" + word + " ");
                }

                var wrappedText = sb.ToString();

                _readTextCoroutine = Game1.StartCoroutine(ReadText(wrappedText, text));
                yield return _readTextCoroutine;

                yield return null;

                while (!Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                    yield return null;
            }

            //clear everything
            _innerTable.Clear();
        }

        IEnumerator ReadText(string text, Label textLabel)
        {
            //yield one frame so we don't immediately skip text
            yield return null;

            int count = 1;
            float characterInterval = .04f;
            float soundInterval = .05f;
            float timer = 0;
            float soundTimer = 0;

            while (count <= text.Length)
            {
                //check if we should skip text
                if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E))
                {
                    textLabel.SetText(text);
                    break;
                }

                //increment timers
                timer += Time.DeltaTime;
                soundTimer += Time.DeltaTime;

                //when timer reachers character interval, add character
                if (timer >= characterInterval)
                {
                    textLabel.SetText(text.Substring(0, count));

                    //play sound if enough time has passed since last sound
                    if (soundTimer >= soundInterval)
                    {
                        Game1.AudioManager.PlaySound(Content.Audio.Sounds.Default_text);
                        soundTimer = 0;
                    }

                    //set timer to 0 and increment counter
                    timer = 0;
                    count++;

                    //adjust characterInterval if the last character was a comma for extra pause
                    characterInterval = textLabel.GetText().Last() == ',' ? .2f : .04f;
                }

                yield return null;
            }
        }

        #endregion

        #region CHOICES DISPLAY

        public IEnumerator DisplayChoices(List<string> choices, Action<int> onChoiceSelected)
        {
            //yield one frame so we don't immediatley select
            yield return null;

            List<BulletPointSelector> selectors = new List<BulletPointSelector>();
            foreach (var choice in choices)
            {
                var selector = new BulletPointSelector(choice, _basicSkin, "default_xxxl");
                selector.OnSelected += OnChoiceSelected;
                _innerTable.Add(selector).Grow();
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
