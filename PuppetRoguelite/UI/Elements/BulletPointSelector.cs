using Microsoft.Xna.Framework;
using Nez;
using Nez.Analysis;
using Nez.UI;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class BulletPointSelector<T> : Table, IGamepadFocusable, IInputListener
    {
        Label _asteriskLabel;
        Label _choiceLabel;

        public event Action<T> OnSelected;
        public event Action OnPointFocused;

        bool _isDisabled;

        T _returnValue;

        bool _shouldShake = true;
        ElementJitter _jitter;

        public BulletPointSelector(string choiceText, T value, bool wrapChoiceText, Skin skin, string styleName = null)
        {
            _returnValue = value;

            _asteriskLabel = new Label("*", skin, styleName);
            _asteriskLabel.SetAlignment(Nez.UI.Align.Center);
            Add(_asteriskLabel).SetSpaceRight(10f);
            _choiceLabel = new Label(choiceText, skin, styleName);
            _choiceLabel.SetAlignment(Nez.UI.Align.Center);
            _choiceLabel.SetWrap(wrapChoiceText);
            Add(_choiceLabel).Grow();

            _asteriskLabel.SetVisible(false);

            SetTouchable(Touchable.Enabled);

            _jitter = new ElementJitter(_choiceLabel, .25f, .08f);
            _jitter.SetEnabled(false);
        }

        public void SetDisabled(bool disabled)
        {
            _isDisabled = disabled;

            var color = disabled ? Color.Gray : Color.White;
            var style = _asteriskLabel.GetStyle();
            var newStyle = style.Clone();
            newStyle.FontColor = color;
            _asteriskLabel.SetStyle(newStyle);
            _choiceLabel.SetStyle(newStyle);
        }

        public T GetValue()
        {
            return _returnValue;
        }

        public bool GetDisabled() => _isDisabled;

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
            if (_isDisabled)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._021_Decline_01);
            }
            else
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select);
                OnSelected?.Invoke(_returnValue);
                //GetStage().SetGamepadFocusElement(null);
            }
        }

        public void OnActionButtonReleased()
        {
            //throw new NotImplementedException();
        }

        public void OnFocused()
        {
            if (!Controls.Instance.Cancel.IsPressed)
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);

            if (_shouldShake)
            {
                _jitter.SetEnabled(true);
            }

            _asteriskLabel.SetVisible(true);

            OnPointFocused?.Invoke();
        }

        public void OnUnfocused()
        {
            _asteriskLabel.SetVisible(false);

            if (_shouldShake)
            {
                _jitter.SetEnabled(false);
            }
            //SetText($"  {_givenText}");
        }

        public void OnUnhandledDirectionPressed(Nez.UI.Direction direction)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region IInputListener

        bool _mouseOver;
        bool _mouseDown;

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
