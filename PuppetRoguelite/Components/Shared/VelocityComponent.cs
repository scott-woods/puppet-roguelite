using Microsoft.Xna.Framework;
using Nez;
using Nez.Tweens;
using Nez.UI;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class VelocityComponent : Component
    {
        public Vector2 Direction = new Vector2(1, 0);
        public float Speed;
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();

        Mover _mover;

        /// <summary>
        /// requires a Mover, default speed, and optionally a subpixelV2
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="speed"></param>
        /// <param name="subPixelV2"></param>
        public VelocityComponent(Mover mover, float speed)
        {
            Speed = speed;

            _mover = mover;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// Move, optionally override the default speed
        /// </summary>
        /// <param name="speed"></param>
        public void Move(float? speed = null)
        {
            speed = speed == null ? Speed : speed.Value;

            //move
            var movement = Direction * (float)speed * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out var result);
            _subPixelV2.Update(ref movement);
            _mover.ApplyMovement(movement);
        }

        //public void SetTweenedValue(float value)
        //{
        //    Speed = value;
        //}

        //public float GetTweenedValue()
        //{
        //    return Speed;
        //}

        //public object GetTargetObject()
        //{
        //    return this;
        //}
    }
}
