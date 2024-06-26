﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    public class Dungenerator : SceneComponent
    {
        Point _roomSize = new Point(24, 24);
        Point _tileSize = new Point(16, 16);
        Point _gridSize = new Point(4, 4);

        List<Map> _maps = new List<Map>()
        {
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_b_1, false, true, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_b_2, false, true, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_bl_1, false, true, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_blr_1, false, true, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_br_1, false, true, false, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_l_1, false, false, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_lr_1, false, false, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_r_1, false, false, false, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_t_1, true, false, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tb_1, true, true, false, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tbl_1, true, true, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tblr_1, true, true, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tbr_1, true, true, false, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tl_1, true, false, true, false),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tlr_1, true, false, true, true),
            new Map(Content.Tiled.Tilemaps.DungeonPrison.Dp_tr_1, true, false, false, true),
        };

        Map _leftKeyMap = new Map(Content.Tiled.Tilemaps.DungeonPrison.Left_key_room, false, false, false, true);
        Map _rightKeyMap = new Map(Content.Tiled.Tilemaps.DungeonPrison.Right_key_room, false, false, true, false);
        Map _preBossMap = new Map(Content.Tiled.Tilemaps.DungeonPrison.Pre_boss_room, false, true, true, true);

        AstarGridGraph _graph;

        Point _hubPoint, _preBossPoint, _bossPoint, _leftKeyPoint, _rightKeyPoint;

        public List<RoomNode> Nodes = new List<RoomNode>();

        int _maxAttempts = 1000;

        public Vector2 GetPlayerSpawnPoint()
        {
            var point = _preBossPoint * _roomSize * _tileSize;
            var midPoint = point + new Point(_roomSize.X * _tileSize.X / 2, _roomSize.Y * _tileSize.Y / 2);
            return midPoint.ToVector2();
        }

        /// <summary>
        /// given a world space position, get the room node that position is in
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public RoomNode GetNodeByWorldPosition(Vector2 position)
        {
            var point = new Point((int)Math.Floor(position.X / (_tileSize.X * _roomSize.X)), (int)Math.Floor(position.Y / (_tileSize.Y * _roomSize.Y)));
            var node = Nodes.FirstOrDefault(n => n.Point == point);
            if (node != null) return node;
            else return null;
        }

        public override void Update()
        {
            base.Update();
        }

        public void Generate()
        {
            var attempts = 0;
            bool success = false;
            while (!success)
            {
                if (!CreateGraph())
                {
                    attempts++;
                    Reset();
                    continue;
                }
                if (!CreatePaths())
                {
                    attempts++;
                    Reset();
                    continue;
                }
                if (!CreateMaps()) //pick maps based on requirements
                {
                    attempts++;
                    Reset();
                    continue;
                }

                ProcessUnconnectedDoors();

                //final check
                if (!FinalValidation())
                {
                    attempts++;
                    Reset();
                    continue;
                }

                success = true;
            }

            //create actual maps
            foreach (var node in Nodes)
            {
                //create map entity
                var mapEntity = new PausableEntity($"map-room-${node.Point.X}-${node.Point.Y}");
                var mapPosition = new Vector2(node.Point.X * _roomSize.X * _tileSize.X, node.Point.Y * _roomSize.Y * _tileSize.Y);
                mapEntity.SetPosition(mapPosition);
                Scene.AddEntity(mapEntity);
                node.MapEntity = mapEntity;

                //load map
                var map = Scene.Content.LoadTiledMap(node.Map.Name);

                //create main map renderer
                var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map, "Walls"));
                mapRenderer.SetLayersToRender(new[] { "Back", "Walls" });
                mapRenderer.RenderLayer = 10;
                mapEntity.AddComponent(new GridGraphManager(mapRenderer));
                mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));
                Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

                //create above map renderer
                var tiledMapDetailsRenderer = mapEntity.AddComponent(new TiledMapRenderer(map));
                var layersToRender = new List<string>();
                if (map.Layers.Contains("Front"))
                    layersToRender.Add("Front");
                if (map.Layers.Contains("AboveFront"))
                    layersToRender.Add("AboveFront");
                tiledMapDetailsRenderer.SetLayersToRender(layersToRender.ToArray());
                tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
                tiledMapDetailsRenderer.Material = Material.StencilWrite();
                //tiledMapDetailsRenderer.Material.Effect = Scene.Content.LoadNezEffect<SpriteAlphaTestEffect>();
            }

            //destroy spawn triggers in the hub room
            Game1.Schedule(.1f, timer =>
            {
                var hubNode = Nodes.First(n => n.RoomType == RoomType.Hub);
                var enemySpawnTriggers = Scene.FindComponentsOfType<EnemySpawnTrigger>().Where(s => s.MapEntity == hubNode.MapEntity).ToList();
                foreach (var trigger in enemySpawnTriggers)
                {
                    trigger.Entity.Destroy();
                }
            });
        }

        bool FinalValidation()
        {
            foreach (var node in Nodes)
            {
                var aboveNode = Nodes.FirstOrDefault(n => n.Point == new Point(node.Point.X, node.Point.Y - 1));
                if (aboveNode != null)
                {
                    if (aboveNode.Map.HasBottom != node.Map.HasTop)
                    {
                        return false;
                    }
                }
                else if (node.Map.HasTop)
                {
                    return false;
                }

                var bottomNode = Nodes.FirstOrDefault(n => n.Point == new Point(node.Point.X, node.Point.Y + 1));
                if (bottomNode != null)
                {
                    if (bottomNode.Map.HasTop != node.Map.HasBottom)
                    {
                        return false;
                    }
                }
                else if (node.Map.HasBottom)
                {
                    return false;
                }

                var leftNode = Nodes.FirstOrDefault(n => n.Point == new Point(node.Point.X - 1, node.Point.Y));
                if (leftNode != null)
                {
                    if (leftNode.Map.HasRight != node.Map.HasLeft)
                    {
                        return false;
                    }
                }
                else if (node.Map.HasLeft)
                {
                    return false;
                }

                var rightNode = Nodes.FirstOrDefault(n => n.Point == new Point(node.Point.X + 1, node.Point.Y));
                if (rightNode != null)
                {
                    if (rightNode.Map.HasLeft != node.Map.HasRight)
                    {
                        return false;
                    }
                }
                else if (node.Map.HasRight)
                {
                    return false;
                }
            }

            return true;
        }

        void Reset()
        {
            Debug.Log("resetting dungeon generation");
            _graph = null;
            foreach(var node in Nodes)
            {
                node.MapEntity?.Destroy();
            }
            Nodes.Clear();
        }

        /// <summary>
        /// create grid graph and determine positions for special rooms
        /// </summary>
        public bool CreateGraph()
        {
            //create graph
            _graph = new AstarGridGraph(_gridSize.X, _gridSize.Y);

            //random position for starting location
            _hubPoint = new Point(Nez.Random.NextInt(_gridSize.X), Nez.Random.NextInt(_gridSize.Y));
            Nodes.Add(new RoomNode(_hubPoint, false, false, false, false, RoomType.Hub, Nodes));

            //pre boss room a certain distance from starting room
            bool preBossPlaced = false;
            while (!preBossPlaced)
            {
                //get random point
                int x = Nez.Random.Range(1, _gridSize.X - 1);
                int y = Nez.Random.Range(0, _gridSize.Y - 1); //boss room is always entered from bottom, so need space above pre boss
                Point potentialPos = new Point(x, y);

                //determine if pos is valid
                if (!IsGridPositionValid(potentialPos)) continue;

                //check map validity
                if (!IsGridPositionValidByMap(potentialPos, _preBossMap)) continue;

                //find path from hub to this point
                var path = _graph.Search(_hubPoint, potentialPos);

                //if no path, pos is invalid
                if (path == null) continue;

                //if path isn't far enough away from start, invalid
                if (path.Count < 3) continue;

                //point is valid. add to list
                _preBossPoint = potentialPos;
                var node = new RoomNode(_preBossPoint, false, true, true, true, RoomType.PreBoss, Nodes);
                node.Map = _preBossMap;
                Nodes.Add(node);
                preBossPlaced = true;
            }

            //add wall at point above pre boss if it is in the grid
            if (_preBossPoint.Y > 0)
            {
                _graph.Walls.Add(new Point(_preBossPoint.X, _preBossPoint.Y - 1));
            }

            //key room with door on right
            bool leftKeyPlaced = false;
            while (!leftKeyPlaced)
            {
                //get random point
                int x = Nez.Random.Range(0, _gridSize.X - 1);
                int y = Nez.Random.Range(0, _gridSize.Y);
                Point potentialPoint = new Point(x, y);

                //determine if pos is valid
                if (!IsGridPositionValid(potentialPoint)) continue;

                //map validity
                if (!IsGridPositionValidByMap(potentialPoint, _leftKeyMap)) continue;

                //point is valid
                _leftKeyPoint = potentialPoint;
                var node = new RoomNode(_leftKeyPoint, false, false, false, true, RoomType.Key, Nodes);
                node.Map = _leftKeyMap;
                Nodes.Add(node);
                //_graph.Walls.Add(_leftKeyPoint);
                leftKeyPlaced = true;
            }

            //key room with door on left
            bool rightKeyPlaced = false;
            while (!rightKeyPlaced)
            {
                //random point
                int x = Nez.Random.Range(1, _gridSize.X);
                int y = Nez.Random.Range(0, _gridSize.Y);
                Point potentialPoint = new Point(x, y);

                //determine if pos is valid
                if (!IsGridPositionValid(potentialPoint)) continue;

                //map validity
                if (!IsGridPositionValidByMap(potentialPoint, _rightKeyMap)) continue;

                //point valid
                _rightKeyPoint = potentialPoint;
                var node = new RoomNode(_rightKeyPoint, false, false, true, false, RoomType.Key, Nodes);
                node.Map = _rightKeyMap;
                Nodes.Add(node);
                //_graph.Walls.Add(_rightKeyPoint);
                rightKeyPlaced = true;
            }

            return true;
        }

        bool IsGridPositionValid(Point gridPosition)
        {
            if (Nodes.Any(n => n.Point == gridPosition)) return false;
            if (gridPosition == _preBossPoint + new Point(0, -1)) return false;
            return true;
        }

        /// <summary>
        /// check that map has no entrances leading out of grid, and that it won't block surrounding nodes doorways
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        bool IsGridPositionValidByMap(Point gridPosition, Map map)
        {
            var topPoint = gridPosition + new Point(0, -1);
            if (topPoint.Y < 0 && map.HasTop) return false;
            var topNode = Nodes.FirstOrDefault(n => n.Point == topPoint);
            if (topNode != null)
            {
                if (topNode.NeedsBottomDoor && !map.HasTop) return false;
            }

            var bottomPoint = gridPosition + new Point(0, 1);
            if (bottomPoint.Y >= _gridSize.Y && map.HasBottom) return false;
            var bottomNode = Nodes.FirstOrDefault(n => n.Point == bottomPoint);
            if (bottomNode != null)
            {
                if (bottomNode.NeedsTopDoor && !map.HasBottom) return false;
            }

            var leftPoint = gridPosition + new Point(-1, 0);
            if (leftPoint.X < 0 && map.HasLeft) return false;
            var leftNode = Nodes.FirstOrDefault(n => n.Point == leftPoint);
            if (leftNode != null)
            {
                if (leftNode.NeedsRightDoor && !map.HasLeft) return false;
            }

            var rightPoint = gridPosition + new Point(1, 0);
            if (rightPoint.X >= _gridSize.X && map.HasRight) return false;
            var rightNode = Nodes.FirstOrDefault(n => n.Point == rightPoint);
            if (rightNode != null)
            {
                if (rightNode.NeedsLeftDoor && !map.HasRight) return false;
            }

            return true;
        }

        bool CreatePathAndNodes(Point startPoint, Point endPoint)
        {
            var finalPath = new List<Point>();
            var walledPoints = new List<Point>();
            var pathFound = false;
            var currentPathStartPoint = startPoint;
            while (!pathFound)
            {
                var newPath = _graph.Search(currentPathStartPoint, endPoint);
                if (newPath == null) return false;
                foreach (var point in newPath)
                {
                    if (point == endPoint)
                    {
                        finalPath.Add(point);
                        pathFound = true;
                        break;
                    }

                    if (point == _leftKeyPoint || point == _rightKeyPoint || point == _preBossPoint + new Point(0, -1))
                    {
                        _graph.Walls.Add(point);
                        walledPoints.Add(point);
                        if (finalPath.Count > 0)
                        {
                            currentPathStartPoint = finalPath.Last();
                            break;
                        }
                        else return false;
                    }
                    else
                    {
                        finalPath.Add(point);
                    }
                }
            }

            //unwall points
            foreach (var point in walledPoints)
            {
                _graph.Walls.Remove(point);
            }

            //loop through final path, add node if necessary, and set where doors are needed
            for (int i = 0; i < finalPath.Count; i++)
            {
                var point = finalPath[i];

                RoomNode node;
                if (Nodes.FirstOrDefault(n => n.Point == point) != null)
                {
                    node = Nodes.FirstOrDefault(n => n.Point == point);
                }
                else
                {
                    node = new RoomNode(point, false, false, false, false, RoomType.Normal, Nodes);
                    Nodes.Add(node);
                }

                if (i > 0)
                {
                    Point previousPoint = finalPath[i - 1];
                    node.NeedsLeftDoor |= (previousPoint.X < node.Point.X);
                    node.NeedsTopDoor |= (previousPoint.Y < node.Point.Y);
                    node.NeedsRightDoor |= (previousPoint.X > node.Point.X);
                    node.NeedsBottomDoor |= (previousPoint.Y > node.Point.Y);
                }

                if (i < finalPath.Count - 1)
                {
                    Point nextPoint = finalPath[i + 1];
                    node.NeedsLeftDoor |= (nextPoint.X < node.Point.X);
                    node.NeedsTopDoor |= (nextPoint.Y < node.Point.Y);
                    node.NeedsRightDoor |= (nextPoint.X > node.Point.X);
                    node.NeedsBottomDoor |= (nextPoint.Y > node.Point.Y);
                }
            }

            return true;
        }

        /// <summary>
        /// generate paths from special rooms to hub
        /// </summary>
        public bool CreatePaths()
        {
            var leftKeyEntrancePoint = _leftKeyPoint + new Point(1, 0);
            if (!CreatePathAndNodes(leftKeyEntrancePoint, _hubPoint)) return false;
            var leftKeyEntranceNode = Nodes.Where(n => n.Point == leftKeyEntrancePoint).First();
            leftKeyEntranceNode.NeedsLeftDoor = true;

            var rightKeyEntrancePoint = _rightKeyPoint + new Point(-1, 0);
            if (!CreatePathAndNodes(rightKeyEntrancePoint, _hubPoint)) return false;
            var rightKeyEntranceNode = Nodes.Where(n => n.Point == rightKeyEntrancePoint).First();
            rightKeyEntranceNode.NeedsRightDoor = true;

            var leftPreBossEntrance = new Point(_preBossPoint.X - 1, _preBossPoint.Y);
            var rightPreBossEntrance = new Point(_preBossPoint.X + 1, _preBossPoint.Y);
            var bottomPreBossEntrance = new Point(_preBossPoint.X, _preBossPoint.Y + 1);
            List<Point> preBossEntrances = new List<Point>()
            {
                leftPreBossEntrance, rightPreBossEntrance, bottomPreBossEntrance
            };

            while (preBossEntrances.Count > 0)
            {
                var point = preBossEntrances.RandomItem();
                preBossEntrances.Remove(point);
                if (CreatePathAndNodes(point, _hubPoint))
                {
                    var preBossEntranceNode = Nodes.Where(n => n.Point == point).First();
                    if (preBossEntranceNode.Point == leftPreBossEntrance)
                    {
                        preBossEntranceNode.NeedsRightDoor = true;
                    }
                    else if (preBossEntranceNode.Point == rightPreBossEntrance)
                    {
                        preBossEntranceNode.NeedsLeftDoor = true;
                    }
                    else if (preBossEntranceNode.Point == bottomPreBossEntrance)
                    {
                        preBossEntranceNode.NeedsTopDoor = true;
                    }

                    break;
                }
                else if (preBossEntrances.Count == 0) return false;
            }

            return true;
        }

        /// <summary>
        /// create maps for each node in the grid
        /// </summary>
        public bool CreateMaps()
        {
            foreach (var node in Nodes)
            {
                if (node.Map == null)
                {
                    var topPoint = new Point(node.Point.X, node.Point.Y - 1);
                    if (topPoint.Y < 0) node.NeedsTopDoor = false;
                    var topNode = Nodes.FirstOrDefault(n => n.Point ==  topPoint);
                    if (topNode != null)
                    {
                        if (topNode.NeedsBottomDoor) node.NeedsTopDoor = true;
                        else if (topNode.Map != null)
                        {
                            if (topNode.Map.HasBottom) node.NeedsTopDoor = true;
                        }
                    }

                    var bottomPoint = new Point(node.Point.X, node.Point.Y + 1);
                    if (bottomPoint.Y >= _gridSize.Y) node.NeedsBottomDoor = false;
                    var bottomNode = Nodes.FirstOrDefault(n => n.Point == bottomPoint);
                    if (bottomNode != null)
                    {
                        if (bottomNode.NeedsTopDoor) node.NeedsBottomDoor = true;
                        else if (bottomNode.Map != null)
                        {
                            if (bottomNode.Map.HasTop) node.NeedsBottomDoor = true;
                        }
                    }

                    var leftPoint = new Point(node.Point.X - 1, node.Point.Y);
                    if (leftPoint.X < 0) node.NeedsLeftDoor = false;
                    var leftNode = Nodes.FirstOrDefault(n => n.Point == leftPoint);
                    if (leftNode != null)
                    {
                        if (leftNode.NeedsRightDoor) node.NeedsLeftDoor = true;
                        else if (leftNode.Map != null)
                        {
                            if (leftNode.Map.HasRight) node.NeedsLeftDoor = true;
                        }
                    }

                    var rightPoint = new Point(node.Point.X + 1, node.Point.Y);
                    if (rightPoint.X >= _gridSize.X) node.NeedsRightDoor = false;
                    var rightNode = Nodes.FirstOrDefault(n => n.Point == rightPoint);
                    if (rightNode != null)
                    {
                        if (rightNode.NeedsLeftDoor) node.NeedsRightDoor = true;
                        else if (rightNode.Map != null)
                        {
                            if (rightNode.Map.HasLeft) node.NeedsRightDoor = true;
                        }
                    }

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
                    }).ToList();

                    if (possibleMaps.Any())
                    {
                        node.Map = possibleMaps.RandomItem();
                    }
                    else return false;
                }

                //var mapEntity = new PausableEntity($"map-room-${node.Point.X}-${node.Point.Y}");
                //var mapPosition = new Vector2(node.Point.X * _roomSize.X * _tileSize.X, node.Point.Y * _roomSize.Y * _tileSize.Y);
                //mapEntity.SetPosition(mapPosition);
                //Scene.AddEntity(mapEntity);
                //var map = Scene.Content.LoadTiledMap(node.Map.Name);
                //var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
                //mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
                //mapRenderer.RenderLayer = 10;
                //mapEntity.AddComponent(new GridGraphManager(mapRenderer));
                //mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));
                //node.MapEntity = mapEntity;

                //switch (node.RoomType)
                //{
                //    case RoomType.Hub:
                //        //CreateMap(node, Nez.Content.Tiled.Tilemaps.Hub_room);
                //        if (!CreateMap(node)) return false;
                //        break;
                //    case RoomType.Key:
                //        var mapString = node.NeedsLeftDoor ? Content.Tiled.Tilemaps.Right_key_room : Content.Tiled.Tilemaps.Left_key_room;
                //        if (!CreateMap(node, mapString)) return false;
                //        break;
                //    case RoomType.PreBoss:
                //        if (!CreateMap(node, Content.Tiled.Tilemaps.Pre_boss_room)) return false;
                //        //if (!CreateMap(node)) return false;
                //        break;
                //    case RoomType.Boss:
                //        if (!CreateMap(node, Content.Tiled.Tilemaps.Boss_room)) return false;
                //        break;
                //    case RoomType.Normal:
                //        if (!CreateMap(node)) return false;
                //        break;
                //}
            }

            return true;
        }

        /// <summary>
        /// Load map and create map entity
        /// </summary>
        /// <param name="node"></param>
        /// <param name="mapString"></param>
        bool CreateMap(RoomNode node, string mapString = null)
        {
            if (string.IsNullOrWhiteSpace(mapString))
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
                }).ToList();

                if (possibleMaps.Any())
                {
                    node.Map = possibleMaps.RandomItem();
                    mapString = node.Map.Name;
                }
                else return false;
            }

            //var mapEntity = new PausableEntity($"map-room-${node.Point.X}-${node.Point.Y}");
            //var mapPosition = new Vector2(node.Point.X * _roomSize.X * _tileSize.X, node.Point.Y * _roomSize.Y * _tileSize.Y);
            //mapEntity.SetPosition(mapPosition);
            //Scene.AddEntity(mapEntity);
            //var map = Scene.Content.LoadTiledMap(mapString);
            //var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map, "collision"));
            //mapRenderer.SetLayersToRender(new[] { "floor", "details", "entities", "above-details" });
            //mapRenderer.RenderLayer = 10;
            //mapEntity.AddComponent(new GridGraphManager(mapRenderer));
            //mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));
            //node.MapEntity = mapEntity;

            return true;
        }

        /// <summary>
        /// check a node's neighbors to determine which directions we need doors in
        /// </summary>
        /// <param name="node"></param>
        void CheckSurroundingNodes(ref RoomNode node)
        {
            var topPoint = new Point(node.Point.X, node.Point.Y - 1);
            var top = Nodes.Where(n => n.Point == topPoint).FirstOrDefault();
            node.NeedsTopDoor = CheckIfNeedsDoor(top, node);

            var bottomPoint = new Point(node.Point.X, node.Point.Y + 1);
            var bottom = Nodes.Where(n => n.Point == bottomPoint).FirstOrDefault();
            node.NeedsBottomDoor = CheckIfNeedsDoor(bottom, node);

            var leftPoint = new Point(node.Point.X - 1, node.Point.Y);
            var left = Nodes.Where(n => n.Point == leftPoint).FirstOrDefault();
            node.NeedsLeftDoor = CheckIfNeedsDoor(left, node);

            var rightPoint = new Point(node.Point.X + 1, node.Point.Y);
            var right = Nodes.Where(n => n.Point == rightPoint).FirstOrDefault();
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

        void ProcessUnconnectedDoors()
        {
            Debug.Log("starting processing unconnected doors");
            var nodesToProcess = new Queue<RoomNode>(Nodes);
            while (nodesToProcess.Count > 0)
            {
                var node = nodesToProcess.Dequeue();
                Debug.Log($"Processing node: {node.RoomType.ToString()}");
                var unconnectedDoors = node.GetUnconnectedDoors();
                foreach (var direction in unconnectedDoors)
                {
                    var adjacentPoint = GetAdjacentPoint(node.Point, direction);
                    var adjacentNode = Nodes.FirstOrDefault(n => n.Point == adjacentPoint);
                    if (adjacentNode == null)
                    {
                        // Create a new room at the adjacent point
                        adjacentNode = new RoomNode(adjacentPoint, false, false, false, false, RoomType.Normal, Nodes);
                        Nodes.Add(adjacentNode);
                        CreateMap(adjacentNode);

                        nodesToProcess.Enqueue(adjacentNode);
                    }
                }
            }
        }

        Point GetAdjacentPoint(Point point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return new Point(point.X, point.Y - 1);
                case Direction.Down: return new Point(point.X, point.Y + 1);
                case Direction.Left: return new Point(point.X - 1, point.Y);
                case Direction.Right: return new Point(point.X + 1, point.Y);
                default: throw new ArgumentException("Invalid direction");
            }
        }
    }
}