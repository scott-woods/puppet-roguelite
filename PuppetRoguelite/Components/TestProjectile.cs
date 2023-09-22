using Nez;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class TestProjectile : Component
    {
        //Components
        Collider _collider;
        PrototypeSpriteRenderer _spriteRenderer;
        DamageComponent _damageComponent;

        //properties
        int _damage = 1;

        public override void Initialize()
        {
            base.Initialize();

            //add sprite
            _spriteRenderer = Entity.AddComponent(new PrototypeSpriteRenderer(16, 16));

            //setup hitbox
            _collider = Entity.AddComponent(new CircleCollider());
            _collider.IsTrigger = true;
            _collider.PhysicsLayer = (int)PhysicsLayers.Damage;

            //add damage component
            _damageComponent = Entity.AddComponent(new DamageComponent(_damage));
        }
    }
}
