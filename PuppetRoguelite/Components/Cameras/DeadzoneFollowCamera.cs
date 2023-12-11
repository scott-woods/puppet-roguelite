using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.SceneComponents;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Cameras
{
    public class DeadzoneFollowCamera : Component, IUpdatable
    {
        Entity _targetEntity;
        public RectangleF Deadzone = new RectangleF(0, 0, 64, 64);
        CameraHandler _cameraHandler;

        //components
        CameraShake _shake;

        public DeadzoneFollowCamera(Entity target, Vector2 deadzoneSize)
        {
            _targetEntity = target;
            Deadzone = new RectangleF(0, 0, deadzoneSize.X, deadzoneSize.Y);
        }

        public override void Initialize()
        {
            base.Initialize();

            _shake = Entity.AddComponent(new CameraShake());
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Log.Information("Creating Deadzone Follow Camera");
            _cameraHandler = Entity.Scene.GetOrCreateSceneComponent<CameraHandler>();
            _cameraHandler.FormatCamera();
        }

        public void Update()
        {
            if (_targetEntity != null)
            {
                var pos = GetTargetPosition();

                var camBounds = Entity.Scene.Camera.Bounds;
                if (pos.X - camBounds.Width / 2 < _cameraHandler.TopLeft.X)
                {
                    pos.X = _cameraHandler.TopLeft.X + camBounds.Width / 2;
                }
                if (pos.X + camBounds.Width / 2 > _cameraHandler.BottomRight.X)
                {
                    pos.X = _cameraHandler.BottomRight.X - camBounds.Width / 2;
                }
                if (pos.Y - camBounds.Height / 2 < _cameraHandler.TopLeft.Y)
                {
                    pos.Y = _cameraHandler.TopLeft.Y + camBounds.Height / 2;
                }
                if (pos.Y + camBounds.Height / 2 > _cameraHandler.BottomRight.Y)
                {
                    pos.Y = _cameraHandler.BottomRight.Y - camBounds.Height / 2;
                }

                Entity.Position = pos;
            }
        }

        public void SetFollowTarget(Entity target)
        {
            Log.Information("Setting Camera Target", target);
            _targetEntity = target;
        }

        public void RemoveFollowTarget()
        {
            Log.Information("Removing Camera Target");
            if (_targetEntity != null)
            {
                _targetEntity = null;
            }
        }

        public Vector2 GetTargetPosition()
        {
            var targetPosition = Transform.Position;
            var cameraPosition = Transform.Position;
            var translatedDeadzone = Deadzone;
            translatedDeadzone.X += cameraPosition.X - Deadzone.Width / 2;
            translatedDeadzone.Y += cameraPosition.Y - Deadzone.Height / 2;

            if (_targetEntity.Position.X <= translatedDeadzone.Left)
                targetPosition = new Vector2(_targetEntity.Position.X + translatedDeadzone.Width / 2 - Deadzone.X, targetPosition.Y);
            if (_targetEntity.Position.X >= translatedDeadzone.Right)
                targetPosition = new Vector2(_targetEntity.Position.X - translatedDeadzone.Width / 2 - Deadzone.X, targetPosition.Y);
            if (_targetEntity.Position.Y <= translatedDeadzone.Top)
                targetPosition = new Vector2(targetPosition.X, _targetEntity.Position.Y + translatedDeadzone.Height / 2 - Deadzone.Y);
            if (_targetEntity.Position.Y >= translatedDeadzone.Bottom)
                targetPosition = new Vector2(targetPosition.X, _targetEntity.Position.Y - translatedDeadzone.Height / 2 - Deadzone.Y);

            return new Vector2(Mathf.Floor(targetPosition.X), Mathf.Floor(targetPosition.Y));
        }
    }
}
