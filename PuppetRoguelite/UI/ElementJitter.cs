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
    public class ElementJitter
    {
        Element _element;
        float _maxOffset;
        float _jitterRate;

        Vector2 _totalOffset = Vector2.Zero;

        ITimer _timer;

        bool _enabled = true;

        public ElementJitter(Element element, float maxOffset = 1f, float jitterRate = .05f)
        {
            _element = element;
            _maxOffset = maxOffset;
            _jitterRate = jitterRate;

            StartTimer();
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;

            if (!enabled)
            {
                _timer?.Stop();
                _timer = null;
            }
            else
            {
                if (_timer == null)
                {
                    StartTimer();
                }
            }
        }

        void StartTimer()
        {
            _timer = Game1.Schedule(_jitterRate, true, timer =>
            {
                float jitterX = Nez.Random.Range(-_maxOffset, _maxOffset) - _totalOffset.X;
                float jitterY = Nez.Random.Range(-_maxOffset, _maxOffset) - _totalOffset.Y;

                jitterX = Math.Clamp(jitterX, -_maxOffset - _totalOffset.X, _maxOffset - _totalOffset.X);
                jitterY = Math.Clamp(jitterY, -_maxOffset - _totalOffset.Y, _maxOffset - _totalOffset.Y);

                _element.SetPosition(_element.GetX() + jitterX, _element.GetY() + jitterY);
                _totalOffset.X += jitterX;
                _totalOffset.Y += jitterY;
            });
        }
    }
}
