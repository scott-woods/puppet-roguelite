using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class YSorter : Component, IUpdatable
    {
        RenderableComponent _renderable;
        OriginComponent _originComponent;
        List<RenderableComponent> _allRenderables;

        public YSorter(RenderableComponent renderable, OriginComponent originComponent, List<RenderableComponent> allRenderables = null)
        {
            _renderable = renderable;
            _originComponent = originComponent;
            _allRenderables = allRenderables == null ? new List<RenderableComponent>() : allRenderables;

            if (!_allRenderables.Contains(_renderable))
                _allRenderables.Add(_renderable);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            var renderables = Entity.GetComponents<RenderableComponent>();
            //_allRenderables = renderables.Where(r => r is not SpriteMime).ToList();
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
                            //var rect = _renderable.Bounds;
                            //rect.X -= (node.Point.X * 16 * 24);
                            //rect.Y -= (node.Point.Y * 16 * 24);

                            //translate origin component
                            var origin = _originComponent.Origin;
                            origin.X -= (node.Point.X * 16 * 24);
                            origin.Y -= (node.Point.Y * 16 * 24);

                            var rect = new Rectangle((int)origin.X, (int)origin.Y, 1, 1);

                            //try to get the renderer's Front layer
                            if (renderer.TiledMap.TileLayers.TryGetValue("Front", out var frontLayer))
                            {
                                if (HandleLayer(frontLayer, rect, origin))
                                {
                                    return;
                                }
                            }

                            if (renderer.TiledMap.TileLayers.TryGetValue("AboveFront", out var aboveFrontLayer))
                            {
                                if (HandleLayer(aboveFrontLayer, rect, origin))
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
                        //var rect = _renderable.Bounds;
                        var origin = _originComponent.Origin;
                        var rect = new Rectangle((int)origin.X, (int)origin.Y, 1, 1);

                        if (renderer.TiledMap.TileLayers.TryGetValue("Front", out var frontLayer))
                        {
                            if (HandleLayer(frontLayer, rect, origin))
                            {
                                return;
                            }
                        }

                        if (renderer.TiledMap.TileLayers.TryGetValue("AboveFront", out var aboveFrontLayer))
                        {
                            if (HandleLayer(aboveFrontLayer, rect, origin))
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
