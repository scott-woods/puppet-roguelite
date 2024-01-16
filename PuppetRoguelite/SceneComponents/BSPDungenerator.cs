using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.DeferredLighting;
using Nez.Tiled;
using PuppetRoguelite.Components;
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
    public class BSPDungenerator : SceneComponent
    {
        /// <summary>
        /// size of the entire dungeon in tiles
        /// </summary>
        Vector2 _dungeonArea = new Vector2(160, 160);
        Vector2 _tileSize = new Vector2(16, 16);
        const int _maxSplits = 3;
        const int _maxLeafSize = 48;

        List<DungeonLeaf> _leafs = new List<DungeonLeaf>();

        public void Generate()
        {
            var root = new DungeonLeaf(0, 0, (int)_dungeonArea.X, (int)_dungeonArea.Y);
            _leafs.Add(root);

            //recursively loop through and split leafs until we can't continue
            bool hasSplit = true;
            while (hasSplit)
            {
                hasSplit = false;
                
                //loop through leafs
                foreach (var leaf in _leafs.ToList())
                {
                    //if leaf doesn't have children yet
                    if (leaf.LeftChild == null && leaf.RightChild == null)
                    {
                        //if leaf is too big, or random chance
                        if (leaf.Size.X > _maxLeafSize || leaf.Size.Y > _maxLeafSize || Nez.Random.Chance(.75f))
                        {
                            //try to split the leaf
                            if (leaf.Split())
                            {
                                //if split was successful, add new leafs to list and continue while loop
                                _leafs.Add(leaf.LeftChild);
                                _leafs.Add(leaf.RightChild);
                                hasSplit = true;
                            }
                        }
                    }
                }
            }

            //handle pre boss room
            //var preBossPlaced = false;
            //var potentialLeafs = new List<DungeonLeaf>();
            //potentialLeafs.InsertRange(0, _leafs);
            //var preBossMap = Maps.ForgePreBoss;
            //while (!preBossPlaced)
            //{
            //    var potentialLeaf = potentialLeafs.RandomItem();
            //    var fits = preBossMap.TmxMap.Width <= potentialLeaf.Size.X + 5 && preBossMap.TmxMap.Height <= potentialLeaf.Size.Y + 6;

            //    if (!fits)
            //        potentialLeafs.Remove(potentialLeaf);
            //    else
            //    {
            //        var pos = new Vector2(Nez.Random.Range(1, (int)(potentialLeaf.Size.X - preBossMap.TmxMap.Width - 1)) + potentialLeaf.Position.X, Nez.Random.Range(1, (int)(potentialLeaf.Size.Y - preBossMap.TmxMap.Height - 1)) + potentialLeaf.Position.Y);
            //        potentialLeaf.Room = new DungeonRoom(preBossMap, pos);
            //        preBossPlaced = true;
            //    }
            //}

            //create rooms in each leaf that is at the bottom of the tree
            foreach (var leaf in _leafs.Where(l => l.LeftChild == null && l.RightChild == null && l.Room == null))
            {
                //get all possible maps that will fit in this partition
                var possibleMaps = Maps.ForgeMaps.Where((m) =>
                {
                    //determine if map will fit in leaf
                    var fits = m.TmxMap.Width <= leaf.Size.X + 5 && m.TmxMap.Height <= leaf.Size.Y + 6;

                    //if doesn't fit, unload
                    if (!fits)
                        Game1.Scene.Content.UnloadAsset<TmxMap>(m.Name);

                    return fits;
                }).ToList();

                //if no possible maps, don't do anything i guess
                if (possibleMaps.Count == 0)
                    return;

                var map = possibleMaps.RandomItem();
                var pos = new Vector2(Nez.Random.Range(1, (int)(leaf.Size.X - map.TmxMap.Width - 1)) + leaf.Position.X, Nez.Random.Range(1, (int)(leaf.Size.Y - map.TmxMap.Height - 1)) + leaf.Position.Y);

                leaf.Room = new DungeonRoom(map, pos);
            }

            //loop through each leaf that has a room in it and instantiate the room entity
            foreach (var room in _leafs.Where(l => l.Room != null).Select(l => l.Room).ToList())
            {
                //create map entity
                var mapEntity = new PausableEntity($"map-room-${room.Position.X}-${room.Position.Y}");
                mapEntity.AddComponent(room);
                var mapPosition = new Vector2(room.Position.X * _tileSize.X, room.Position.Y * _tileSize.Y);
                mapEntity.SetPosition(mapPosition);
                Scene.AddEntity(mapEntity);
                //node.MapEntity = mapEntity;

                //load map
                var tmxMap = room.Map.TmxMap;

                //create main map renderer
                var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(tmxMap, "Walls"));
                mapRenderer.SetLayersToRender(new[] { "Back", "Walls" });
                mapRenderer.RenderLayer = 10;
                mapEntity.AddComponent(new GridGraphManager(mapRenderer));
                mapEntity.AddComponent(new TiledObjectHandler(mapRenderer));
                Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

                //create above map renderer
                var tiledMapDetailsRenderer = mapEntity.AddComponent(new TiledMapRenderer(tmxMap));
                var layersToRender = new List<string>();
                if (tmxMap.Layers.Contains("Front"))
                    layersToRender.Add("Front");
                if (tmxMap.Layers.Contains("AboveFront"))
                    layersToRender.Add("AboveFront");
                tiledMapDetailsRenderer.SetLayersToRender(layersToRender.ToArray());
                tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
                tiledMapDetailsRenderer.Material = Material.StencilWrite();
                //tiledMapDetailsRenderer.Material.Effect = Scene.Content.LoadNezEffect<SpriteAlphaTestEffect>();
            }

            //create pathfinding graph. anything that is part of a map is a wall in terms of hallway pathfinding
            var graph = new AstarGridGraph((int)_dungeonArea.X, (int)_dungeonArea.Y);
            var mapRenderers = Scene.FindComponentsOfType<TiledMapRenderer>();
            foreach (var renderer in mapRenderers)
            {
                for (var y = 0; y < renderer.TiledMap.Height; y++)
                {
                    for (var x = 0; x < renderer.TiledMap.Width; x++)
                    {
                        graph.Walls.Add(new Point((int)(renderer.Entity.Position.X / 16) + x, (int)(renderer.Entity.Position.Y / 16) + y));
                    }
                }
            }

            //loop through leafs that have left and right children that both have rooms, and connect those rooms
            foreach (var leaf in _leafs.Where(l => l.LeftChild != null && l.RightChild != null && l.LeftChild.Room != null && l.RightChild.Room != null))
            {
                var room1 = leaf.LeftChild.Room;
                var room2 = leaf.RightChild.Room;

                //get the positions of the closest two exits
                var minDistance = float.MaxValue;
                DungeonDoorway selectedDoorway1 = null;
                DungeonDoorway selectedDoorway2 = null;
                Vector2 room1ExitPosition = new Vector2();
                Vector2 room2ExitPosition = new Vector2();
                foreach (var doorway1 in room1.Doorways)
                {
                    foreach (var doorway2 in room2.Doorways)
                    {
                        var pos1 = (doorway1.Entity.Position / 16) + doorway1.PathfindingOffset;
                        var pos2 = (doorway2.Entity.Position / 16) + doorway2.PathfindingOffset;
                        var dist = Vector2.Distance(pos1, pos2);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            room1ExitPosition = pos1;
                            room2ExitPosition = pos2;
                            selectedDoorway1 = doorway1;
                            selectedDoorway2 = doorway2;
                        }
                    }
                }

                selectedDoorway1.SetOpen(true);
                selectedDoorway2.SetOpen(true);

                //add padding
                Vector2 room1ExitPositionPadded = new Vector2(room1ExitPosition.X, room1ExitPosition.Y);
                switch (selectedDoorway1.Direction)
                {
                    case "Top":
                        room1ExitPositionPadded.Y -= 2;
                        break;
                    case "Bottom":
                        room1ExitPositionPadded.Y += 5;
                        break;
                    case "Left":
                        room1ExitPositionPadded.X -= 3;
                        break;
                    case "Right":
                        room1ExitPositionPadded.X += 3;
                        break;
                }
                Vector2 room2ExitPositionPadded = new Vector2(room2ExitPosition.X, room2ExitPosition.Y);
                switch (selectedDoorway2.Direction)
                {
                    case "Top":
                        room2ExitPositionPadded.Y -= 2;
                        break;
                    case "Bottom":
                        room2ExitPositionPadded.Y += 5;
                        break;
                    case "Left":
                        room2ExitPositionPadded.X -= 3;
                        break;
                    case "Right":
                        room2ExitPositionPadded.X += 3;
                        break;
                }

                //remove walls from the exit positions
                graph.Walls.Remove(room1ExitPosition.ToPoint());
                graph.Walls.Remove(room2ExitPosition.ToPoint());

                //assemble final path
                List<Point> finalPath = new List<Point>();
                var path1 = graph.Search(room1ExitPosition.ToPoint(), room1ExitPositionPadded.ToPoint());
                if (path1 != null)
                    finalPath.AddRange(path1);
                var path2 = graph.Search(room1ExitPositionPadded.ToPoint(), room2ExitPositionPadded.ToPoint());
                if (path2 != null)
                    finalPath.AddRange(path2);
                var path3 = graph.Search(room2ExitPositionPadded.ToPoint(), room2ExitPosition.ToPoint());
                if (path3 != null)
                    finalPath.AddRange(path3);

                finalPath = finalPath.Distinct().ToList();

                List<HallwayModel> hallwayModels = new List<HallwayModel>();

                for (int i = 1; i < finalPath.Count - 1; i++)
                {
                    var previousPoint = finalPath[i - 1];
                    var point = finalPath[i];
                    var nextPoint = finalPath[i + 1];

                    Vector2 direction1 = new Vector2(point.X - previousPoint.X, point.Y - previousPoint.Y);
                    Vector2 direction2 = new Vector2(nextPoint.X - point.X, nextPoint.Y - point.Y);
                    direction1.Normalize();
                    direction2.Normalize();

                    //horizontal
                    if (direction1.X == direction2.X && direction1.Y == 0 && direction2.Y == 0)
                    {
                        var hallway = new HallwayModel(HallwayMaps.ForgeHorizontal, point, false);
                        hallwayModels.Add(hallway);
                    }
                    //vertical
                    if (direction1.Y == direction2.Y && direction1.X == 0 && direction2.X == 0)
                    {
                        var hallway = new HallwayModel(HallwayMaps.ForgeVertical, point, false);
                        hallwayModels.Add(hallway);
                    }
                    //bottom right corner
                    if ((direction1.Y == 1 && direction2.X == -1) || (direction1.X == 1 && direction2.Y == -1))
                    {
                        var hallway = new HallwayModel(HallwayMaps.ForgeBottomRight, point);
                        hallwayModels.Add(hallway);

                        var tmxMap = hallway.Map.TmxMap;
                        if (tmxMap.Properties.TryGetValue("PathfindingOffset", out var offsetString))
                        {
                            var offsetValues = offsetString.Split(' ');

                            var prevDirectionValue = 0;
                            var nextDirectionValue = 0;
                            if (direction1.X == 1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[0]);
                                nextDirectionValue = Convert.ToInt32(offsetValues[1]);
                            }

                            if (direction1.Y == 1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[1]);
                                nextDirectionValue = Convert.ToInt32(offsetValues[0]);
                                
                            }

                            var startIndex = Math.Max(hallwayModels.Count - 1 - prevDirectionValue, 0);
                            var countToRemove = hallwayModels.Count - 1 - startIndex;
                            hallwayModels.RemoveRange(startIndex, countToRemove);
                            i += nextDirectionValue;
                        }
                    }
                    //bottom left corner
                    else if ((direction1.Y == 1 && direction2.X == 1) || (direction1.X == -1 && direction2.Y == -1))
                    {
                        var hallway = new HallwayModel(HallwayMaps.ForgeBottomLeft, point);
                        hallwayModels.Add(hallway);

                        var tmxMap = hallway.Map.TmxMap;
                        if (tmxMap.Properties.TryGetValue("PathfindingOffset", out var offsetString))
                        {
                            var offsetValues = offsetString.Split(' ');

                            var prevDirectionValue = 0;
                            var nextDirectionValue = 0;
                            if (direction1.X == -1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[0]);
                                nextDirectionValue = Convert.ToInt32(offsetValues[1]);
                            }

                            if (direction1.Y == 1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[1]);
                                nextDirectionValue = Convert.ToInt32(offsetValues[0]);

                            }

                            var startIndex = Math.Max(hallwayModels.Count - 1 - prevDirectionValue, 0);
                            var countToRemove = hallwayModels.Count - 1 - startIndex;
                            hallwayModels.RemoveRange(startIndex, countToRemove);
                            i += nextDirectionValue;
                        }
                    }
                    //top left corner
                    else if ((direction1.Y == -1 && direction2.X == 1) || (direction1.X == -1 && direction2.Y == 1))
                    {
                        var hallway = new HallwayModel(HallwayMaps.ForgeTopLeft, point);
                        hallwayModels.Add(hallway);

                        var tmxMap = hallway.Map.TmxMap;
                        if (tmxMap.Properties.TryGetValue("PathfindingOffset", out var offsetString))
                        {
                            var offsetValues = offsetString.Split(' ');

                            var prevDirectionValue = 0;
                            var nextDirectionValue = 0;
                            if (direction1.X == -1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[0]);
                                nextDirectionValue = Convert.ToInt32(offsetValues[1]) - 3;
                            }

                            if (direction1.Y == -1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[1]) - 3;
                                nextDirectionValue = Convert.ToInt32(offsetValues[0]);
                            }

                            var startIndex = Math.Max(hallwayModels.Count - 1 - prevDirectionValue, 0);
                            var countToRemove = hallwayModels.Count - 1 - startIndex;
                            hallwayModels.RemoveRange(startIndex, countToRemove);
                            i += nextDirectionValue;
                        }
                    }
                    //top right corner
                    else if ((direction1.Y == -1 && direction2.X == -1) || (direction1.X == 1) && (direction2.Y == 1))
                    {
                        var hallway = new HallwayModel(HallwayMaps.ForgeTopRight, point);
                        hallwayModels.Add(hallway);

                        var tmxMap = hallway.Map.TmxMap;
                        if (tmxMap.Properties.TryGetValue("PathfindingOffset", out var offsetString))
                        {
                            var offsetValues = offsetString.Split(' ');

                            var prevDirectionValue = 0;
                            var nextDirectionValue = 0;
                            if (direction1.X == 1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[0]);
                                nextDirectionValue = Convert.ToInt32(offsetValues[1]) - 3;
                            }

                            if (direction1.Y == -1)
                            {
                                prevDirectionValue = Convert.ToInt32(offsetValues[1]) - 3;
                                nextDirectionValue = Convert.ToInt32(offsetValues[0]);
                            }

                            var startIndex = Math.Max(hallwayModels.Count - 1 - prevDirectionValue, 0);
                            var countToRemove = hallwayModels.Count - 1 - startIndex;
                            hallwayModels.RemoveRange(startIndex, countToRemove);
                            i += nextDirectionValue;
                        }
                    }
                }

                foreach (var point in finalPath)
                {
                    var ent = Scene.CreateEntity("pathfinding-test");
                    ent.SetPosition(point.X * 16, point.Y * 16);
                    var prototypeSpriteRenderer = ent.AddComponent(new PrototypeSpriteRenderer(4, 4));
                    prototypeSpriteRenderer.RenderLayer = int.MinValue;
                }

                foreach (var hallway in hallwayModels)
                {
                    //load map
                    var tmxMap = hallway.Map.TmxMap;
                    var hallwayPos = new Vector2(hallway.PathPoint.X, hallway.PathPoint.Y);
                    if (tmxMap.Properties.TryGetValue("PathfindingOffset", out var offsetString))
                    {
                        var offsetValues = offsetString.Split(' ');
                        hallwayPos.X -= Convert.ToInt32(offsetValues[0]);
                        hallwayPos.Y -= Convert.ToInt32(offsetValues[1]);
                    }

                    var ent = Scene.CreateEntity("hallway");
                    ent.SetPosition(hallwayPos.X * 16, hallwayPos.Y * 16);

                    //create main map renderer
                    var mapRenderer = ent.AddComponent(new TiledMapRenderer(tmxMap, "Walls"));
                    mapRenderer.SetLayersToRender(new[] { "Back", "Walls" });
                    mapRenderer.RenderLayer = 10;
                    ent.AddComponent(new GridGraphManager(mapRenderer));
                    ent.AddComponent(new TiledObjectHandler(mapRenderer));
                    Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

                    //create above map renderer
                    var tiledMapDetailsRenderer = ent.AddComponent(new TiledMapRenderer(tmxMap));
                    var layersToRender = new List<string>();
                    if (tmxMap.Layers.Contains("Front"))
                        layersToRender.Add("Front");
                    if (tmxMap.Layers.Contains("AboveFront"))
                        layersToRender.Add("AboveFront");
                    tiledMapDetailsRenderer.SetLayersToRender(layersToRender.ToArray());
                    tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
                    tiledMapDetailsRenderer.Material = Material.StencilWrite();
                    //tiledMapDetailsRenderer.Material.Effect = Scene.Content.LoadNezEffect<SpriteAlphaTestEffect>();
                }

                //var path = graph.Search(room1ExitPosition.ToPoint(), room2ExitPosition.ToPoint());

                //if (path != null)
                //{
                //    List<Point> corners = new List<Point>();
                //    bool isPathValid = false;
                //    int attempts = 0;
                //    while (!isPathValid && attempts < 10000)
                //    {
                //        attempts++;

                //        bool pathAdjusted = false;
                //        for (int i = 1; i < path.Count - 1; i++)
                //        {
                //            var previousPoint = path[i - 1];
                //            var point = path[i];
                //            var nextPoint = path[i + 1];

                //            Vector2 direction1 = new Vector2(point.X - previousPoint.X, point.Y - previousPoint.Y);
                //            Vector2 direction2 = new Vector2(nextPoint.X - point.X, nextPoint.Y - point.Y);
                //            direction1.Normalize();
                //            direction2.Normalize();

                //            var isTurn = direction1 != direction2;

                //            if (isTurn)
                //            {
                //                //bottom right corner
                //                if ((direction1.Y == -1 && direction2.X == 1) || (direction1.X == 1 && direction2.Y == -1))
                //                {
                //                    var pointsAbove = CountPointsInDirection(point, new Point(0, -1), path, 4);
                //                    var pointsLeft = CountPointsInDirection(point, new Point(-1, 0), path, 2);
                //                    var offset = new Point(pointsLeft < 2 ? 2 - pointsLeft : 0, pointsAbove < 4 ? 4 - pointsAbove : 0);
                //                    var adjustedCornerPoint = point + offset;
                //                    //corners.Clear();
                //                    //corners.Add(adjustedCornerPoint);
                //                    //path.Clear();
                //                    //var startPoint = room1ExitPosition.ToPoint();
                //                    //foreach (var corner in corners)
                //                    //{
                //                    //    var pathToCorner = graph.Search(startPoint, corner);
                //                    //    if (pathToCorner != null)
                //                    //        path.AddRange(pathToCorner);
                //                    //    startPoint = corner;
                //                    //}
                //                    //var pathToExit = graph.Search(startPoint, room2ExitPosition.ToPoint());
                //                    //if (pathToExit != null)
                //                    //    path.AddRange(pathToExit);
                //                    //pathAdjusted = true;
                //                    //break;
                //                }
                //                //bottom left corner
                //                else if ((direction1.Y == 1 && direction2.X == 1) || (direction1.X == -1 && direction2.Y == -1))
                //                {
                //                    var pointsAbove = CountPointsInDirection(point, new Point(0, -1), path, 4);
                //                    var pointsRight = CountPointsInDirection(point, new Point(1, 0), path, 2);
                //                    var offset = new Point(pointsRight < 2 ? pointsRight - 2 : 0, pointsAbove < 4 ? 4 - pointsAbove : 0);
                //                    var adjustedCornerPoint = point + offset;
                //                    //corners.Clear();
                //                    //corners.Add(adjustedCornerPoint);
                //                    //path.Clear();
                //                    //var startPoint = room1ExitPosition.ToPoint();
                //                    //foreach (var corner in corners)
                //                    //{
                //                    //    var pathToCorner = graph.Search(startPoint, corner);
                //                    //    if (pathToCorner != null)
                //                    //        path.AddRange(pathToCorner);
                //                    //    startPoint = corner;
                //                    //}
                //                    //var pathToExit = graph.Search(startPoint, room2ExitPosition.ToPoint());
                //                    //if (pathToExit != null)
                //                    //    path.AddRange(pathToExit);
                //                    //pathAdjusted = true;
                //                    //break;
                //                }
                //                //top left corner
                //                else if ((direction1.Y == -1 && direction2.X == 1) || (direction1.X == -1 && direction2.Y == 1))
                //                {
                //                    var pointsBelow = CountPointsInDirection(point, new Point(0, 1), path, 1);
                //                    var pointsRight = CountPointsInDirection(point, new Point(1, 0), path, 2);
                //                    var offset = new Point(pointsRight < 2 ? pointsRight - 2 : 0, pointsBelow < 4 ? pointsBelow - 4 : 0);
                //                    var adjustedCornerPoint = point + offset;
                //                    //corners.Clear();
                //                    //corners.Add(adjustedCornerPoint);
                //                    //path.Clear();
                //                    //var startPoint = room1ExitPosition.ToPoint();
                //                    //foreach (var corner in corners)
                //                    //{
                //                    //    var pathToCorner = graph.Search(startPoint, corner);
                //                    //    if (pathToCorner != null)
                //                    //        path.AddRange(pathToCorner);
                //                    //    startPoint = corner;
                //                    //}
                //                    //var pathToExit = graph.Search(startPoint, room2ExitPosition.ToPoint());
                //                    //if (pathToExit != null)
                //                    //    path.AddRange(pathToExit);
                //                    //pathAdjusted = true;
                //                    //break;
                //                }
                //                //top right corner
                //                else if ((direction1.Y == -1 && direction2.X == -1) || (direction1.X == 1) && (direction2.Y == 1))
                //                {
                //                    var pointsBelow = CountPointsInDirection(point, new Point(0, 1), path, 1);
                //                    var pointsLeft = CountPointsInDirection(point, new Point(-1, 0), path, 2);
                //                    var offset = new Point(pointsLeft < 2 ? 2 - pointsLeft : 0, pointsBelow < 4 ? pointsBelow - 4 : 0);
                //                    var adjustedCornerPoint = point + offset;
                //                    //corners.Clear();
                //                    //corners.Add(adjustedCornerPoint);
                //                    //path.Clear();
                //                    //var startPoint = room1ExitPosition.ToPoint();
                //                    //foreach (var corner in corners)
                //                    //{
                //                    //    var pathToCorner = graph.Search(startPoint, corner);
                //                    //    if (pathToCorner != null)
                //                    //        path.AddRange(pathToCorner);
                //                    //    startPoint = corner;
                //                    //}
                //                    //var pathToExit = graph.Search(startPoint, room2ExitPosition.ToPoint());
                //                    //if (pathToExit != null)
                //                    //    path.AddRange(pathToExit);
                //                    //pathAdjusted = true;
                //                    //break;
                //                }
                //            }
                //        }

                //        if (!pathAdjusted)
                //            isPathValid = true;
                //    }

                //    foreach (var point in path)
                //    {
                //        var ent = Scene.CreateEntity("pathfinding-test");
                //        ent.SetPosition(point.X * 16, point.Y * 16);
                //        var prototypeSpriteRenderer = ent.AddComponent(new PrototypeSpriteRenderer(4, 4));
                //        prototypeSpriteRenderer.RenderLayer = int.MinValue;
                //    }
                //}
            }
        }

        int CountPointsInDirection(Point start, Point direction, List<Point> path, int requiredCount)
        {
            Point current = start;
            int count = 0;

            for (int i = 0; i < requiredCount; i++)
            {
                current = new Point(current.X + direction.X, current.Y + direction.Y);

                if (path.Contains(current))
                {
                    count++;
                }
                else
                    break;
            }

            return count;
        }

        public void CreateHall(DungeonRoom leftRoom, DungeonRoom rightRoom)
        {

        }
    }
}
