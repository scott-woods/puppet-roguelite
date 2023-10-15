using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.SceneComponents
{
    /// <summary>
    /// Helper to create entities based off of Tiled objects
    /// </summary>
    public class TiledObjectHandler : SceneComponent
    {
        /// <summary>
        /// Process a tiled map, handling any Tiled objects
        /// </summary>
        /// <param name="map"></param>
        public void ProcessTiledMap(TiledMapRenderer tmxRenderer, string mapId)
        {
            var entPosition = tmxRenderer.Entity.Position;

            foreach (var obj in tmxRenderer.TiledMap.ObjectGroups.SelectMany(g => g.Objects))
            {
                var type = Type.GetType("PuppetRoguelite.Components.TiledComponents." + obj.Type);
                var instance = Activator.CreateInstance(type, obj, mapId) as TiledComponent;

                var position = new Vector2();
                switch (obj.ObjectType)
                {
                    case TmxObjectType.Basic:
                    case TmxObjectType.Polygon:
                    case TmxObjectType.Ellipse:
                    case TmxObjectType.Tile:
                        position = tmxRenderer.Entity.Position + new Vector2(obj.X + obj.Width / 2, obj.Y + obj.Height / 2);
                        break;
                    default:
                        position = tmxRenderer.Entity.Position + new Vector2(obj.X, obj.Y);
                        break;

                }

                var entity = Scene.CreateEntity(obj.Name, position);
                entity.AddComponent(instance);
            }
        }
    }
}
