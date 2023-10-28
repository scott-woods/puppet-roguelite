using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Components.Characters.Spitter;
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
        List<Type> _enemyTypes = new List<Type>() { typeof(Spitter) };

        public EnemySpawnPoint(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public Enemy SpawnEnemy()
        {
            var type = _enemyTypes.RandomItem();

            ConstructorInfo info = type.GetConstructor(new Type[] { typeof(Entity) });
            if (info != null)
            {
                var ent = new PausableEntity(type.Name);
                Entity.Scene.AddEntity(ent);
                ent.SetPosition(Entity.Position);
                var enemy = ent.AddComponent((Enemy)info.Invoke(new object[] { MapEntity }));

                return enemy;
            }

            return null;
        }
    }
}
