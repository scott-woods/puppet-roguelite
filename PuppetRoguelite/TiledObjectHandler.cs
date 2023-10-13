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

namespace PuppetRoguelite
{
    public class TiledObjectHandler : SceneComponent
    {
        /// <summary>
        /// Process a tiled map, handling any Tiled objects
        /// </summary>
        /// <param name="map"></param>
        public void ProcessTiledMap(TiledMapRenderer tmxRenderer, string mapId)
        {
            var entPosition = tmxRenderer.Entity.Position;

            for (int i = 0; i < tmxRenderer.TiledMap.ObjectGroups.Count; i++)
            {
                var objGroup = tmxRenderer.TiledMap.ObjectGroups[i];
                objGroup.Visible = true;
            }

            foreach(var obj in tmxRenderer.TiledMap.ObjectGroups.SelectMany(g => g.Objects))
            {
                var type = Type.GetType("PuppetRoguelite.Components.TiledComponents." + obj.Type);
                var instance = Activator.CreateInstance(type, obj, mapId) as TiledComponent;
                var position = tmxRenderer.Entity.Position + new Vector2(obj.X, obj.Y);
                var entity = Scene.CreateEntity(obj.Name, position);
                entity.AddComponent(instance);
            }

            //foreach(var obj in tmxRenderer.TiledMap.ObjectGroups.SelectMany(g => g.Objects))
            //{
            //    switch (obj.Type)
            //    {
            //        case nameof(TiledObjectTypes.Trigger):
            //            HandleTrigger(obj, entPosition, mapId);
            //            break;
            //        case nameof(TiledObjectTypes.EnemySpawnPoint):
            //            HandleEnemySpawn(obj, entPosition, mapId);
            //            break;
            //        case nameof(TiledObjectTypes.Gate):
            //            HandleGate(obj, entPosition, mapId);
            //            break;
            //        case nameof(TiledObjectTypes.BossGate):
            //            HandleBossGate(obj, entPosition, mapId);
            //            break;
            //        case nameof(TiledObjectTypes.Chest):
            //            HandleChest(obj, entPosition, mapId);
            //            break;
            //        case nameof(TiledObjectTypes.ExitArea):
            //            HandleExitArea(obj, entPosition, mapId);
            //            break;
            //        case nameof(TiledObjectTypes.Shelf):
            //            HandleShelf(obj, entPosition, mapId);
            //            break;
            //    }
            //}
        }

        //public virtual void HandleTrigger(TmxObject obj, Vector2 mapPosition, string mapId)
        //{
        //    var ent = Scene.CreateEntity(obj.Name);
        //    ent.SetPosition(mapPosition + new Vector2((int)obj.X, (int)obj.Y));
        //    ent.AddComponent(new Trigger(obj, mapId));
        //}

        //public virtual void HandleEnemySpawn(TmxObject obj, Vector2 mapPosition, string mapId)
        //{
        //    var ent = Scene.CreateEntity(obj.Name);
        //    ent.SetPosition(mapPosition + new Vector2((int)obj.X, (int)obj.Y));
        //    var enemySpawnPoint = ent.AddComponent(new EnemySpawnPoint(obj, mapId));
        //}

        //public virtual void HandleGate(TmxObject obj, Vector2 mapPosition, string mapId)
        //{
        //    obj.Y -= 16;
        //    var ent = Scene.CreateEntity(obj.Name);
        //    ent.SetPosition(mapPosition + new Vector2((int)obj.X, (int)obj.Y));
        //    var gate = ent.AddComponent(new Gate(obj, mapId));
        //}
    }
}
