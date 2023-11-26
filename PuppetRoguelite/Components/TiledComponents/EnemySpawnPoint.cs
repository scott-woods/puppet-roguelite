using Nez;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Enemies;
using PuppetRoguelite.Entities;
using System;
using System.Reflection;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class EnemySpawnPoint : TiledComponent
    {
        public EnemySpawnPoint(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public EnemyBase SpawnEnemy(Type type)
        {
            ConstructorInfo info = type.GetConstructor(new Type[] { typeof(Entity) });
            if (info != null)
            {
                var ent = new PausableEntity(type.Name);
                Entity.Scene.AddEntity(ent);
                ent.SetPosition(Entity.Position);
                var enemy = ent.AddComponent((EnemyBase)info.Invoke(new object[] { MapEntity }));

                return enemy;
            }

            return null;
        }
    }
}
