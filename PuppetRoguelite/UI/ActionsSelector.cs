using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class ActionsSelector : UICanvas
    {
        Skin _basicSkin;
        Vector2 _anchorPosition;

        //elements
        Label _label;
        HorizontalGroup _actionGroup;

        Vector2 _offset = new Vector2(0, -20);

        public ActionsSelector(Vector2 anchorPosition)
        {
            _anchorPosition = anchorPosition;
        }

        public override void Initialize()
        {
            base.Initialize();

            _basicSkin = CustomSkins.CreateBasicSkin();
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            ArrangeElements();
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            var pos = Entity.Scene.Camera.WorldToScreenPoint(_anchorPosition + _offset);
            _actionGroup.SetPosition((pos.X * 2) - (_actionGroup.PreferredWidth / 2), pos.Y * 2);
        }

        void ArrangeElements()
        {
            _actionGroup = new HorizontalGroup().SetSpacing(4);
            _actionGroup.AddElement(new Button(_basicSkin, "attackActionButton"));
            _actionGroup.AddElement(new Button(_basicSkin, "toolActionButton"));
            Stage.AddElement(_actionGroup);
            //_actionGroup.SetScale(.75f);
        }
    }
}
