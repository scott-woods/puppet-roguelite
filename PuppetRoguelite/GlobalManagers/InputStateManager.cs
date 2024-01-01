using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class InputStateManager : GlobalManager
    {
        public Emitter<InputStateEvents, bool> Emitter = new Emitter<InputStateEvents, bool>();

        bool _isUsingGamepad = false;
        public bool IsUsingGamepad
        {
            get => _isUsingGamepad;
            set
            {
                var changed = _isUsingGamepad != value;
                _isUsingGamepad = value;
                if (changed)
                    Emitter.Emit(InputStateEvents.InputStateChanged, value);
            }
        }

        float _pollInterval = 1f;
        ITimer _timer;

        public override void OnEnabled()
        {
            base.OnEnabled();

            _timer = Game1.Schedule(_pollInterval, true, DetermineInputType);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            _timer?.Stop();
            _timer = null;
        }

        void DetermineInputType(ITimer timer)
        {
            //if gamepad not connected, no need to check
            if (!Input.GamePads[0].IsConnected())
            {
                IsUsingGamepad = false;
                return;
            }

            //if any keys are being pressed, set using gamepad to false and return
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (Input.IsKeyDown(key))
                {
                    IsUsingGamepad = false;
                    return;
                }
            }

            //if mouse is being used, set using gamepad to false and return
            if (Input.MousePositionDelta.X != 0 || Input.MousePositionDelta.Y != 0 || Input.LeftMouseButtonDown || Input.RightMouseButtonDown)
            {
                IsUsingGamepad = false;
                return;
            }

            //if gamepad buttons are pressed, set using gamepad to true and return
            foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
            {
                if (Input.GamePads[0].IsButtonDown(button))
                {
                    IsUsingGamepad = true;
                    return;
                }
            }

            //if joysticks are being used, set using gamepad to true and return
            if (Input.GamePads[0].GetLeftStick() != Vector2.Zero || Input.GamePads[0].GetRightStick() != Vector2.Zero)
            {
                IsUsingGamepad = true;
                return;
            }
        }
    }

    public enum InputStateEvents
    {
        InputStateChanged
    }
}
