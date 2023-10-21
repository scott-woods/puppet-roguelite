using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
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
        float _speed;
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
            _speed = speed;

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
        public void Move(float speed = 0)
        {
            speed = speed == 0 ? _speed : speed;
            //move
            var movement = Direction * speed * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out var result);
            _subPixelV2.Update(ref movement);
            _mover.ApplyMovement(movement);
        }
    }
}
