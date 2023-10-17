using Nez.Sprites;
using Nez;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PuppetRoguelite.Enums;
using Nez.AI.BehaviorTrees;
using Nez.Textures;
using PuppetRoguelite.Components.Characters.ChainBot;
using PuppetRoguelite.Tools;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Components.Characters.Player;

namespace PuppetRoguelite.Components.Characters.Boss
{
    public class Boss : Enemy, IUpdatable
    {
        //behavior tree
        BehaviorTree<Boss> _tree;

        //components
        public Mover Mover;
        public HealthComponent HealthComponent;
        public BoxCollider Collider;
        public SpriteAnimator Animator;
        public Hurtbox Hurtbox;
        public VelocityComponent VelocityComponent;
        public PathfindingComponent PathfindingComponent;

        #region SETUP

        public Boss(string mapId) : base(mapId)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            SetupBehaviorTree();
        }

        void AddComponents()
        {
            Mover = Entity.AddComponent(new Mover());

            HealthComponent = Entity.AddComponent(new HealthComponent(10, 10));
            HealthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);

            Collider = Entity.AddComponent(new BoxCollider());
            Flags.SetFlagExclusive(ref Collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);

            var hurtboxCollider = Entity.AddComponent(new BoxCollider());
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            Hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, 75f, new Vector2(1, 0)));

            PathfindingComponent = Entity.AddComponent(new PathfindingComponent(VelocityComponent, MapId));

            Animator = Entity.AddComponent(new SpriteAnimator());
        }

        void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<Boss>
                .Begin(this)
                    .Selector()
                    .EndComposite()
                .Build();
        }

        #endregion

        public void Update()
        {
            _tree.Tick();
        }

        #region Observers

        void OnDamageTaken(HealthComponent healthComponent)
        {

        }

        #endregion
    }
}
