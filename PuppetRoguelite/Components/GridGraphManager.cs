using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Splines;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class GridGraphManager : Component
    {
        public string MapId { get; set; }

        AstarGridGraph _graph;
        TmxMap _map;

        public GridGraphManager(AstarGridGraph graph, TmxMap map, string mapId)
        {
            _graph = graph;
            _map = map;

            MapId = mapId;
        }

        public Point WorldToGridPosition(Vector2 worldPosition)
        {
            //subtract entity position to get position relative to this grid graph
            return _map.WorldToTilePosition(worldPosition - Entity.Position);
        }

        public Vector2 GridToWorldPosition(Point gridPosition)
        {
            //subtract entity position to get position relative to this grid graph
            return _map.TileToWorldPosition(gridPosition) + Entity.Position;
        }

        /// <summary>
        /// given a start and end point in world space, return list of world space positions along grid between them
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public List<Vector2> FindPath(Vector2 startPoint, Vector2 endPoint)
        {
            //determine basic path along grid
            var gridPath = _graph.Search(WorldToGridPosition(startPoint), WorldToGridPosition(endPoint));

            //if no path found, just return the end point
            if (gridPath == null)
            {
                return new List<Vector2> { endPoint };
            }

            //modify path into list of Vector2 in world space (in center of tile)
            List<Vector2> path = new List<Vector2>();
            foreach (var pathItem in gridPath)
            {
                path.Add(GridToWorldPosition(pathItem) + new Vector2(8, 8));
            }

            //if first point on path is further from target than start entity, remove it
            if (path.Count > 1)
            {
                var startToEndDistance = Math.Abs(Vector2.Distance(startPoint, endPoint));
                var firstPointToEndDistance = Math.Abs(Vector2.Distance(path[0], endPoint));
                if (startToEndDistance < firstPointToEndDistance)
                {
                    path.RemoveAt(0);
                }
            }

            //add target as final point
            if (path.Last() != endPoint)
            {
                path.Add(endPoint);
            }

            //if we only have one point here, return it
            if (path.Count == 1)
            {
                return path;
            }

            //simplify path
            //List<Vector2> simplifiedPath = new List<Vector2>();
            //int currentIndex = 0;
            //while (currentIndex < path.Count - 1)
            //{
            //    int furthestVisibleIndex = currentIndex;
            //    for (int i = currentIndex + 1; i < path.Count; i++)
            //    {
            //        var raycastHit = Physics.Linecast(path[currentIndex], path[i], 1 << 0);
            //        if (raycastHit.Collider == null)
            //        {
            //            furthestVisibleIndex = i;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }

            //    //safeguard against getting stuck in while loop
            //    if (furthestVisibleIndex == currentIndex)
            //    {
            //        currentIndex++;
            //    }
            //    else
            //    {
            //        currentIndex = furthestVisibleIndex;
            //    }

            //    simplifiedPath.Add(path[currentIndex]);
            //}

            //smooth path
            //List<Vector2> smoothedPath = new List<Vector2>();
            //if (gridPath.Count >= 3)
            //{
            //    for (int i = 0; i < gridPath.Count - 2; i++)
            //    {
            //        var a = GridToWorldPosition(gridPath[i]);
            //        var b = GridToWorldPosition(gridPath[i + 1]);
            //        var c = GridToWorldPosition(gridPath[i + 2]);

            //        smoothedPath.Add(a);

            //        for (float t = .1f; t < 1; t += .1f)
            //        {
            //            smoothedPath.Add(Bezier.GetPoint(a, b, c, t));
            //        }

            //        smoothedPath.Add(c);
            //    }
            //}
            //else
            //{
            //    foreach(var point in gridPath)
            //    {
            //        smoothedPath.Add(GridToWorldPosition(point));
            //    }
            //}

            //return simplifiedPath;
            return path;
        }
    }
}
