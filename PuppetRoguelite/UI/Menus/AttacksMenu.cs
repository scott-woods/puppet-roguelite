using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.PlayerActions;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
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

            //get skin
            _skin = CustomSkins.CreateBasicSkin();

            //determine anchor pos
            _anchorPosition = ResolutionHelper.GameToUiPoint(Entity, _playerPosition + _offset);

            //disabled by default
            SetEnabled(false);
        }

        public override void OnEnabled()
        {
            ArrangeElements();

            //set default focus element
            var contentTable = _dialog.GetContentTable();
            DefaultElement = contentTable.GetCells().FirstOrDefault((c) =>
            {
                var button = c.GetElement<Button>();
                if (button == null || button.GetDisabled())
                {
                    return false;
                }
                return true;
            })?.GetElement<Button>();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (_dialog != null)
            {
                _dialog.Remove();
            }

            base.OnDisabled();
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
            foreach (var attackType in options)
            {
                //check affordability
                var apCostColor = Color.White;
                var disabled = false;
                var apCost = PlayerActionUtils.GetApCost(attackType);
                if (apCost > Player.Instance.ActionPointComponent.ActionPoints)
                {
                    apCostColor = Color.Gray;
                    disabled = true;
                }

                //create button
                var button = new ListButton(this, PlayerActionUtils.GetName(attackType), _skin, "listButton");
                button.SetDisabled(disabled);
                button.OnClicked += button => _turnHandler.HandleActionSelected(attackType);
                contentTable.Add(button).SetExpandX().Left().SetSpaceRight(32);

                //add cost label
                contentTable.Add(new Label(PlayerActionUtils.GetApCost(attackType).ToString(), new LabelStyle(Graphics.Instance.BitmapFont, apCostColor))).SetExpandX().Right();
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
