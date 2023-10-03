using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.PlayerActions;
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
                contentTable.Add(new Label(_labelHeader, new LabelStyle(Graphics.Instance.BitmapFont, Color.Black))).SetColspan(colSpan).SetExpandX().Right();
            }

            contentTable.Row();

            //buttons
            foreach(var pair in _content)
            {
                contentTable.Add(pair.Key).SetExpandX().Left().SetSpaceRight(32);
                contentTable.Add(pair.Value).SetExpandX().Right();
            }

            //var options = Player.Instance.AttacksList.AvailableAttackTypes;
            //contentTable.Add(new Label("Cost", new LabelStyle(Graphics.Instance.BitmapFont, Color.Black))).SetColspan(2).SetExpandX().Right();
            //contentTable.Row();
            //foreach (var attackType in options)
            //{
            //    //check affordability
            //    var apCostColor = Color.White;
            //    var disabled = false;
            //    var apCost = PlayerActionUtils.GetApCost(attackType);
            //    if (apCost > Player.Instance.ActionPointComponent.ActionPoints)
            //    {
            //        apCostColor = Color.Gray;
            //        disabled = true;
            //    }

            //    //create button
            //    var button = new ListButton(this, PlayerActionUtils.GetName(attackType), _skin, "listButton");
            //    button.SetDisabled(disabled);
            //    button.OnClicked += button => _turnHandler.HandleActionSelected(attackType);
            //    contentTable.Add(button).SetExpandX().Left().SetSpaceRight(32);

            //    //add cost label
            //    contentTable.Add(new Label(PlayerActionUtils.GetApCost(attackType).ToString(), new LabelStyle(Graphics.Instance.BitmapFont, apCostColor))).SetExpandX().Right();
            //    contentTable.Row();
            //}

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
