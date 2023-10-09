using Nez;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PuppetRoguelite.Components
{
    public class EnemySpawnPoint : Component
    {
        public Enemy SpawnEnemy(List<Type> enemyTypes, DungeonRoom room)
        {
            var type = enemyTypes.RandomItem();

            ConstructorInfo info = type.GetConstructor(new Type[] {typeof(DungeonRoom)});
            if (info != null)
            {
                var ent = new PausableEntity(type.Name);
                Entity.Scene.AddEntity(ent);
                var enemy = ent.AddComponent((Enemy)info.Invoke(new object[] {room}));
                ent.SetPosition(Entity.Position);

                return enemy;
            }

            return null;
        }
    }
}
