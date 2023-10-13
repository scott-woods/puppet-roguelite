using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class EnemySpawnPoint : TiledComponent
    {
        List<Type> _enemyTypes = new List<Type>() { typeof(ChainBot) };

        public EnemySpawnPoint(TmxObject tmxObject, string mapId) : base(tmxObject, mapId)
        {

        }

        public Enemy SpawnEnemy()
        {
            var type = _enemyTypes.RandomItem();

            ConstructorInfo info = type.GetConstructor(new Type[] { typeof(string) });
            if (info != null)
            {
                var ent = new PausableEntity(type.Name);
                Entity.Scene.AddEntity(ent);
                ent.SetPosition(Entity.Position);
                var enemy = ent.AddComponent((Enemy)info.Invoke(new object[] { MapId }));

                return enemy;
            }

            return null;
        }

        //public Enemy SpawnEnemy(List<Type> enemyTypes, DungeonRoom room)
        //{
        //    var type = _enemyTypes.RandomItem();

        //    ConstructorInfo info = type.GetConstructor(new Type[] {typeof(DungeonRoom)});
        //    if (info != null)
        //    {
        //        var ent = new PausableEntity(type.Name);
        //        Entity.Scene.AddEntity(ent);
        //        var enemy = ent.AddComponent((Enemy)info.Invoke(new object[] {room}));
        //        ent.SetPosition(Entity.Position);

        //        return enemy;
        //    }

        //    return null;
        //}
    }
}
