using Nez.AI.BehaviorTrees;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite
{
    public static class EnemyTasks
    {
        public static TaskStatus ChasePlayer(NewPathfindingComponent pathfinder, VelocityComponent velocityComponent)
        {
            var reachedTarget = pathfinder.FollowPath(PlayerController.Instance.Entity.Position, true);
            if (!reachedTarget)
            {
                velocityComponent.Move();
                return TaskStatus.Running;
            }
            else
            {
                return TaskStatus.Success;
            }
        }
    }
}
