using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.AI.FSM;
using Nez.AI.Pathfinding;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
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
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        PathfindingComponent _pathfinder;
        Collider _collider;

        //Player
        PlayerController _player;

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
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(8, 20));
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.EnemyHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1, new int[] { (int)PhysicsLayers.PlayerDamage }));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(10, 10));

            //pathfinding
            _pathfinder = Entity.AddComponent(new PathfindingComponent());

            //collider
            _collider = Entity.AddComponent(new BoxCollider(-4, 5, 8, 5));

            AddObservers();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _player = Entity.Scene.FindComponentOfType<PlayerController>();
        }

        public void Update()
        {
            BehaviorTree.Tick();
        }

        public void AddObservers()
        {

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
