using Microsoft.Xna.Framework;
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

        public RoomNode(Point point, bool needsTopDoor, bool needsBottomDoor, bool needsLeftDoor, bool needsRightDoor, RoomType roomType)
        {
            Point = point;
            NeedsTopDoor = needsTopDoor;
            NeedsBottomDoor = needsBottomDoor;
            NeedsLeftDoor = needsLeftDoor;
            NeedsRightDoor = needsRightDoor;
            RoomType = roomType;
        }
    }
}
