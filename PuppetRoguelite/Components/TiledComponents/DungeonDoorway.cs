using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
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

        public DungeonDoorway(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if (TmxObject.Properties.TryGetValue("PathfindingOffset", out var pathfindingOffset))
            {
                var offsetValues = pathfindingOffset.Split(' ');
                PathfindingOffset = new Vector2(Convert.ToInt32(offsetValues[0]), Convert.ToInt32(offsetValues[1]));
            }

            if (TmxObject.Properties.TryGetValue("Direction", out var direction))
            {
                Direction = direction;

                if (MapEntity.TryGetComponent<DungeonRoom>(out var dungeonRoom))
                {
                    dungeonRoom.Doorways.Add(this);

                    if (dungeonRoom.Map.TmxMap.Properties.TryGetValue("Area", out var area))
                    {
                        var mapName = $"{area.ToLower()}_{direction.ToLower()}_open";
                        var map = base.Entity.Scene.Content.LoadTiledMap($@"Content\Tiled\Tilemaps\{area}\Doorways\{mapName}.tmx");

                        Entity.SetPosition(new Microsoft.Xna.Framework.Vector2(base.Entity.Position.X - (map.Width * map.TileWidth) / 2, base.Entity.Position.Y - (map.Height * map.TileHeight) / 2));

                        //create main map renderer
                        var mapRenderer = Entity.AddComponent(new TiledMapRenderer(map, "Walls"));
                        mapRenderer.SetLayersToRender(new[] { "Back", "Walls" });
                        mapRenderer.RenderLayer = 10;
                        Entity.AddComponent(new GridGraphManager(mapRenderer));
                        Entity.AddComponent(new TiledObjectHandler(mapRenderer));
                        Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, (int)PhysicsLayers.Environment);

                        //create above map renderer
                        var tiledMapDetailsRenderer = Entity.AddComponent(new TiledMapRenderer(map));
                        var layersToRender = new List<string>();
                        if (map.Layers.Contains("Front"))
                            layersToRender.Add("Front");
                        if (map.Layers.Contains("AboveFront"))
                            layersToRender.Add("AboveFront");
                        tiledMapDetailsRenderer.SetLayersToRender(layersToRender.ToArray());
                        tiledMapDetailsRenderer.RenderLayer = (int)RenderLayers.AboveDetails;
                        tiledMapDetailsRenderer.Material = Material.StencilWrite();
                    }
                }
            }
        }
    }
}
