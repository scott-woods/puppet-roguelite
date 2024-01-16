using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Models
{
    /// <summary>
    /// model that represents a partition of the overall dungeon
    /// </summary>
    public class DungeonLeaf
    {
        const int _minSize = 37;

        public DungeonLeaf LeftChild;
        public DungeonLeaf RightChild;
        public DungeonRoom Room;

        public Vector2 Position;
        public Vector2 Size;

        public DungeonLeaf(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public DungeonLeaf(int x, int y, int width, int height)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
        }

        /// <summary>
        /// returns true if the split was successful
        /// </summary>
        /// <returns></returns>
        public bool Split()
        {
            //if we've already split, don't do anything
            if (LeftChild != null || RightChild != null)
                return false;

            //if width is 25% larger, split vertically. if height is 25% larger, split horizontally
            //if neither applies, split randomly
            var splitH = Nez.Random.Chance(.5f);
            if (Size.X > Size.Y && Size.X / Size.Y >= 1.25f)
                splitH = false;
            else if (Size.Y > Size.X && Size.Y / Size.X >= 1.25f)
                splitH = true;

            //determine if area is too small to split
            var max = splitH ? Size.Y : Size.X;
            max -= _minSize;
            if (max <= _minSize)
                return false;

            var split = Nez.Random.Range(_minSize, (int)max);

            //create children from the split
            if (splitH)
            {
                LeftChild = new DungeonLeaf(Position, new Vector2(Size.X, split));
                RightChild = new DungeonLeaf(new Vector2(Position.X, Position.Y + split), new Vector2(Size.X, Size.Y - split));
            }
            else
            {
                LeftChild = new DungeonLeaf(Position, new Vector2(split, Size.Y));
                RightChild = new DungeonLeaf(new Vector2(Position.X + split, Position.Y), new Vector2(Size.X - split, Size.Y));
            }

            return true;
        }

        public void CreateRooms()
        {
            if (LeftChild != null || RightChild != null)
            {
                LeftChild?.CreateRooms();
                RightChild?.CreateRooms();
            }
            else
            {
                //get all possible maps that will fit in this partition
                var possibleMaps = Maps.ForgeMaps.Where((m) =>
                {
                    //determine if map will fit in leaf
                    var fits = m.TmxMap.Width <= Size.X + 10 && m.TmxMap.Height <= Size.Y + 12;

                    //if doesn't fit, unload
                    if (!fits)
                        Game1.Scene.Content.UnloadAsset<TmxMap>(m.Name);

                    return fits;
                }).ToList();

                //if no possible maps, don't do anything i guess
                if (possibleMaps.Count == 0)
                    return;

                var map = possibleMaps.RandomItem();
                var pos = new Vector2(Nez.Random.Range(1, Size.X - map.TmxMap.Width - 1) + Position.X, Nez.Random.Range(1, Size.Y - map.TmxMap.Height - 1) + Position.Y);

                Room = new DungeonRoom(map, pos);
            }
        }

        public DungeonRoom GetRoom()
        {
            if (Room != null)
                return Room;
            else
            {
                DungeonRoom leftRoom = null;
                DungeonRoom rightRoom = null;
                if (LeftChild != null)
                    leftRoom = LeftChild.GetRoom();
                if (RightChild != null)
                    rightRoom = RightChild.GetRoom();

                if (leftRoom == null && rightRoom == null)
                    return null;
                else if (rightRoom == null)
                    return leftRoom;
                else if (leftRoom == null)
                    return rightRoom;
                else if (Nez.Random.Chance(.5f))
                    return leftRoom;
                else
                    return rightRoom;
            }
        }
    }
}
