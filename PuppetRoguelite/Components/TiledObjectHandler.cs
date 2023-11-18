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

namespace PuppetRoguelite.Components
{
    /// <summary>
    /// Helper to create entities based off of Tiled objects
    /// </summary>
    public class TiledObjectHandler : Component
    {
        TiledMapRenderer _mapRenderer;

        public TiledObjectHandler(TiledMapRenderer mapRenderer)
        {
            _mapRenderer = mapRenderer;
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var obj in _mapRenderer.TiledMap.ObjectGroups.SelectMany(g => g.Objects))
            {
                if (string.IsNullOrWhiteSpace(obj.Type)) return;
                var type = Type.GetType("PuppetRoguelite.Components.TiledComponents." + obj.Type);
                var instance = Activator.CreateInstance(type, obj, Entity) as TiledComponent;

                var position = new Vector2();
                switch (obj.ObjectType)
                {
                    case TmxObjectType.Basic:
                    case TmxObjectType.Ellipse:
                    case TmxObjectType.Tile:
                        position = Entity.Position + new Vector2(obj.X + obj.Width / 2, obj.Y + obj.Height / 2);
                        break;
                    default:
                        position = Entity.Position + new Vector2(obj.X, obj.Y);
                        break;

                }

                var entity = Entity.Scene.CreateEntity(obj.Name, position);
                entity.AddComponent(instance);
            }
        }
    }
}
