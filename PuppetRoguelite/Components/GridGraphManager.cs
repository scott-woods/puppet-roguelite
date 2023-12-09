using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Splines;
using Nez.Tiled;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class GridGraphManager : Component
    {
        TiledMapRenderer _mapRenderer;
        AstarGridGraph _graph;

        public GridGraphManager(TiledMapRenderer mapRenderer)
        {
            _mapRenderer = mapRenderer;
        }

        public override void Initialize()
        {
            base.Initialize();

            _graph = new AstarGridGraph(_mapRenderer.CollisionLayer);
        }

        public void HandleCombatArea()
        {
            for (int i = 0; i < _mapRenderer.CollisionLayer.Width; i++)
            {
                for (int j = 0; j < _mapRenderer.CollisionLayer.Height; j++)
                {
                    if (!CombatArea.IsPointInCombatArea(new Vector2(i, j)))
                    {
                        _graph.Walls.Add(new Point(i, j));
                    }
                }
            }
        }

        public Point WorldToGridPosition(Vector2 worldPosition)
        {
            //subtract entity position to get position relative to this grid graph
            return _mapRenderer.TiledMap.WorldToTilePosition(worldPosition - Entity.Position);
        }

        public Vector2 GridToWorldPosition(Point gridPosition)
        {
            //subtract entity position to get position relative to this grid graph
            return _mapRenderer.TiledMap.TileToWorldPosition(gridPosition) + Entity.Position;
        }

        public bool IsPositionInWall(Vector2 worldPosition)
        {
            var gridPos = WorldToGridPosition(worldPosition);
            return _graph.Walls.Contains(gridPos);
        }

        /// <summary>
        /// given a start and end point in world space, return list of world space positions along grid between them
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public List<Vector2> FindPath(Vector2 startPoint, Vector2 endPoint)
        {
            List<Vector2> path = new List<Vector2>();

            var attempts = 0;
            while (attempts < 250)
            {
                attempts++;

                //determine basic path along grid
                var gridPath = _graph.Search(WorldToGridPosition(startPoint), WorldToGridPosition(endPoint));

                //if no path found, just return the end point
                if (gridPath == null)
                {
                    return new List<Vector2> { endPoint };
                }

                //remove any points that are in walls (this could happen if they are starting in a wall somehow)
                gridPath.RemoveAll(_graph.Walls.Contains);

                //if removing walls leaves no points, just return the end point
                if (gridPath.Count == 0)
                    return new List<Vector2> { endPoint };

                //modify path into list of Vector2 in world space (in center of tile)
                bool success = true;
                foreach (var pathItem in gridPath)
                {
                    var pos = GridToWorldPosition(pathItem) + new Vector2(8, 8);

                    //check that point isn't in a collider
                    if (Physics.OverlapCircle(pos, .5f, 1 << (int)PhysicsLayers.Environment) != null)
                    {
                        _graph.Walls.Add(pathItem);
                        path.Clear();
                        success = false;
                        break;
                    }

                    //check that this point is in a combat area
                    //if (!CombatArea.IsPointInCombatArea(pos))
                    //{
                    //    _graph.Walls.Add(pathItem);
                    //    path.Clear();
                    //    success = false;
                    //    break;
                    //}

                    path.Add(GridToWorldPosition(pathItem) + new Vector2(8, 8));
                }

                if (success)
                    break;
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

            return path;
        }
    }
}
