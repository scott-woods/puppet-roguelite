﻿using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class PathfindingComponent : Component
    {
        int _pathDesiredDistance;
        int _targetDesiredDistance;

        GridGraphManager _gridGraphManager;
        VelocityComponent _velocityComponent;
        OriginComponent _originComponent;

        public Entity MapEntity;

        bool _active = true;

        List<Entity> _debugPoints = new List<Entity>();

        public PathfindingComponent(VelocityComponent velocityComponent, OriginComponent originComponent, Entity mapEntity, int pathDesiredDistance = 16, int targetDesiredDistance = 16)
        {
            MapEntity = mapEntity;
            _velocityComponent = velocityComponent;
            _originComponent = originComponent;
            _pathDesiredDistance = pathDesiredDistance;
            _targetDesiredDistance = targetDesiredDistance;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _gridGraphManager = Entity.Scene.FindComponentsOfType<GridGraphManager>().FirstOrDefault(g => g.Entity == MapEntity);
        }

        public override void OnRemovedFromEntity()
        {
            foreach (var point in _debugPoints)
            {
                point.Destroy();
            }
            _debugPoints.Clear();
        }

        /// <summary>
        /// follow a path to a target. returns true when the entity is within a set distance of the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="applySmoothing"></param>
        /// <returns></returns>
        public bool FollowPath(Vector2 target, bool applySmoothing = true, float interval = .5f)
        {
            //get origin from origin component
            var origin = _originComponent.Origin;

            //if within range of target, return true
            if (Math.Abs(Vector2.Distance(origin, target)) <= _targetDesiredDistance)
            {
                return true;
            }

            //check if active and handle timer if necessary
            if (!_active) return false;
            else if (interval > 0)
            {
                _active = false;
                Game1.Schedule(interval, timer =>
                {
                    _active = true;
                });
            }

            //get basic path on grid
            var path = _gridGraphManager.FindPath(origin, target);

            //foreach (var point in _debugPoints)
            //{
            //    point.Destroy();
            //}
            //_debugPoints.Clear();
            //foreach (var point in path)
            //{
            //    var ent = Entity.Scene.CreateEntity("path-point");
            //    ent.SetPosition(point);
            //    ent.AddComponent(new PrototypeSpriteRenderer(2, 2));
            //    _debugPoints.Add(ent);
            //}

            var finalPath = new List<Vector2>();

            //apply smoothing if desired
            if (applySmoothing)
            {
                int currentIndex = 0;
                while (currentIndex < path.Count - 1)
                {
                    int furthestVisibleIndex = currentIndex;
                    for (int i = currentIndex + 1; i < path.Count; i++)
                    {
                        var raycastHit = Physics.Linecast(path[currentIndex], path[i], 1 << (int)PhysicsLayers.Environment);
                        if (raycastHit.Collider == null)
                        {
                            furthestVisibleIndex = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //safeguard against getting stuck in while loop
                    if (furthestVisibleIndex == currentIndex)
                    {
                        currentIndex++;
                    }
                    else
                    {
                        currentIndex = furthestVisibleIndex;
                    }

                    finalPath.Add(path[currentIndex]);
                }
            }
            else
            {
                finalPath = path;
            }

            //loop through points along path
            foreach (var pos in finalPath)
            {
                //if within set distance to the next point on path, continue to the next one
                if (Math.Abs(Vector2.Distance(origin, pos)) <= _pathDesiredDistance)
                {
                    continue;
                }
                else //if not within distance to next point, we need to move there. update direction of velocity component
                {
                    var direction = pos - origin;
                    direction.Normalize();
                    _velocityComponent.SetDirection(direction);
                    return false;
                }
            }

            //no points along the path were good, just go to target
            var dir = target - origin;
            dir.Normalize();
            _velocityComponent.SetDirection(dir);
            return false;
        }
    }
}
