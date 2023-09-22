using Nez;
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
        Collider _hitbox;
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
            _hitbox = Entity.AddComponent(new CircleCollider());
            _hitbox.IsTrigger = true;

            //add damage component
            _damageComponent = Entity.AddComponent(new DamageComponent(_damage));
        }
    }
}
