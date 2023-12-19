using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class GrowButton : Table, IGamepadFocusable, IInputListener
    {
        public event Action OnPressed;

        Image _image;
        float _defaultScale;
        float _growFactor;

        ElementJitter _jitter;

        public GrowButton(SpriteDrawable drawable, float defaultScale, float growFactor)
        {
            _defaultScale = defaultScale;
            _growFactor = growFactor;
            _image = new Image(drawable);
            _image.SetScale(defaultScale);
            Add(_image);

            SetTouchable(Touchable.Enabled);

            _jitter = new ElementJitter(this, 1f, .05f);
        }

        #region IGamepadFocusable

        public bool ShouldUseExplicitFocusableControl { get; set; }
        public IGamepadFocusable GamepadUpElement { get; set; }
        public IGamepadFocusable GamepadDownElement { get; set; }
        public IGamepadFocusable GamepadLeftElement { get; set; }
        public IGamepadFocusable GamepadRightElement { get; set; }

        public void EnableExplicitFocusableControl(IGamepadFocusable upEle, IGamepadFocusable downEle, IGamepadFocusable leftEle, IGamepadFocusable rightEle)
        {
            ShouldUseExplicitFocusableControl = true;
            GamepadUpElement = upEle;
            GamepadDownElement = downEle;
            GamepadLeftElement = leftEle;
            GamepadRightElement = rightEle;
        }

        public void OnActionButtonPressed()
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select);
            _image.SetColor(Color.Yellow);
            OnPressed?.Invoke();
            //_image.SetDrawable(_downDrawable);
        }

        public void OnActionButtonReleased()
        {
            _image.SetColor(Color.Blue);
            //throw new NotImplementedException();
        }

        public void OnFocused()
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);
            _image.SetColor(Color.Blue);
            _image.SetScale(_defaultScale * _growFactor, _defaultScale * _growFactor);
            _image.InvalidateHierarchy();
        }

        public void OnUnfocused()
        {
            _image.SetScale(_defaultScale);
            _image.SetColor(Color.White);
            _image.InvalidateHierarchy();
        }

        public void OnUnhandledDirectionPressed(Nez.UI.Direction direction)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region IInputListener

        bool _mouseOver, _mouseDown;
        bool _isDisabled;

        void IInputListener.OnMouseEnter()
        {
            _mouseOver = true;
            GetStage().SetGamepadFocusElement(null);
            GetStage().SetGamepadFocusElement(this);
        }


        void IInputListener.OnMouseExit()
        {
            _mouseOver = _mouseDown = false;
        }


        bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
        {
            if (_isDisabled)
                return false;

            _mouseDown = true;

            OnActionButtonPressed();

            return true;
        }

        bool IInputListener.OnRightMousePressed(Vector2 mousePos)
        {
            if (_isDisabled)
                return false;

            _mouseDown = true;
            return true;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
            //if we get too far outside the label cancel future events
            if (DistanceOutsideBoundsToPoint(mousePos) > 50f)
            {
                _mouseDown = _mouseOver = false;
                GetStage().RemoveInputFocusListener(this);
            }
        }


        void IInputListener.OnLeftMouseUp(Vector2 mousePos)
        {
            _mouseDown = false;
        }

        void IInputListener.OnRightMouseUp(Vector2 mousePos)
        {
            _mouseDown = false;
        }


        bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
        {
            return false;
        }

        #endregion
    }
}
