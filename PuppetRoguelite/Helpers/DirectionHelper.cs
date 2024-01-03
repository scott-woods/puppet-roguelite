using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Helpers
{
    public static class DirectionHelper
    {
        public static Direction GetDirectionByDegrees(float angleInDegrees)
        {
            //make sure angle is 360 max
            angleInDegrees = (angleInDegrees + 360) % 360;

            var newDirection = Direction.Right;
            if (angleInDegrees >= 45 && angleInDegrees < 135) newDirection = Direction.Down;
            else if (angleInDegrees >= 135 && angleInDegrees < 225) newDirection = Direction.Left;
            else if (angleInDegrees >= 225 && angleInDegrees < 315) newDirection = Direction.Up;

            return newDirection;
        }

        public static Direction GetDirectionByVector2(Vector2 vector)
        {
            float currentAngle = (float)Math.Atan2(vector.Y, vector.X);
            return GetDirectionByDegrees(MathHelper.ToDegrees(currentAngle));
        }

        public static Direction GetDirectionToMouse(Vector2 position)
        {
            var angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(position, Game1.Scene.Camera.MouseToWorldPoint()));
            return GetDirectionByDegrees(angle);
        }
    }
}
