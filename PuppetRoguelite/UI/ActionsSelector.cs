using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
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

            _basicSkin = Skin.CreateDefaultSkin();
            IsFullScreen = false;

            ArrangeElements();
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            var pos = Entity.Scene.Camera.WorldToScreenPoint( _anchorPosition + _offset );
            _actionGroup.SetPosition(pos.X - (_actionGroup.PreferredWidth / 2), pos.Y);
        }

        void ArrangeElements()
        {
            _actionGroup = new HorizontalGroup().SetSpacing(4);
            _actionGroup.AddElement(new Label("t", _basicSkin));
            _actionGroup.AddElement(new Label("e", _basicSkin));
            _actionGroup.AddElement(new Label("s", _basicSkin));
            _actionGroup.AddElement(new Label("t", _basicSkin));
            Stage.AddElement(_actionGroup);
            //_label = new Label("test", _basicSkin);
            //label.SetPosition(100, 100);
            //Stage.AddElement(_label);
            //var dialog = new Dialog("test", _basicSkin);
            //Stage.AddElement(dialog);
        }
    }
}
