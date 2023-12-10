using Microsoft.Xna.Framework;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class FocusLabel : Label, IGamepadFocusable, IInputListener
    {
        string _givenText;

        bool _mouseOver, _mouseDown;
        bool _isDisabled;

        public event Action OnLabelFocused;

        public bool ShouldUseExplicitFocusableControl { get; set; }
        public IGamepadFocusable GamepadUpElement { get; set; }
        public IGamepadFocusable GamepadDownElement { get; set; }
        public IGamepadFocusable GamepadLeftElement { get; set; }
        public IGamepadFocusable GamepadRightElement { get; set; }

        public FocusLabel(string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
            _givenText = text;
            SetTouchable(Touchable.Enabled);
        }

        #region IGamepadFocusable

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
            //throw new NotImplementedException();
        }

        public void OnActionButtonReleased()
        {
            //throw new NotImplementedException();
        }

        public void OnFocused()
        {
            SetText($"* {_givenText}");

            if (OnLabelFocused != null)
                OnLabelFocused();
            //unfocus other focus labels
            //var focusLabels = GetStage().FindAllElementsOfType<FocusLabel>();
            //focusLabels.Remove(this);
            //foreach(var focusLabel in focusLabels)
            //{
            //    focusLabel.OnUnfocused();
            //}
        }

        public void OnUnfocused()
        {
            SetText($"  {_givenText}");
        }

        public void OnUnhandledDirectionPressed(Direction direction)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region IInputListener

        void IInputListener.OnMouseEnter()
        {
            _mouseOver = true;
            GetStage().SetGamepadFocusElement(null);
            GetStage().SetGamepadFocusElement(this);
        }


        void IInputListener.OnMouseExit()
        {
            _mouseOver = _mouseDown = false;        }


        bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
        {
            if (_isDisabled)
                return false;

            _mouseDown = true;
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
