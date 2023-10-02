using Microsoft.Xna.Framework;
using Nez;
using Nez.Timers;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class PathfindingComponent : Component
    {
        GridGraphManager _gridGraphManager;

        List<Vector2> _currentPath;
        int _currentPathIndex = 0;

        float _minimumDistance = 3f;
        float _updatePathInterval = .2f;

        ITimer _timer;

        Vector2? _target;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _gridGraphManager = Entity.Scene.GetSceneComponent<GridGraphManager>();

            _timer = Core.Schedule(_updatePathInterval, true, timer => CalculatePath());
        }

        /// <summary>
        /// calculate a path using the grid graph manager of the scene
        /// </summary>
        public void CalculatePath()
        {
            if (_target != null)
            {
                if (_gridGraphManager != null)
                {
                    _currentPath = _gridGraphManager.FindPath(Entity.Position, _target.Value);
                    _currentPathIndex = 0;
                }
            }
        }

        /// <summary>
        /// Get the next point on the path, incrementing if within min distance to it
        /// </summary>
        /// <returns></returns>
        public Vector2 GetNextPosition()
        {
            var currentTarget = _currentPath[_currentPathIndex];
            if (Math.Abs(Vector2.Distance(Entity.Position, currentTarget)) < _minimumDistance)
            {
                if (_currentPathIndex < _currentPath.Count - 1)
                {
                    _currentPathIndex++;
                }
            }

            return _currentPath[_currentPathIndex];
        }

        /// <summary>
        /// returns true if our position is within the minimum distance of the final target
        /// </summary>
        /// <returns></returns>
        public bool IsNavigationFinished()
        {
            if (_currentPath == null) return false;
            if (_currentPathIndex == _currentPath.Count - 1)
            {
                var currentTarget = _currentPath[_currentPathIndex];
                if (Math.Abs(Vector2.Distance(Entity.Position, currentTarget)) < _minimumDistance)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// set target position
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Vector2 target)
        {
            _target = target;
            CalculatePath();
        }

        /// <summary>
        /// Change frequency that path is calculated
        /// </summary>
        /// <param name="interval"></param>
        public void SetUpdateInterval(float interval)
        {
            _updatePathInterval = interval;
            _timer.Stop();
            _timer = Core.Schedule(_updatePathInterval, true, timer => CalculatePath());
        }

        /// <summary>
        /// Render path in debug mode
        /// </summary>
        /// <param name="batcher"></param>
        public override void DebugRender(Batcher batcher)
        {
            base.DebugRender(batcher);

            if (_currentPath != null)
            {
                foreach (var node in _currentPath)
                {
                    batcher.DrawPixel(node.X, node.Y, Color.Orange, 4);
                }
            }
        }
    }
}
