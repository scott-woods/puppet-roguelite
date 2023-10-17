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
        public Vector2 Direction;
        float _speed;
        SubpixelVector2 _subpixelV2;

        Mover _mover;

        public VelocityComponent(Mover mover, float speed, Vector2 direction, SubpixelVector2 subpixelV2)
        {
            _speed = speed;
            Direction = direction;
            _subpixelV2 = subpixelV2;

            _mover = mover;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
        }

        public void Move()
        {
            //move
            var movement = Direction * _speed * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out var result);
            _subpixelV2.Update(ref movement);
            _mover.ApplyMovement(movement);
        }
    }
}
