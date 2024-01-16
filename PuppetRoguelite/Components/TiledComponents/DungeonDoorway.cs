using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using Nez.UI;
using PuppetRoguelite.Entities;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class DungeonDoorway : TiledComponent
    {
        public bool HasConnection = false;
        public Vector2 PathfindingOffset;
        public string Direction;
        public DungeonRoom DungeonRoom;

        List<TiledMapRenderer> _mapRenderers = new List<TiledMapRenderer>();
        TmxMap _map;

        public DungeonDoorway(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            //get pathfinding offset
            if (TmxObject.Properties.TryGetValue("PathfindingOffset", out var pathfindingOffset))
            {
                var offsetValues = pathfindingOffset.Split(' ');
                PathfindingOffset = new Vector2(Convert.ToInt32(offsetValues[0]), Convert.ToInt32(offsetValues[1]));
            }

            //get direction exit is facing
            if (TmxObject.Properties.TryGetValue("Direction", out var direction))
            {
                Direction = direction;
            }

            //get parent dungeon room
            if (MapEntity.TryGetComponent<DungeonRoom>(out var dungeonRoom))
            {
                DungeonRoom = dungeonRoom;
                dungeonRoom.Doorways.Add(this);
            }

            CreateMap();

            Entity.SetPosition(new Microsoft.Xna.Framework.Vector2(base.Entity.Position.X - (_map.Width * _map.TileWidth) / 2, base.Entity.Position.Y - (_map.Height * _map.TileHeight) / 2));
        }

        public void SetOpen(bool open)
        {
            HasConnection = open;

            //remove previous renderers
            foreach (var renderer in _mapRenderers)
            {
                Entity.RemoveComponent(renderer);
            }
            _mapRenderers.Clear();

            CreateMap();
        }

        /// <summary>
        /// create the actual map entity. will be open if IsConnection is true, and closed if it is false
        /// </summary>
        void CreateMap()
        {
            if (DungeonRoom.Map.TmxMap.Properties.TryGetValue("Area", out var area))
            {
                var doorwayStatus = HasConnection ? "open" : "closed";
                var mapName = $"{area.ToLower()}_{Direction.ToLower()}_{doorwayStatus}";
                _map = base.Entity.Scene.Content.LoadTiledMap($@"Content\Tiled\Tilemaps\{area}\Doorways\{mapName}.tmx");

                //create main map renderer
                var mapRenderer = Entity.AddComponent(new TiledMapRenderer(_map, "Walls"));
                mapRenderer.SetLayersToRender(new[] { "Back", "Walls" });
                mapRenderer.RenderLayer = 10;
                Entity.AddComponent(new GridGraphManager(mapRenderer));
                Entity.AddComponent(new TiledObjectHandler(mapRenderer));
                Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

                //create above map renderer
                var tiledMapDetailsRenderer = Entity.AddComponent(new TiledMapRenderer(_map));
                var layersToRender = new List<string>();
                if (_map.Layers.Contains("Front"))
                    layersToRender.Add("Front");
                if (_map.Layers.Contains("AboveFront"))
                    layersToRender.Add("AboveFront");
                tiledMapDetailsRenderer.SetLayersToRender(layersToRender.ToArray());
                tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
                //tiledMapDetailsRenderer.Material = Material.StencilWrite();

                _mapRenderers.Add(mapRenderer);
                _mapRenderers.Add(tiledMapDetailsRenderer);
            }
        }
    }
}
