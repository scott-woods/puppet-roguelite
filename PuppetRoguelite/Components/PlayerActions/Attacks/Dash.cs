using Nez;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Attacks
{
    [PlayerActionInfo("Dash", 3)]
    public class Dash : PlayerAction
    {
        int _damage = 1;
        int[] _targetLayers = new int[] { (int)PhysicsLayers.EnemyHurtbox };

        Mover _mover;
        SpriteAnimator _animator;
        Hitbox _hitbox;
        Collider _hitboxCollider;

        List<Component> _componentsList = new List<Component>();

        public override void Initialize()
        {
            base.Initialize();

            _mover = Entity.AddComponent(new Mover());
            _componentsList.Add( _mover );
            
            _animator = Entity.AddComponent(new SpriteAnimator());
            _componentsList.Add(_animator );
            AddAnimations();

            _hitboxCollider = Entity.AddComponent(new BoxCollider());
            _componentsList.Add(_hitboxCollider);
            _hitbox = new Hitbox(_hitboxCollider, _damage, _targetLayers);
            _componentsList.Add(_hitbox);
        }

        void AddAnimations()
        {

        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            foreach(var component in _componentsList)
            {
                Entity.RemoveComponent(component);
            }
        }
    }
}
