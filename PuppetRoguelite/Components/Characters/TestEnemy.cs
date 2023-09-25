using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.AI.FSM;
using Nez.AI.Pathfinding;
using Nez.Sprites;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetRoguelite.Components.Characters
{
    public class TestEnemy : Component, IUpdatable
    {
        public BehaviorTree<TestEnemy> BehaviorTree;

        //components
        Mover _mover;
        SpriteRenderer _spriteRenderer;
        Hitbox _hitbox;
        HealthComponent _healthComponent;
        InvincibilityComponent _invincibilityComponent;
        HurtComponent _hurtComponent;
        PathfindingComponent _pathfinder;
        Collider _collider;

        //Player
        Player _player;

        //Properties
        Vector2 _direction;
        float _moveSpeed = 75f;

        //misc
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();

        public override void Initialize()
        {
            base.Initialize();

            BehaviorTree = BehaviorTreeBuilder<TestEnemy>.Begin(this)
                .Selector()
                    .Sequence()
                        .Action(enemy => enemy.ChasePlayer())
                        .WaitAction(.25f)
                        .Action(enemy => enemy.AttackPlayer())
                        .WaitAction(.5f)
                    .EndComposite()
                .EndComposite()
                .Build();
            BehaviorTree.UpdatePeriod = 0;

            //Mover
            _mover = Entity.AddComponent(new Mover());

            //Sprite
            _spriteRenderer = Entity.AddComponent(new PrototypeSpriteRenderer(8, 20));

            //hitbox
            var hitboxCollider = Entity.AddComponent(new BoxCollider(8, 20));
            hitboxCollider.IsTrigger = true;
            hitboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyHitbox;
            _hitbox = Entity.AddComponent(new Hitbox(hitboxCollider));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(10));

            //Invincibility
            _invincibilityComponent = Entity.AddComponent(new InvincibilityComponent(1));

            //hurt
            _hurtComponent = Entity.AddComponent(new HurtComponent());

            //pathfinding
            _pathfinder = Entity.AddComponent(new PathfindingComponent());

            //collider
            _collider = Entity.AddComponent(new BoxCollider(-4, 5, 8, 5));

            AddObservers();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _player = Entity.Scene.FindComponentOfType<Player>();
        }

        public void Update()
        {
            BehaviorTree.Tick();
        }

        public void AddObservers()
        {
            _hitbox.Emitter.AddObserver(HitboxEventTypes.Hit, OnHitboxHit);
        }

        public void OnHitboxHit(Collider collider)
        {
            if (!_invincibilityComponent.IsInvincible)
            {
                if (collider.Entity.TryGetComponent<DamageComponent>(out var damageComponent))
                {
                    _hurtComponent.PlayHurtEffect();
                    _healthComponent.DecrementHealth(damageComponent.Damage);
                    _invincibilityComponent.Activate();
                }
            }
        }

        public TaskStatus ChasePlayer()
        {
            if (_player != null)
            {
                //check if finished
                if (_pathfinder.IsNavigationFinished())
                {
                    return TaskStatus.Success;
                }

                //if close enough to player, return success
                var distanceToPlayer = Vector2.Distance(Entity.Position, _player.Entity.Position);
                if (distanceToPlayer < 20)
                {
                    return TaskStatus.Success;
                }

                //get next path position
                //_pathfinder.CalculatePath(_player.Entity.Position);
                _pathfinder.SetTarget(_player.Entity.Position);
                var target = _pathfinder.GetNextPosition();

                //determine direction
                _direction = target - Entity.Position;
                _direction.Normalize();

                //move
                var movement = _direction * _moveSpeed * Time.DeltaTime;
                _mover.CalculateMovement(ref movement, out var result);
                _subPixelV2.Update(ref movement);
                _mover.ApplyMovement(movement);

                return TaskStatus.Running;
            }

            return TaskStatus.Failure;
        }

        public TaskStatus AttackPlayer()
        {
            _spriteRenderer.Color = Color.Green;
            Core.Schedule(.5f, (timer) => _spriteRenderer.Color = Color.White);
            return TaskStatus.Success;
        }
    }
}
