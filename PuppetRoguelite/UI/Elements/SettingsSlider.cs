using Microsoft.Xna.Framework;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class SettingsSlider : Slider, IInputListener
    {
        public event Action<Slider> OnMouseOver;

        bool _focused = false;

        public SettingsSlider(Skin skin, string styleName = null) : base(skin, styleName)
        {
        }

        public SettingsSlider(float min, float max, float stepSize, bool vertical, SliderStyle style) : base(min, max, stepSize, vertical, style)
        {
        }

        public SettingsSlider(Skin skin, string styleName = null, float min = 0, float max = 1, float step = 0.1F) : base(skin, styleName, min, max, step)
        {
        }

        public SettingsSlider(float min, float max, float stepSize, bool vertical, Skin skin, string styleName = null) : base(min, max, stepSize, vertical, skin, styleName)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            _focused = true;

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);

            var knob = GetKnobDrawable() as PrimitiveDrawable;
            knob.Color = Color.Black;
        }

        protected override void OnUnfocused()
        {
            base.OnUnfocused();

            _focused = false;

            var knob = GetKnobDrawable() as PrimitiveDrawable;
            knob.Color = Color.White;
        }

        #region IInputListener

        bool _mouseOver, _mouseDown;

        void IInputListener.OnMouseEnter()
        {
            if (!_focused)
            {
                OnMouseOver?.Invoke(this);
            }
            
            _mouseOver = true;
        }


        void IInputListener.OnMouseExit()
        {
            _mouseOver = _mouseDown = false;
        }


        bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
        {
            CalculatePositionAndValue(mousePos);
            _mouseDown = true;
            return true;
        }


        bool IInputListener.OnRightMousePressed(Vector2 mousePos)
        {
            return false;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
            if (DistanceOutsideBoundsToPoint(mousePos) > SliderBoundaryThreshold)
            {
                _mouseDown = _mouseOver = false;
                GetStage().RemoveInputFocusListener(this);
            }
            else
            {
                CalculatePositionAndValue(mousePos);
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

        void CalculatePositionAndValue(Vector2 mousePos)
        {
            var knob = GetKnobDrawable();
            var style = GetStyle();

            float value;
            if (_vertical)
            {
                var height = this.height - style.Background.TopHeight - style.Background.BottomHeight;
                var knobHeight = knob == null ? 0 : knob.MinHeight;
                position = mousePos.Y - style.Background.BottomHeight - knobHeight * 0.5f;
                value = Min + (Max - Min) * (position / (height - knobHeight));
                position = Math.Max(0, position);
                position = Math.Min(height - knobHeight, position);
            }
            else
            {
                var width = this.width - style.Background.LeftWidth - style.Background.RightWidth;
                var knobWidth = knob == null ? 0 : knob.MinWidth;
                position = mousePos.X - style.Background.LeftWidth - knobWidth * 0.5f;
                value = Min + (Max - Min) * (position / (width - knobWidth));
                position = Math.Max(0, position);
                position = Math.Min(width - knobWidth, position);
            }

            SetValue(value);
        }
    }
}
