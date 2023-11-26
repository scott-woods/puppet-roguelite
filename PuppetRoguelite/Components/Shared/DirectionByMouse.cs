using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    /// <summary>
    /// Manages a direction based on the entity's angle to the mouse
    /// </summary>
    public class DirectionByMouse : Component, IUpdatable
    {
        public Direction PreviousDirection = Direction.Right;
        public Direction CurrentDirection = Direction.Right;

        public void Update()
        {
            PreviousDirection = CurrentDirection;

            var angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(Entity.Position, Entity.Scene.Camera.MouseToWorldPoint()));
            angle = (angle + 360) % 360;
            var newDirection = Direction.Right;
            if (angle >= 45 && angle < 135) newDirection = Direction.Down;
            else if (angle >= 135 && angle < 225) newDirection = Direction.Left;
            else if (angle >= 225 && angle < 315) newDirection = Direction.Up;

            CurrentDirection = newDirection;
        }
    }
}
