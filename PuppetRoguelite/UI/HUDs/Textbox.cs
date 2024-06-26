﻿using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Models;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Elements;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.HUDs
{
    public class Textbox : CustomCanvas
    {
        float _horizontalPad = 8;
        float _spacing = 2;
        float _innerBoxPad = 40f;
        float _innerBoxSpacing = 10f;

        public bool IsFinishedReading = false;

        Table _table;
        WindowTable _box;
        Table _innerTable;
        Label _text;
        Label _asterisk;

        float _boxWidth { get => Game1.UIResolution.X * .7f; }
        float _boxHeight { get => Game1.UIResolution.Y * .3f; }
        float _textMaxWidth { get => _innerBoxWidth - _asterisk.GetWidth() - _innerBoxSpacing; }

        float _innerBoxWidth { get => _boxWidth - _innerBoxPad * 2; }

        Skin _basicSkin;

        ICoroutine _readTextCoroutine;
        string _currentText;

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //base table
            _table = Stage.AddElement(new Table()).Bottom();
            //_table.SetFillParent(true);
            _table.SetWidth(Game1.UIResolution.X);
            _table.SetHeight(Game1.UIResolution.Y);

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            ArrangeElements();
        }

        void ArrangeElements()
        {
            //textbox table
            _box = new WindowTable(_basicSkin);
            var vPad = Game1.UIResolution.Y * .05f;
            _table.Add(_box).Expand().Bottom().SetPadBottom(vPad).Width(_boxWidth).Height(_boxHeight);

            //inner table
            _innerTable = new Table();
            _box.Add(_innerTable).Grow().Pad(_innerBoxPad);

            //asterisk
            _asterisk = new Label("*", _basicSkin, "default_xxxl");
            _innerTable.Add(_asterisk).SetExpandY().Top().Left();
            _asterisk.Pack();

            _text = new Label("", _basicSkin, "default_xxxl").SetAlignment(Align.TopLeft);
            _innerTable.Add(_text).Grow().Top().Left().SetSpaceLeft(_innerBoxSpacing);

            _innerTable.Pack();
        }

        public IEnumerator ReadLine(DialogueLine line)
        {
            _readTextCoroutine = Core.StartCoroutine(ReadText(GetWrappedText(line.Text, _textMaxWidth)));
            yield return _readTextCoroutine;
        }

        IEnumerator ReadText(string text)
        {
            IsFinishedReading = false;
            _currentText = text;

            int count = 1;
            float characterInterval = .05f;
            float timer = 0;

            while (count <= text.Length)
            {
                timer += Time.DeltaTime;

                if (timer >= characterInterval)
                {
                    _text.SetText(text.Substring(0, count));

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
            Log.Debug("Skipping Textbox Text");

            _readTextCoroutine?.Stop();
            _readTextCoroutine = null;
            _text.SetText(_currentText);
            IsFinishedReading = true;
        }
    }
}
