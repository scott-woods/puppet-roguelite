using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Menus
{
    /// <summary>
    /// Wrapper to set action key for any menus
    /// </summary>
    public abstract class UIMenu : UICanvas
    {
        protected Element BaseElement;
        protected Vector2 AnchorPosition;
        protected Vector2 ScreenSpaceOffset = Vector2.Zero;
        public Vector2 WorldSpaceOffset = Vector2.Zero;
        protected IGamepadFocusable DefaultFocus;
        protected IGamepadFocusable LastFocused;
        Action _cancelHandler;
        ICoroutine _waitCoroutine;

        public UIMenu(Action cancelHandler)
        {
            _cancelHandler = cancelHandler;
        }

        public override void Initialize()
        {
            base.Initialize();

            //render layer
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //set default accept key
            Stage.KeyboardActionKey = Microsoft.Xna.Framework.Input.Keys.E;

            //setup
            BaseElement = ArrangeElements();
            DefaultFocus = GetDefaultFocus();
        }

        public override void Update()
        {
            base.Update();

            //cancel if cancel key pressed
            if (Input.IsKeyPressed(Keys.X))
            {
                if (_waitCoroutine != null)
                {
                    _waitCoroutine.Stop();
                }

                _cancelHandler?.Invoke();
            }

            //position elements in world space
            if (BaseElement != null)
            {
                var pos = ResolutionHelper.GameToUiPoint(Entity, AnchorPosition + WorldSpaceOffset);
                BaseElement.SetPosition(pos.X + ScreenSpaceOffset.X, pos.Y + ScreenSpaceOffset.Y);
            }
        }

        public virtual void Show(Vector2 basePosition)
        {
            ValidateButtons();

            AnchorPosition = basePosition;

            Stage.AddElement(BaseElement);

            Stage.SetGamepadFocusElement(null);
            var focus = LastFocused != null ? LastFocused : DefaultFocus;
            Stage.SetGamepadFocusElement(focus);

            SetEnabled(true);

            _waitCoroutine = Game1.StartCoroutine(WaitForActionKeyReleased());
        }

        public void Hide()
        {
            RemoveHandlersFromButtons();
            BaseElement.Remove();
            Stage.UnfocusAll();

            SetEnabled(false);
        }

        IEnumerator WaitForActionKeyReleased()
        {
            while (Input.IsKeyDown(Keys.E) || Input.IsKeyReleased(Keys.E))
            {
                yield return null;
            }

            AddHandlersToButtons();
        }

        //public override void OnEnabled()
        //{
        //    base.OnEnabled();

        //    if (BaseElement != null)
        //    {
        //        Stage.AddElement(BaseElement);
        //    }
        //    else
        //    {
        //        //if somehow setup hasn't been done, do it here
        //        BaseElement = ArrangeElements();
        //    }
        //}

        //public override void OnDisabled()
        //{
        //    base.OnDisabled();

        //    if (BaseElement != null)
        //    {
        //        BaseElement.Remove();
        //    }
        //}

        protected void OnMenuButtonClicked(Button button)
        {
            LastFocused = button;
        }

        public abstract Element ArrangeElements();
        public abstract IGamepadFocusable GetDefaultFocus();
        public abstract void AddHandlersToButtons();
        public abstract void RemoveHandlersFromButtons();
        public abstract void ValidateButtons();
    }
}
