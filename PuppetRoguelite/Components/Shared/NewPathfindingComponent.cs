using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class NewPathfindingComponent : Component
    {
        int _pathDesiredDistance;
        int _targetDesiredDistance;

        GridGraphManager _gridGraphManager;
        VelocityComponent _velocityComponent;

        string _mapId;

        public NewPathfindingComponent(VelocityComponent velocityComponent, string mapId, int pathDesiredDistance = 16, int targetDesiredDistance = 16)
        {
            _mapId = mapId;
            _velocityComponent = velocityComponent;
            _pathDesiredDistance = pathDesiredDistance;
            _targetDesiredDistance = targetDesiredDistance;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _gridGraphManager = Entity.Scene.FindComponentsOfType<GridGraphManager>().FirstOrDefault(g => g.MapId == _mapId);
        }

        /// <summary>
        /// follow a path to a target. returns true when the entity is within a set distance of the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="applySmoothing"></param>
        /// <returns></returns>
        public bool FollowPath(Vector2 target, bool applySmoothing = true)
        {
            if (Math.Abs(Vector2.Distance(Entity.Position, target)) <= _targetDesiredDistance)
            {
                return true;
            }

            //get basic path on grid
            var path = _gridGraphManager.FindPath(Entity.Position, target);

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
                        var raycastHit = Physics.Linecast(path[currentIndex], path[i]);
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
            foreach(var pos in finalPath)
            {
                //if within set distance to the next point on path, continue to the next one
                if (Math.Abs(Vector2.Distance(Entity.Position, pos)) <= _pathDesiredDistance)
                {
                    continue;
                }
                else //if not within distance to next point, we need to move there. update direction of velocity component
                {
                    var direction = pos - Entity.Position;
                    direction.Normalize();
                    _velocityComponent.SetDirection(direction);
                    return false;
                }
            }

            //no points along the path were good, just go to target
            var dir = target - Entity.Position;
            dir.Normalize();
            _velocityComponent.SetDirection(dir);
            return false;
        }
    }
}
