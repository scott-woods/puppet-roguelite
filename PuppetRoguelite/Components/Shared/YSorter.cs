using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class YSorter : Component, IUpdatable
    {
        IRenderable _renderable;
        OriginComponent _originComponent;
        List<RenderableComponent> _allRenderables;

        public YSorter(IRenderable renderable, OriginComponent originComponent)
        {
            _renderable = renderable;
            _originComponent = originComponent;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _allRenderables = Entity.GetComponents<RenderableComponent>();
        }

        public void Update()
        {
            if (_renderable != null)
            {
                //get dungenerator scene component
                var generator = Entity.Scene.GetSceneComponent<Dungenerator>();
                if (generator != null)
                {
                    //get node that this entity is in
                    var node = generator.GetNodeByWorldPosition(Entity.Position);
                    if (node != null)
                    {
                        //get renderers from this node's map entity
                        var renderers = node.MapEntity.GetComponents<TiledMapRenderer>();
                        foreach (var renderer in renderers)
                        {
                            //translate entity's bounds to be compatible with the renderer
                            var rect = _renderable.Bounds;
                            rect.X -= (node.Point.X * 16 * 24);
                            rect.Y -= (node.Point.Y * 16 * 24);

                            //translate origin component
                            var origin = _originComponent.Origin;
                            origin.X -= (node.Point.X * 16 * 24);
                            origin.Y -= (node.Point.Y * 16 * 24);

                            //try to get the renderer's above-details layer
                            if (renderer.TiledMap.TileLayers.TryGetValue("above-details", out var layer))
                            {
                                if (HandleLayer(layer, rect, origin))
                                {
                                    return;
                                }
                            }

                            if (renderer.TiledMap.TileLayers.TryGetValue("above-furniture", out var aboveFurnitureLayer))
                            {
                                if (HandleLayer(aboveFurnitureLayer, rect, origin))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                else //if no dungenerator, there should only be one map. no need for translation
                {
                    var tiledMapRenderers = Entity.Scene.FindComponentsOfType<TiledMapRenderer>();
                    foreach (var renderer in tiledMapRenderers)
                    {
                        var rect = _renderable.Bounds;
                        var origin = _originComponent.Origin;

                        if (renderer.TiledMap.TileLayers.TryGetValue("above-details", out var layer))
                        {
                            if (HandleLayer(layer, rect, origin))
                            {
                                return;
                            }
                        }

                        if (renderer.TiledMap.TileLayers.TryGetValue("above-furniture", out var aboveFurnitureLayer))
                        {
                            if (HandleLayer(aboveFurnitureLayer, rect, origin))
                            {
                                return;
                            }
                        }
                    }
                }

                foreach (var renderable in _allRenderables)
                {
                    renderable.RenderLayer = (int)RenderLayers.AboveDetails - (int)_originComponent.Origin.Y;
                }
            }
        }

        bool HandleLayer(TmxLayer layer, Rectangle rect, Vector2 origin)
        {
            //get tiles from this layer intersecting the renderable
            var tiles = layer.GetTilesIntersectingBounds(rect);
            foreach (var tile in tiles)
            {
                //an above tile is below the origin. render below above details
                if ((tile.Y * 16) + 16 > origin.Y)
                {
                    foreach (var renderable in _allRenderables)
                    {
                        renderable.RenderLayer = -(int)(_originComponent.Origin.Y);
                    }

                    return true;
                }
            }

            return false;
        }

        public void SetOriginComponent(OriginComponent originComponent)
        {
            _originComponent = originComponent;
        }
    }
}
