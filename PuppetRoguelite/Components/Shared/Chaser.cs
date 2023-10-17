//using Microsoft.Xna.Framework;
//using Nez;
//using PuppetRoguelite.Components.Characters.Player;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PuppetRoguelite.Components.Shared
//{
//    /// <summary>
//    /// component that allows an entity to chase a target entity
//    /// </summary>
//    public class Chaser : Component
//    {
//        Entity _targetEntity;
//        Mover _mover;
//        PathfindingComponent _pathfinder;

//        public Chaser(Entity targetEntity, Mover mover, PathfindingComponent pathfinder)
//        {
//            _targetEntity = targetEntity;
//            _mover = mover;
//            _pathfinder = pathfinder;
//        }

//        public void Chase(Entity targetEntity)
//        {
//            _pathfinder.SetTarget(targetEntity.Position);
//            var targetPos = _pathfinder.GetNextPosition();

//            var direction = targetPos - Entity.Position;
//            direction.Normalize();

//            //move
//            var movement = direction * _chainBot.MoveSpeed * Time.DeltaTime;
//            _chainBot.Mover.CalculateMovement(ref movement, out var result);
//            _chainBot.SubPixelV2.Update(ref movement);
//            _chainBot.Mover.ApplyMovement(movement);

//            if (targetEntity != null)
//            {
//                //check if finished
//                if (_chainBot.Pathfinder.IsNavigationFinished())
//                {
//                    return TaskStatus.Success;
//                }

//                //if close enough to player, return success
//                var distanceToPlayer = Vector2.Distance(_chainBot.Entity.Position, targetEntity.Entity.Position);
//                if (distanceToPlayer < 20)
//                {
//                    return TaskStatus.Success;
//                }

//                //get next path position
//                _chainBot.Pathfinder.SetTarget(targetEntity.Entity.Position);
//                var target = _chainBot.Pathfinder.GetNextPosition();

//                //determine direction
//                _chainBot.Direction = target - _chainBot.Entity.Position;
//                _chainBot.Direction.Normalize();

//                //handle animation
//                var animation = _chainBot.Direction.X >= 0 ? "RunRight" : "RunLeft";
//                if (!_chainBot.Animator.IsAnimationActive(animation))
//                {
//                    _chainBot.Animator.Play(animation);
//                }

//                //move
//                var movement = _chainBot.Direction * _chainBot.MoveSpeed * Time.DeltaTime;
//                _chainBot.Mover.CalculateMovement(ref movement, out var result);
//                _chainBot.SubPixelV2.Update(ref movement);
//                _chainBot.Mover.ApplyMovement(movement);

//                return TaskStatus.Running;
//            }

//            return TaskStatus.Failure;
//        }
//    }
//}
