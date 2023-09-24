using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class DeadzoneFollowCamera : Component, IUpdatable
    {
        Entity _targetEntity;
        public RectangleF Deadzone = new RectangleF(0, 0, 64, 64);

        public DeadzoneFollowCamera(Entity target, Vector2 deadzoneSize)
        {
            _targetEntity = target;
            Deadzone = new RectangleF(0, 0, deadzoneSize.X, deadzoneSize.Y);
        }

        public override void OnAddedToEntity()
        {
        }

        public void Update()
        {
            Transform.Position = GetTargetPosition();
        }

        public Vector2 GetTargetPosition()
        {
            var targetPosition = Transform.Position;
            var cameraPosition = Transform.Position;
            var translatedDeadzone = Deadzone;
            translatedDeadzone.X += cameraPosition.X - (Deadzone.Width / 2);
            translatedDeadzone.Y += cameraPosition.Y - (Deadzone.Height / 2);

            if (_targetEntity.Position.X <= translatedDeadzone.Left)
                targetPosition = new Vector2(_targetEntity.Position.X + (translatedDeadzone.Width / 2) - Deadzone.X, targetPosition.Y);
            if (_targetEntity.Position.X >= translatedDeadzone.Right)
                targetPosition = new Vector2(_targetEntity.Position.X - (translatedDeadzone.Width / 2) - Deadzone.X, targetPosition.Y);
            if (_targetEntity.Position.Y <= translatedDeadzone.Top)
                targetPosition = new Vector2(targetPosition.X, _targetEntity.Position.Y + (translatedDeadzone.Height / 2) - Deadzone.Y);
            if (_targetEntity.Position.Y >= translatedDeadzone.Bottom)
                targetPosition = new Vector2(targetPosition.X, _targetEntity.Position.Y - (translatedDeadzone.Height / 2) - Deadzone.Y);

            return new Vector2(Mathf.Floor(targetPosition.X), Mathf.Floor(targetPosition.Y));
        }
    }
}
