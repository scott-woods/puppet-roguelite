using Nez;
using Nez.AI.Pathfinding;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    /// <summary>
    /// custom component for creating and handling tiled maps
    /// </summary>
    public class TiledMapComponent : Component
    {
        public string Id { get; set; }

        //components
        public TiledMapRenderer TiledMapRenderer;
        public GridGraphManager GridGraphManager;

        public TiledMapComponent(TiledMapRenderer tiledMapRenderer)
        {
            TiledMapRenderer = tiledMapRenderer;

            //generate new id
            Id = Guid.NewGuid().ToString();
        }

        public override void Initialize()
        {
            base.Initialize();

            //process tiled objects
            var tiledObjectHandler = Entity.Scene.GetOrCreateSceneComponent<TiledObjectHandler>();
            tiledObjectHandler.ProcessTiledMap(TiledMapRenderer, Id);

            //pathfinding
            var graph = new AstarGridGraph(TiledMapRenderer.CollisionLayer);
            GridGraphManager = Entity.AddComponent(new GridGraphManager(graph, TiledMapRenderer.TiledMap, Id));
        }
    }
}
