using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Actions;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class AttacksMenu : UIMenu
    {
        Vector2 _offset = new Vector2(-20, 0);

        Vector2 _playerPosition;
        TurnHandler _turnHandler;

        Vector2 _anchorPosition;

        //pagination
        int _page = 0;
        int _maxPerPage = 4;

        //elements
        ListBox<string> _listBox;
        Dialog _dialog;

        Skin _skin;

        public AttacksMenu(Vector2 playerPosition, TurnHandler turnHandler,
            bool shouldMaintainFocusedElement = false) : base(shouldMaintainFocusedElement)
        {
            _playerPosition = playerPosition;
            _turnHandler = turnHandler;
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);
        }

        public override void Initialize()
        {
            base.Initialize();

            _skin = CustomSkins.CreateBasicSkin();
            _anchorPosition = ResolutionHelper.GameToUiPoint(Entity, _playerPosition + _offset);
            ArrangeElements();
            var contentTable = _dialog.GetContentTable();
            DefaultElement = contentTable.GetCells().FirstOrDefault(c => c.GetElement<Button>() != null).GetElement<Button>();
            SetEnabled(false);
        }

        void ArrangeElements()
        {
            //create dialog box
            _dialog = new Dialog("Attacks", _skin);
            _dialog.SetDebug(false);

            //dialog content
            var contentTable = _dialog.GetContentTable();
            contentTable.Pad(10).Top();
            contentTable.Defaults().Space(8);
            var options = Player.Instance.AttacksList.AvailableAttackTypes;
            contentTable.Add(new Label("Cost", new LabelStyle(Graphics.Instance.BitmapFont, Color.Black))).SetColspan(2).SetExpandX().Right();
            contentTable.Row();
            foreach(var attackType in options)
            {
                var button = new ListButton(this, PlayerActionUtils.GetName(attackType), _skin);
                button.OnClicked += button => _turnHandler.HandleActionSelected(attackType);
                contentTable.Add(button).SetExpandX().Left().SetSpaceRight(32);
                contentTable.Add(new Label(PlayerActionUtils.GetApCost(attackType).ToString(), new LabelStyle(Graphics.Instance.BitmapFont, Color.White))).SetExpandX().Right();
                contentTable.Row();
            }

            //dialog buttons
            if (options.Count > _maxPerPage)
            {
                var buttonTable = _dialog.GetButtonTable();
                _dialog.GetCell(buttonTable).SetExpandX().SetFillX();
                buttonTable.Pad(10);
                buttonTable.Add(new TextButton("Back", _skin)).SetExpandX().Left();
                buttonTable.Add(new TextButton("Next", _skin)).SetExpandX().Right();
            }

            //size to fit children
            _dialog.Pack();

            //add to stage and set position
            Stage.AddElement(_dialog);
            _dialog.SetPosition(_anchorPosition.X - _dialog.PreferredWidth, _anchorPosition.Y - _dialog.PreferredHeight);
        }

        void SetPage(int newPage)
        {
            _page = newPage;
        }
    }
}
