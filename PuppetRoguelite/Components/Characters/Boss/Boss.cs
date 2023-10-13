using Nez.Sprites;
using Nez;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetRoguelite.Enums;

namespace PuppetRoguelite.Components.Characters.Boss
{
    public class Boss : Enemy
    {
        //components
        public Mover Mover;
        public SpriteAnimator Animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        public PathfindingComponent Pathfinder;
        Collider _collider;

        public Boss(string mapId) : base(mapId)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            Mover = Entity.AddComponent(new Mover());

            Animator = Entity.AddComponent(new SpriteAnimator());

            var hurtboxCollider = new BoxCollider();
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1, new int[] { (int)PhysicsLayers.PlayerDamage }));

            _healthComponent = Entity.AddComponent(new HealthComponent(10, 10));

            _collider = Entity.AddComponent(new BoxCollider());

            //Pathfinder = Entity.AddComponent(new PathfindingComponent(DungeonRoom.GridGraphManager));
        }
    }
}
