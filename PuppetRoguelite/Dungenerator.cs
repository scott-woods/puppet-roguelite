﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class Dungenerator : SceneComponent
    {
        Point _roomSize = new Point(24, 24);
        Point _tileSize = new Point(16, 16);
        Point _gridSize = new Point(5, 5);

        List<Map> _maps = new List<Map>()
        {
            new Map(Nez.Content.Tiled.Tilemaps.TBLR_1, true, true, true, true),
            new Map(Nez.Content.Tiled.Tilemaps.TBL_1, true, true, true, false),
            new Map(Nez.Content.Tiled.Tilemaps.TBR_1, true, true, false, true),
            new Map(Nez.Content.Tiled.Tilemaps.TLR_1, true, false, true, true),
            new Map(Nez.Content.Tiled.Tilemaps.BLR_1, false, true, true, true),
            new Map(Nez.Content.Tiled.Tilemaps.TB_1, true, true, false, false),
            new Map(Nez.Content.Tiled.Tilemaps.TL_1, true, false, true, false),
            new Map(Nez.Content.Tiled.Tilemaps.TR_1, true, false, false, true),
            new Map(Nez.Content.Tiled.Tilemaps.BL_1, false, true, true, false),
            new Map(Nez.Content.Tiled.Tilemaps.BR_1, false, true, false, true),
            new Map(Nez.Content.Tiled.Tilemaps.LR_1, false, false, true, true),
            new Map(Nez.Content.Tiled.Tilemaps.T_1, true, false, false, false),
            new Map(Nez.Content.Tiled.Tilemaps.B_1, false, true, false, false),
            new Map(Nez.Content.Tiled.Tilemaps.L_1, false, false, true, false),
            new Map(Nez.Content.Tiled.Tilemaps.R_1, false, false, false, true)
        };

        AstarGridGraph _graph;

        Point _hubPoint, _preBossPoint, _bossPoint, _leftKeyPoint, _rightKeyPoint;

        Dictionary<Point, Entity> _roomDictionary = new Dictionary<Point, Entity>();

        List<RoomNode> _nodes = new List<RoomNode>();

        int _maxAttempts = 10;

        public void Generate()
        {
            var attempts = 0;
            bool success = false;
            while (!success && attempts < _maxAttempts)
            {
                var graphSuccess = CreateGraph();
                var pathsSuccess = CreatePaths();
                var mapsSuccess = CreateMaps();

                if (!graphSuccess || !pathsSuccess || !mapsSuccess)
                {
                    Reset();
                    attempts += 1;
                }
                else
                {
                    success = true;
                }
            }

            var point = _hubPoint * _roomSize * _tileSize;
            PlayerController.Instance.Entity.SetPosition(point.ToVector2() + new Vector2(64, 64));
        }

        void Reset()
        {
            _graph = null;
            foreach(var room in _roomDictionary)
            {
                room.Value.Destroy();
            }
            _roomDictionary.Clear();
            _nodes.Clear();
        }

        /// <summary>
        /// create grid graph and determine positions for special rooms
        /// </summary>
        public bool CreateGraph()
        {
            _graph = new AstarGridGraph((int)_gridSize.X, (int)_gridSize.Y);

            //random position for starting location
            _hubPoint = new Point(Nez.Random.NextInt((int)_gridSize.X), Nez.Random.NextInt((int)_gridSize.Y));
            _nodes.Add(new RoomNode(_hubPoint, true, true, true, true, RoomType.Hub));

            //pre boss room a certain distance from starting room
            bool bossRoomPlaced = false;
            while (!bossRoomPlaced)
            {
                int x = Nez.Random.Range(0, _gridSize.X);
                int y = Nez.Random.Range(1, _gridSize.Y); //boss room is always entered from bottom, so need space above pre boss
                Point potentialPos = new Point(x, y);

                var path = _graph.Search(_hubPoint, potentialPos);

                if (path.Count >= 4)
                {
                    _preBossPoint = potentialPos;
                    _nodes.Add(new RoomNode(_preBossPoint, true, true, true, true, RoomType.PreBoss));
                    bossRoomPlaced = true;
                }
            }

            //boss above pre boss
            _bossPoint = new Point(_preBossPoint.X, _preBossPoint.Y - 1);
            _nodes.Add(new RoomNode(_bossPoint, false, true, false, false, RoomType.Boss));
            _graph.Walls.Add(_bossPoint);

            //left key
            bool leftKeyPlaced = false;
            while (!leftKeyPlaced)
            {
                int x = Nez.Random.NextInt((int)_gridSize.X - 1);
                int y = Nez.Random.NextInt((int)_gridSize.Y);
                Point potentialPoint = new Point(x, y);

                if (_nodes.Where(n => n.Point == potentialPoint).FirstOrDefault() != null)
                {
                    continue;
                }
                var top = potentialPoint + new Point(0, -1);
                var bottom = potentialPoint + new Point(0, 1);
                var left = potentialPoint + new Point(-1, 0);
                var surroundingSpaces = new List<Point>() { top, bottom, left };
                if (surroundingSpaces.Contains(_hubPoint) || surroundingSpaces.Contains(_preBossPoint))
                {
                    continue;
                }
                var right = potentialPoint + new Point(1, 0);
                if (_bossPoint == right || _rightKeyPoint == right)
                {
                    continue;
                }

                _leftKeyPoint = potentialPoint;
                _nodes.Add(new RoomNode(_leftKeyPoint, false, false, false, true, RoomType.Key));
                _graph.Walls.Add(_leftKeyPoint);
                leftKeyPlaced = true;
            }

            //right key
            bool rightKeyPlaced = false;
            while (!rightKeyPlaced)
            {
                int x = Nez.Random.Range(1, (int)_gridSize.X);
                int y = Nez.Random.NextInt((int)_gridSize.Y);
                Point potentialPoint = new Point(x, y);

                if (_nodes.Where(n => n.Point == potentialPoint).FirstOrDefault() != null)
                {
                    continue;
                }
                var top = potentialPoint + new Point(0, -1);
                var bottom = potentialPoint + new Point(0, 1);
                var right = potentialPoint + new Point(1, 0);
                var surroundingSpaces = new List<Point>() { top, bottom, right };
                if (surroundingSpaces.Contains(_hubPoint) || surroundingSpaces.Contains(_preBossPoint))
                {
                    continue;
                }
                var left = potentialPoint + new Point(-1, 0);
                if (_bossPoint == left || _leftKeyPoint == left)
                {
                    continue;
                }

                _rightKeyPoint = potentialPoint;
                _nodes.Add(new RoomNode(_rightKeyPoint, false, false, true, false, RoomType.Key));
                _graph.Walls.Add(_rightKeyPoint);
                rightKeyPlaced = true;
            }

            return true;
        }

        /// <summary>
        /// generate paths from special rooms to hub
        /// </summary>
        public bool CreatePaths()
        {
            var leftKeyEntrance = _leftKeyPoint + new Point(1, 0);
            var leftKeyToHub = _graph.Search(leftKeyEntrance, _hubPoint);
            if (leftKeyToHub == null)
            {
                return false;
            }
            for (int i = 0; i < leftKeyToHub.Count; i++)
            {
                var pathPoint = leftKeyToHub[i];

                var node = _nodes.Where(n => n.Point == pathPoint).FirstOrDefault();
                if (node != null)
                {
                    if (i == 0)
                    {
                        node.NeedsLeftDoor = true;
                    }
                    continue;
                }
                else
                {
                    node = new RoomNode(pathPoint, false, false, true, false, RoomType.Normal);
                    _nodes.Add(node);
                }
            }

            var rightKeyEntrance = _rightKeyPoint + new Point(-1, 0);
            var rightKeyToHub = _graph.Search(rightKeyEntrance, _hubPoint);
            if (rightKeyToHub == null)
            {
                return false;
            }
            for (int i = 0; i < rightKeyToHub.Count; i++)
            {
                var pathPoint = rightKeyToHub[i];

                var node = _nodes.Where(n => n.Point == pathPoint).FirstOrDefault();
                if (node != null)
                {
                    if (i == 0)
                    {
                        node.NeedsRightDoor = true;
                    }
                    continue;
                }
                else
                {
                    node = new RoomNode(pathPoint, false, false, false, true, RoomType.Normal);
                    _nodes.Add(node);
                }
            }

            var preBossEntrances = new List<Point>
            {
                new Point(_preBossPoint.X + 1, _preBossPoint.Y),
                new Point(_preBossPoint.X, _preBossPoint.Y + 1),
                new Point(_preBossPoint.X - 1, _preBossPoint.Y)
            };
            _graph.Walls.Add(_preBossPoint);
            var shortestPath = new List<Point>();
            foreach (var point in preBossEntrances)
            {
                var path = _graph.Search(point, _hubPoint);
                if (path != null)
                {
                    if (path.Count < shortestPath.Count || shortestPath.Count == 0)
                    {
                        shortestPath = path;
                    }
                }
            }
            if (shortestPath.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < shortestPath.Count; i++)
            {
                var pathPoint = shortestPath[i];

                var node = _nodes.Where(n => n.Point == pathPoint).FirstOrDefault();
                if (node != null)
                {
                    continue;
                }
                else
                {
                    node = new RoomNode(pathPoint, false, false, false, false, RoomType.Normal);
                    _nodes.Add(node);
                }
            }
            _graph.Walls.Remove(_preBossPoint);

            return true;
        }

        /// <summary>
        /// create maps for each node in the grid
        /// </summary>
        public bool CreateMaps()
        {
            foreach (var node in _nodes)
            {
                switch (node.RoomType)
                {
                    case RoomType.Hub:
                        CreateMap(node, Nez.Content.Tiled.Tilemaps.Hub_room);
                        break;
                    case RoomType.Key:
                        var mapString = node.NeedsLeftDoor ? Nez.Content.Tiled.Tilemaps.Right_key_room : Nez.Content.Tiled.Tilemaps.Left_key_room;
                        CreateMap(node, mapString);
                        break;
                    case RoomType.PreBoss:
                        CreateMap(node, Nez.Content.Tiled.Tilemaps.Pre_boss_room);
                        break;
                    case RoomType.Boss:
                        CreateMap(node, Nez.Content.Tiled.Tilemaps.Boss_room);
                        break;
                    case RoomType.Normal:
                        CreateMap(node);
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Load map and create map entity
        /// </summary>
        /// <param name="node"></param>
        /// <param name="mapString"></param>
        void CreateMap(RoomNode node, string mapString = null)
        {
            if (String.IsNullOrWhiteSpace(mapString))
            {
                CheckSurroundingNodes(ref node);
                var possibleMaps = _maps.Where(m =>
                {
                    if (node.NeedsTopDoor == m.HasTop
                    && node.NeedsBottomDoor == m.HasBottom
                    && node.NeedsLeftDoor == m.HasLeft
                    && node.NeedsRightDoor == m.HasRight)
                    {
                        return true;
                    }

                    return false;
                    //if (node.NeedsTopDoor && !m.HasTop) return false;
                    //if (node.NeedsBottomDoor && !m.HasBottom) return false;
                    //if (node.NeedsLeftDoor && !m.HasLeft) return false;
                    //if (node.NeedsRightDoor && !m.HasRight) return false;

                    //return true;
                }).ToList();

                mapString = possibleMaps.RandomItem().Name;
            }

            var mapEntity = new PausableEntity($"map-room-${node.Point.X}-${node.Point.Y}");
            var mapPosition = new Vector2(node.Point.X * _roomSize.X * _tileSize.X, node.Point.Y * _roomSize.Y * _tileSize.Y);
            mapEntity.SetPosition(mapPosition);
            Scene.AddEntity(mapEntity);
            var map = Scene.Content.LoadTiledMap(mapString);
            var renderer = mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            renderer.SetLayersToRender(new[] { "floor", "details" });
            renderer.RenderLayer = 10;
            _roomDictionary.Add(node.Point, mapEntity);
        }

        /// <summary>
        /// check a node's neighbors to determine which directions we need doors in
        /// </summary>
        /// <param name="node"></param>
        void CheckSurroundingNodes(ref RoomNode node)
        {
            var topPoint = new Point(node.Point.X, node.Point.Y - 1);
            var top = _nodes.Where(n => n.Point == topPoint).FirstOrDefault();
            node.NeedsTopDoor = CheckIfNeedsDoor(top, node);

            var bottomPoint = new Point(node.Point.X, node.Point.Y + 1);
            var bottom = _nodes.Where(n => n.Point == bottomPoint).FirstOrDefault();
            node.NeedsBottomDoor = CheckIfNeedsDoor(bottom, node);

            var leftPoint = new Point(node.Point.X - 1, node.Point.Y);
            var left = _nodes.Where(n => n.Point == leftPoint).FirstOrDefault();
            node.NeedsLeftDoor = CheckIfNeedsDoor(left, node);

            var rightPoint = new Point(node.Point.X + 1, node.Point.Y);
            var right = _nodes.Where(n => n.Point == rightPoint).FirstOrDefault();
            node.NeedsRightDoor = CheckIfNeedsDoor(right, node);
        }

        /// <summary>
        /// Check a node that is neighboring a baseNode, to see if a connection is needed
        /// </summary>
        /// <param name="nodeToCheck"></param>
        /// <param name="baseNode"></param>
        /// <returns></returns>
        bool CheckIfNeedsDoor(RoomNode nodeToCheck, RoomNode baseNode)
        {
            if (nodeToCheck != null)
            {
                switch (nodeToCheck.RoomType)
                {
                    case RoomType.Key:
                        var dir = nodeToCheck.Point - baseNode.Point;
                        if (dir.X > 0 && nodeToCheck.NeedsLeftDoor) return true;
                        else if (dir.X < 0 && nodeToCheck.NeedsRightDoor) return true;
                        else return false;
                    case RoomType.Boss:
                        return false;
                    case RoomType.PreBoss:
                    case RoomType.Normal:
                    case RoomType.Hub:
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}