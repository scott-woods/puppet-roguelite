using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class SelectionDialog : Dialog
    {
        const int _maxPerPage = 4;

        Dictionary<ListButton, Label> _content;
        string _buttonHeader, _labelHeader;
        Skin _skin;

        public SelectionDialog(Dictionary<ListButton, Label> content, string title, Skin skin, string styleName = null,
            string buttonHeader = null, string labelHeader = null) : base(title, skin, styleName)
        {
            _content = content;
            _buttonHeader = buttonHeader;
            _labelHeader = labelHeader;
            _skin = skin;

            Setup();
        }

        void Setup()
        {
            //content table settings
            var contentTable = GetContentTable();
            contentTable.Pad(10).Top();
            contentTable.Defaults().Space(8);

            //headers
            var colSpan = String.IsNullOrWhiteSpace(_buttonHeader) || String.IsNullOrWhiteSpace(_labelHeader) ? 2 : 1;
            if (!String.IsNullOrWhiteSpace(_buttonHeader))
            {
                contentTable.Add(new Label(_buttonHeader, new LabelStyle(Graphics.Instance.BitmapFont, Color.Black))).SetColspan(colSpan).SetExpandX().Left();
            }
            if (!String.IsNullOrWhiteSpace(_labelHeader))
            {
                contentTable.Add(new Label(_labelHeader, _skin, "abaddon_24")).SetColspan(colSpan).SetExpandX().Right();
            }

            //buttons
            foreach(var pair in _content)
            {
                contentTable.Row();
                contentTable.Add(pair.Key).SetExpandX().Left().SetSpaceRight(32);
                contentTable.Add(pair.Value).SetExpandX().Right();
            }

            //dialog buttons
            if (_content.Count > _maxPerPage)
            {
                var buttonTable = GetButtonTable();
                GetCell(buttonTable).SetExpandX().SetFillX();
                buttonTable.Pad(10);
                buttonTable.Add(new TextButton("Back", _skin)).SetExpandX().Left();
                buttonTable.Add(new TextButton("Next", _skin)).SetExpandX().Right();
            }

            //size to fit children
            Pack();
        }
    }
}
