using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class Dungenerator : SceneComponent
    {
        Vector2 _roomSize = new Vector2(24, 24);
        Vector2 _tileSize = new Vector2(16, 16);

        List<string> _leftDoorRooms = new List<string>()
        {
            Nez.Content.Tiled.Tilemaps.TBLR_1,
            Nez.Content.Tiled.Tilemaps.TBL_1,
            Nez.Content.Tiled.Tilemaps.TLR_1,
            Nez.Content.Tiled.Tilemaps.BLR_1,
            Nez.Content.Tiled.Tilemaps.TL_1,
            Nez.Content.Tiled.Tilemaps.BL_1,
            Nez.Content.Tiled.Tilemaps.LR_1,
            Nez.Content.Tiled.Tilemaps.L_1
        };

        List<string> _rightDoorRooms = new List<string>()
        {
            Nez.Content.Tiled.Tilemaps.TBLR_1,
            Nez.Content.Tiled.Tilemaps.TBR_1,
            Nez.Content.Tiled.Tilemaps.TLR_1,
            Nez.Content.Tiled.Tilemaps.BLR_1,
            Nez.Content.Tiled.Tilemaps.TR_1,
            Nez.Content.Tiled.Tilemaps.BR_1,
            Nez.Content.Tiled.Tilemaps.LR_1,
            Nez.Content.Tiled.Tilemaps.R_1
        };

        List<string> _topDoorRooms = new List<string>()
        {
            Nez.Content.Tiled.Tilemaps.TBLR_1,
            Nez.Content.Tiled.Tilemaps.TBL_1,
            Nez.Content.Tiled.Tilemaps.TBR_1,
            Nez.Content.Tiled.Tilemaps.TLR_1,
            Nez.Content.Tiled.Tilemaps.TB_1,
            Nez.Content.Tiled.Tilemaps.TL_1,
            Nez.Content.Tiled.Tilemaps.TR_1,
            Nez.Content.Tiled.Tilemaps.T_1
        };

        List<string> _bottomDoorRooms = new List<string>()
        {
            Nez.Content.Tiled.Tilemaps.TBLR_1,
            Nez.Content.Tiled.Tilemaps.TBL_1,
            Nez.Content.Tiled.Tilemaps.TBR_1,
            Nez.Content.Tiled.Tilemaps.BLR_1,
            Nez.Content.Tiled.Tilemaps.TB_1,
            Nez.Content.Tiled.Tilemaps.BL_1,
            Nez.Content.Tiled.Tilemaps.BR_1,
            Nez.Content.Tiled.Tilemaps.B_1
        };

        AstarGridGraph _graph;

        Point _hubPoint, _keyPoint, _preBossPoint, _bossPoint;

        Dictionary<Point, Entity> _roomDictionary = new Dictionary<Point, Entity>();

        public void Generate()
        {
            CreateGraph();
            CreateMaps();
        }

        public void CreateGraph()
        {
            _graph = new AstarGridGraph(5, 5);

            //random position for starting location
            _hubPoint = new Point(Nez.Random.Range(0, 5), Nez.Random.Range(0, 5));

            //pre boss room a certain distance from starting room
            bool bossRoomPlaced = false;
            while (!bossRoomPlaced)
            {
                int x = Nez.Random.Range(1, 5); //pre boss room is entered from its left side
                int y = Nez.Random.Range(1, 5); //boss room is always entered from bottom, so need space above pre boss
                Point potentialPos = new Point(x, y);

                var path = _graph.Search(_hubPoint, potentialPos);

                if (path.Count >= 4)
                {
                    _preBossPoint = potentialPos;
                    bossRoomPlaced = true;
                }
            }

            //boss above pre boss
            _bossPoint = new Point(_preBossPoint.X, _preBossPoint.Y - 1);

            //key
            bool keyRoomPlaced = false;
            while (!keyRoomPlaced)
            {
                int x = Nez.Random.NextInt(5);
                int y = Nez.Random.NextInt(5);
                Point potentialKeyRoomPoint = new Point(x, y);

                if (new List<Point>() { _hubPoint, _preBossPoint, _bossPoint }.Contains(potentialKeyRoomPoint))
                {
                    continue;
                }

                var path = _graph.Search(_hubPoint, potentialKeyRoomPoint);

                if (path.Count >= 3)
                {
                    _keyPoint = potentialKeyRoomPoint;
                    keyRoomPlaced = true;
                }
            }
        }

        public void CreateMaps()
        {
            //hub room
            var hubRoomEntity = new PausableEntity($"map-hub-room");
            var hubPos = new Vector2(_hubPoint.X * _roomSize.X * _tileSize.X, _hubPoint.Y * _roomSize.Y * _tileSize.Y);
            hubRoomEntity.SetPosition(hubPos);
            Scene.AddEntity(hubRoomEntity);
            var hubRoomMap = Scene.Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.TBLR_1);
            var hubRoomRenderer = hubRoomEntity.AddComponent(new TiledMapRenderer(hubRoomMap, "collision"));
            hubRoomRenderer.SetLayersToRender(new[] { "floor", "details" });
            hubRoomRenderer.RenderLayer = 10;
            _roomDictionary.Add(_hubPoint, hubRoomEntity);

            //key room
            var keyRoomEntity = new PausableEntity($"map-key-room");
            var keyPos = new Vector2(_keyPoint.X * _roomSize.X * _tileSize.X, _keyPoint.Y * _roomSize.Y * _tileSize.Y);
            keyRoomEntity.SetPosition(keyPos);
            Scene.AddEntity(keyRoomEntity);
            var keyRoomMap = Scene.Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.R_1);
            var keyRoomRenderer = keyRoomEntity.AddComponent(new TiledMapRenderer(keyRoomMap, "collision"));
            keyRoomRenderer.SetLayersToRender(new[] { "floor", "details" });
            keyRoomRenderer.RenderLayer = 10;
            _roomDictionary.Add(_keyPoint, keyRoomEntity);

            //add walls to graph
            //var keyRoomLeft = new Point(_keyPoint.X - 1, _keyPoint.Y);
            //_graph.Walls.Add(keyRoomLeft);
            //var keyRoomAbove = new Point(_keyPoint.X, _keyPoint.Y - 1);
            //_graph.Walls.Add(keyRoomAbove);
            //var keyRoomBelow = new Point(_keyPoint.X, _keyPoint.Y + 1);
            //_graph.Walls.Add(keyRoomBelow);

            //pre boss
            var preBossRoomEntity = new PausableEntity($"map-preBoss-room");
            var preBossPos = new Vector2(_preBossPoint.X * _roomSize.X * _tileSize.X, _preBossPoint.Y * _roomSize.Y * _tileSize.Y);
            preBossRoomEntity.SetPosition(preBossPos);
            Scene.AddEntity(preBossRoomEntity);
            var preBossRoomMap = Scene.Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.TL_1);
            var preBossRoomRenderer = preBossRoomEntity.AddComponent(new TiledMapRenderer(preBossRoomMap, "collision"));
            preBossRoomRenderer.SetLayersToRender(new[] { "floor", "details" });
            preBossRoomRenderer.RenderLayer = 10;
            _roomDictionary.Add(_preBossPoint, preBossRoomEntity);

            //add walls to graph
            //_graph.Walls.Add(new Point(_preBossPoint.X + 1, _preBossPoint.Y));
            //_graph.Walls.Add(new Point(_preBossPoint.X, _preBossPoint.Y + 1));

            //boss
            var bossRoomEntity = new PausableEntity($"map-boss-room");
            var bossPos = new Vector2(_bossPoint.X * _roomSize.X * _tileSize.X, _bossPoint.Y * _roomSize.Y * _tileSize.Y);
            bossRoomEntity.SetPosition(bossPos);
            Scene.AddEntity(bossRoomEntity);
            var bossRoomMap = Scene.Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.B_1);
            var bossRoomRenderer = bossRoomEntity.AddComponent(new TiledMapRenderer(bossRoomMap, "collision"));
            bossRoomRenderer.SetLayersToRender(new[] { "floor", "details" });
            bossRoomRenderer.RenderLayer = 10;
            _roomDictionary.Add(_bossPoint, bossRoomEntity);

            //add walls
            //_graph.Walls.Add(new Point(_bossPoint.X + 1, _bossPoint.Y));
            //_graph.Walls.Add(new Point(_bossPoint.X - 1, _bossPoint.Y));
            //_graph.Walls.Add(new Point(_bossPoint.X, _bossPoint.Y - 1));

            //path from hub to key
            var hubToKey = _graph.Search(_hubPoint, _keyPoint);
            for (int i = 1; i <  hubToKey.Count - 1; i++)
            {
                var point = hubToKey[i];

                if (!_roomDictionary.ContainsKey(point))
                {
                    var tiledEntity = new PausableEntity($"map-{point.X}-{point.Y}");
                    var pos = new Vector2(point.X * _roomSize.X * _tileSize.X, point.Y * _roomSize.Y * _tileSize.Y);
                    tiledEntity.SetPosition(pos);
                    Scene.AddEntity(tiledEntity);
                    var map = Scene.Content.LoadTiledMap(Nez.Content.Tiled.Tilemaps.TBLR_1);
                    var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "collision"));
                    tiledMapRenderer.SetLayersToRender(new[] { "floor", "details" });
                    tiledMapRenderer.RenderLayer = 10;

                    _roomDictionary.Add(point, tiledEntity);
                }
                else
                {
                    //TODO
                }
            }

            PlayerController.Instance.Entity.SetPosition(hubPos + new Vector2((_roomSize.X * _tileSize.X) / 2, (_roomSize.Y * _tileSize.Y) / 2));
        }
    }
}
