using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
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
    public class ActionsSelector : UIMenu
    {
        Skin _basicSkin;
        Vector2 _anchorPosition;

        //elements
        Label _label;
        HorizontalGroup _actionGroup;
        ActionButton _attackButton;
        ActionButton _toolButton;
        ActionButton _executeButton;

        Vector2 _offset = new Vector2(0, -20);

        TurnHandler _turnHandler;
        AttacksMenu _attacksMenu;

        public ActionsSelector(Vector2 anchorPosition, TurnHandler turnHandler, AttacksMenu attacksMenu,
            bool shouldMaintainFocusedElement = false) : base(shouldMaintainFocusedElement)
        {
            _anchorPosition = anchorPosition;
            _turnHandler = turnHandler;
            _attacksMenu = attacksMenu;
        }

        public override void Initialize()
        {
            base.Initialize();

            _basicSkin = CustomSkins.CreateBasicSkin();
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            ArrangeElements();
            DefaultElement = _attackButton;
            SetEnabled(false);
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition + _offset);
            _actionGroup.SetPosition(pos.X - (_actionGroup.PreferredWidth / 2), pos.Y);
        }

        void ArrangeElements()
        {
            _actionGroup = new HorizontalGroup().SetSpacing(4);
            _attackButton = _actionGroup.AddElement(new ActionButton(this, _basicSkin, "attackActionButton"));
            _attackButton.OnClicked += OnAttackButtonClicked;
            _toolButton = _actionGroup.AddElement(new ActionButton(this, _basicSkin, "toolActionButton"));
            _toolButton.OnClicked += OnToolButtonClicked;
            _executeButton = _actionGroup.AddElement(new ActionButton(this, _basicSkin, "executeButton"));
            _executeButton.OnClicked += OnExecuteButtonClicked;
            Stage.AddElement(_actionGroup);
        }

        void OnAttackButtonClicked(Button obj)
        {
            _turnHandler.OpenMenu(_attacksMenu);
        }

        void OnToolButtonClicked(Button obj)
        {
            throw new NotImplementedException();
        }

        void OnExecuteButtonClicked(Button obj)
        {
            _turnHandler.ExecuteActions();
        }
    }
}
