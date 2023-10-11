using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class RoomNode
    {
        public Point Point { get; set; }
        public bool NeedsTopDoor { get; set; }
        public bool NeedsBottomDoor { get; set; }
        public bool NeedsLeftDoor { get; set; }
        public bool NeedsRightDoor { get; set; }
        public RoomType RoomType { get; set; }
        public List<RoomNode> RoomNodes { get; set; }

        public RoomNode(Point point, bool needsTopDoor, bool needsBottomDoor, bool needsLeftDoor, bool needsRightDoor, RoomType roomType, List<RoomNode> roomNodes)
        {
            Point = point;
            NeedsTopDoor = needsTopDoor;
            NeedsBottomDoor = needsBottomDoor;
            NeedsLeftDoor = needsLeftDoor;
            NeedsRightDoor = needsRightDoor;
            RoomType = roomType;
            RoomNodes = roomNodes;
        }

        public List<Direction> GetUnconnectedDoors()
        {
            var unconnectedDoors = new List<Direction>();
            if (NeedsTopDoor && !IsConnected(Direction.Up)) unconnectedDoors.Add(Direction.Up);
            if (NeedsBottomDoor && !IsConnected(Direction.Down)) unconnectedDoors.Add(Direction.Down);
            if (NeedsLeftDoor && !IsConnected(Direction.Left)) unconnectedDoors.Add(Direction.Left);
            if (NeedsRightDoor && !IsConnected(Direction.Right)) unconnectedDoors.Add(Direction.Right);
            return unconnectedDoors;
        }

        public bool IsConnected(Direction direction)
        {
            var point = new Point();
            switch (direction)
            {
                case Direction.Up:
                    point = Point + new Point(0, -1);
                    break;
                case Direction.Down:
                    point = Point + new Point(0, 1);
                    break;
                case Direction.Left:
                    point = Point + new Point(-1, 0);
                    break;
                case Direction.Right:
                    point = Point + new Point(1, 0);
                    break;
                default:
                    return false;
            }

            return RoomNodes.Where(n => n.Point == point).Any();
        }
    }
}
