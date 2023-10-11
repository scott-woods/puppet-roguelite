using Nez;
using Nez.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class Textbox : UICanvas
    {
        float _horizontalPad = 8;

        public bool IsFinishedReading = false;

        Table _table;
        Table _box;
        Label _text;
        Label _asterisk;

        Skin _basicSkin;

        ICoroutine _readTextCoroutine;
        string _currentText;

        public override void Initialize()
        {
            base.Initialize();

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);
            _table.SetDebug(false);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            ArrangeElements();
        }

        void ArrangeElements()
        {
            //var stack = new Stack();
            //var width = (float)(Math.Round((480 * .8f) / 48) * 48);
            //var height = (float)(Math.Round((270 * .38f) / 48) * 48);
            //_table.Add(stack).Expand().Width(width).Height(height).Bottom();
            //_table.SetDebug(true);

            //create textbox
            _box = new Table();
            _box.Top().Left();
            _box.Defaults().SetPadTop(4).SetPadBottom(4).SetSpaceRight(2);
            _box.SetBackground(_basicSkin.GetNinePatchDrawable("np_inventory_01"));
            var width = 480 * .8f;
            var height = (float)Math.Round(270 * .38f);
            _table.Add(_box).Expand().Width(width).Height(height).Bottom();
            _box.SetDebug(true);

            //asterisk
            _asterisk = new Label("*", _basicSkin);
            _box.Add(_asterisk).SetExpandY().Top().Left().SetPadLeft(_horizontalPad);

            _text = new Label("", _basicSkin).SetAlignment(Align.TopLeft);
            _box.Add(_text).Grow().Top().Left().SetPadRight(_horizontalPad);
        }

        public void ReadLine(DialogueLine line)
        {
            var maxWidth = _table.GetCell(_box).GetPrefWidth() - _box.GetCell(_asterisk).GetPrefWidth() - 16 - (_horizontalPad * 2);
            _readTextCoroutine = Game1.StartCoroutine(ReadText(GetWrappedText(line.Text, maxWidth)));
        }

        IEnumerator ReadText(string text)
        {
            IsFinishedReading = false;
            _currentText = text;

            int count = 1;
            while (count <= text.Length)
            {
                _text.SetText(text.Substring(0, count));

                //TODO: sound

                //Set frames to wait to 2, or 30 if the last character was a comma for extra pause
                var framesToWait = text[count - 1] == ',' ? 1000f : 200f;

                //yield
                yield return Coroutine.WaitForSeconds(framesToWait * Time.DeltaTime);

                //increment counter
                count++;
            }

            IsFinishedReading = true;
        }

        string GetWrappedText(string text, float maxWidth)
        {
            //create a copy label with the text
            var labelCopy = new Label("", _text.GetStyle());
            labelCopy.SetVisible(false);

            //Loop through text and keep track of last space
            var lastSpaceIndex = 0;
            for (int i = 1; i < text.Length; i++)
            {
                if (text[i - 1] == ' ')
                {
                    lastSpaceIndex = i - 1;
                }

                //Set text and pack to get new width
                labelCopy.SetText(text.Substring(0, i));
                labelCopy.Pack();

                //If we go past the label cell's max width, add a line break at the last space
                if (labelCopy.GetWidth() > maxWidth)
                {
                    text = text.Remove(lastSpaceIndex, 1).Insert(lastSpaceIndex, "\n");
                }
            }

            return text;
        }

        public void SkipText()
        {
            _readTextCoroutine.Stop();
            _text.SetText(_currentText);
            IsFinishedReading = true;
        }
    }
}
