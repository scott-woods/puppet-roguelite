using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    /// <summary>
    /// Wrapper to set action key for any menus
    /// </summary>
    public abstract class UIMenu : UICanvas, IUIMenu
    {
        protected Skin _defaultSkin;
        protected Element _baseElement;
        protected Vector2 _anchorPosition;
        protected Vector2 _screenSpaceOffset = Vector2.Zero;

        public override void Initialize()
        {
            base.Initialize();

            //render layer
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //set default accept key
            Stage.KeyboardActionKey = Microsoft.Xna.Framework.Input.Keys.E;

            //load skin
            _defaultSkin = CustomSkins.CreateBasicSkin();

            //setup
            _baseElement = ArrangeElements();
            ValidateButtons();
            DetermineDefaultElement();
            DeterminePosition();
        }

        public override void Update()
        {
            base.Update();

            //position elements in world space
            if (_baseElement != null)
            {
                var pos = ResolutionHelper.GameToUiPoint(Entity, _anchorPosition);
                _baseElement.SetPosition(pos.X + _screenSpaceOffset.X, pos.Y + _screenSpaceOffset.Y);
            }
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            if (_baseElement != null)
            {
                Stage.AddElement(_baseElement);
            }
            else
            {
                //if somehow setup hasn't been done, do it here
                _baseElement = ArrangeElements();
                ValidateButtons();
                DetermineDefaultElement();
                DeterminePosition();
            }
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            if (_baseElement != null)
            {
                _baseElement.Remove();
            }
        }

        public abstract Element ArrangeElements();
        public abstract void ValidateButtons();
        public abstract void DetermineDefaultElement();
        public abstract void DeterminePosition();
    }
}
