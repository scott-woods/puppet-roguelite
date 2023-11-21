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
    public class SelectionDialog : Table
    {
        const int _maxPerPage = 4;

        Dictionary<ListButton, Label> _content;
        string _buttonHeader, _labelHeader;
        Skin _skin;

        public SelectionDialog(Dictionary<ListButton, Label> content, Skin skin,
            string buttonHeader = null, string labelHeader = null)
        {
            _content = content;
            _buttonHeader = buttonHeader;
            _labelHeader = labelHeader;
            _skin = skin;

            Setup();
        }

        void Setup()
        {
            //table settings
            SetBackground(_skin.GetNinePatchDrawable("np_inventory_01"));
            Pad(40).Top();
            Defaults().Space(20);

            //headers
            var colSpan = String.IsNullOrWhiteSpace(_buttonHeader) || String.IsNullOrWhiteSpace(_labelHeader) ? 2 : 1;
            if (!String.IsNullOrWhiteSpace(_buttonHeader))
            {
                Add(new Label(_buttonHeader, new LabelStyle(Graphics.Instance.BitmapFont, Color.Black))).SetColspan(colSpan).SetExpandX().Left();
            }
            if (!String.IsNullOrWhiteSpace(_labelHeader))
            {
                Add(new Label(_labelHeader, _skin, "default_md")).SetColspan(colSpan).SetExpandX().Right();
            }

            //buttons
            foreach(var pair in _content)
            {
                Row();
                Add(pair.Key).SetExpandX().Left().SetSpaceRight(32);
                Add(pair.Value).SetExpandX().Right();
            }

            //dialog buttons
            //if (_content.Count > _maxPerPage)
            //{
            //    var buttonTable = GetButtonTable();
            //    GetCell(buttonTable).SetExpandX().SetFillX();
            //    buttonTable.Pad(10);
            //    buttonTable.Add(new TextButton("Back", _skin)).SetExpandX().Left();
            //    buttonTable.Add(new TextButton("Next", _skin)).SetExpandX().Right();
            //}

            //size to fit children
            Pack();
        }
    }
}
