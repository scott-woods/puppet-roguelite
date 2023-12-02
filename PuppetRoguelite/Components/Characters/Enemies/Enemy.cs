﻿using Nez;
using Nez.AI.BehaviorTrees;
using PuppetRoguelite.Components.Characters.Enemies.ChainBot;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.GlobalManagers;

namespace PuppetRoguelite.Components.Characters.Enemies
{
    public abstract class Enemy<T> : EnemyBase, IUpdatable where T : Enemy<T>
    {
        //components
        public StatusComponent StatusComponent;

        public Status DefaultStatus = new Status(Status.StatusType.Normal, (int)StatusPriority.Normal);

        protected BehaviorTree<T> _tree;
        protected BehaviorTree<T> _subTree;

        public Enemy(Entity mapEntity)
        {
            MapEntity = mapEntity;
        }

        public override void Initialize()
        {
            base.Initialize();

            //components
            StatusComponent = Entity.AddComponent(new StatusComponent(DefaultStatus));

            //create sub tree
            _subTree = CreateSubTree();

            //create tree
            _tree = CreateBehaviorTree();
        }

        public virtual void Update()
        {
            _tree.Tick();
        }

        public virtual BehaviorTree<T> CreateBehaviorTree()
        {
            var tree = BehaviorTreeBuilder<T>.Begin(this as T)
                .Selector(AbortTypes.Self)
                    .ConditionalDecorator(c => c.StatusComponent.CurrentStatus.Type != Status.StatusType.Normal, true)
                        .Action(c => c.AbortActions())
                    .ConditionalDecorator(c =>
                    {
                        var gameStateManager = Game1.GameStateManager;
                        return gameStateManager.GameState != GameState.Combat;
                    }, true)
                        .Sequence()
                            .Action(c => c.AbortActions())
                            .Action(c => c.Idle())
                        .EndComposite()
                    .ConditionalDecorator(c =>
                    {
                        var gameStateManager = Game1.GameStateManager;
                        if (gameStateManager.GameState != GameState.Combat) return false;
                        if (c.StatusComponent.CurrentStatus.Type != Status.StatusType.Normal) return false;
                        return true;
                    }, true)
                        .SubTree(_subTree)
                .EndComposite()
                .Build();

            tree.UpdatePeriod = 0;

            return tree;
        }

        public abstract BehaviorTree<T> CreateSubTree();

        #region Tasks

        public abstract Nez.AI.BehaviorTrees.TaskStatus AbortActions();
        public abstract Nez.AI.BehaviorTrees.TaskStatus Idle();

        #endregion
    }
}