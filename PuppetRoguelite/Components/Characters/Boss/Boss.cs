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

        //actions
        List<IEnemyAction> _actions = new List<IEnemyAction>();
        IEnemyAction _nextAction;

        Entity _meleeEntity;

        //components
        Mover _mover;
        public SpriteAnimator Animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        //public PathfindingComponent Pathfinder;
        Collider _collider;
        YSorter _ySorter;
        //Chaser _chaser;
        NewPathfindingComponent _pathfinder;
        VelocityComponent _velocityComponent;

        Hitbox _melee;

        //properties
        public float MoveSpeed = 75f;
        bool _isHurt = false;
        bool _isActive = true;

        //misc
        public SubpixelVector2 SubPixelV2 = new SubpixelVector2();
        public Vector2 Direction = Vector2.One;

        #region SETUP

        public Boss(string mapId) : base(mapId)
        {

        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            AddComponents();
            SetupBehaviorTree();

            _healthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);
        }

        void AddComponents()
        {
            //Mover
            _mover = Entity.AddComponent(new Mover());

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(1, 1));

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(0, -9, 8, 18));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            _hurtbox = Entity.AddComponent(new Hurtbox(_collider, 1));

            //velocity component
            _velocityComponent = Entity.AddComponent(new VelocityComponent(_mover, MoveSpeed, new Vector2(0, 1)));

            //pathfinding
            //Pathfinder = Entity.AddComponent(new PathfindingComponent(MapId));
            _pathfinder = Entity.AddComponent(new NewPathfindingComponent(_velocityComponent, MapId));

            //collider
            _collider = Entity.AddComponent(new BoxCollider(0, 5, 8, 5));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Environment);

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, 12));

            //chaser
            //_chaser = Entity.AddComponent(new Chaser(PlayerController.Instance.Entity, _mover, Pathfinder));
        }

        void AddAnimations()
        {
            //Idle
            var idleTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Idle);
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 126, 39);
            Animator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArray(idleSprites, new List<int> { 1, 3, 5, 7, 9 }));
            Animator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArray(idleSprites, new List<int> { 0, 2, 4, 6, 8 }));

            //Run
            var runTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Run);
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 126, 39);
            var leftSprites = runSprites.Where((sprite, index) => index % 2 != 0);
            var rightSprites = runSprites.Where((sprite, index) => index % 2 == 0);
            Animator.AddAnimation("RunLeft", leftSprites.ToArray());
            Animator.AddAnimation("RunRight", rightSprites.ToArray());

            //Attack
            var attackTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Attack);
            var attackSprites = Sprite.SpritesFromAtlas(attackTexture, 126, 39);
            Animator.AddAnimation("AttackLeft", attackSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            Animator.AddAnimation("AttackRight", attackSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //transition to charge
            var transitionTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Transitiontocharge);
            var transitionSprites = Sprite.SpritesFromAtlas(transitionTexture, 126, 39);
            Animator.AddAnimation("TransitionLeft", transitionSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            Animator.AddAnimation("TransitionRight", transitionSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //charge
            var chargeTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Charge);
            var chargeSprites = Sprite.SpritesFromAtlas(chargeTexture, 126, 39);
            Animator.AddAnimation("ChargeLeft", chargeSprites.Where((sprite, index) => index % 2 != 0).ToArray());
            Animator.AddAnimation("ChargeRight", chargeSprites.Where((sprite, index) => index % 2 == 0).ToArray());

            //hit
            var hitTexture = Entity.Scene.Content.LoadTexture(Content.Textures.Characters.ChainBot.Hit);
            var hitSprites = Sprite.SpritesFromAtlas(hitTexture, 126, 39);
            Animator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 1, 3 }));
            Animator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArray(hitSprites, new List<int>() { 0, 2 }));
        }

        public void SetupBehaviorTree()
        {
            _tree = BehaviorTreeBuilder<Boss>.Begin(this)
                .Selector()
                    .Sequence(AbortTypes.LowerPriority) //idle if not active
                        .Conditional(b => !b._isActive)
                        .Action(b => b.Idle())
                    .EndComposite()
                    .Sequence(AbortTypes.LowerPriority) //cancel action if hurt
                        .Conditional(b => b._isHurt)
                        .Action(b => b.CancelAction())
                    .EndComposite()
                    .Selector(AbortTypes.LowerPriority)
                        .Sequence(AbortTypes.LowerPriority)
                            .Action(b => b.ChasePlayer())
                            //.Action(b => b.Attack())
                            .Action(b => b.Idle())
                            .WaitAction(.5f)
                        .EndComposite()
                    .EndComposite()
                    //.Sequence(AbortTypes.LowerPriority)
                    //    .Conditional(e => !_isHurt)
                    //    .Action(enemy => SelectNextAction())
                    //    .Action(enemy => ExecuteAction())
                    //    //.SubTree(_nextAction.GetBehaviorTree(this))
                    //    .Action(enemy => Idle())
                    //    .WaitAction(.5f)
                    //.EndComposite()
                .EndComposite()
                .Build();
            _tree.UpdatePeriod = 0;
        }

        #endregion

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void Update()
        {
            _tree.Tick();
        }

        TaskStatus ChasePlayer()
        {
            var reachedTarget = _pathfinder.FollowPath(PlayerController.Instance.Entity.Position, true);
            if (!reachedTarget)
            {
                _velocityComponent.Move();
                return TaskStatus.Running;
            }
            else
            {
                return TaskStatus.Success;
            }
        }

        public TaskStatus SelectNextAction()
        {
            _nextAction = _actions.RandomItem();
            _nextAction.Start();
            return TaskStatus.Success;
        }

        public TaskStatus ExecuteAction()
        {
            if (_nextAction.IsCompleted)
            {
                _nextAction.Reset();
                return TaskStatus.Success;
            }
            else
            {
                var tree = _nextAction.GetBehaviorTree();
                tree.Tick();
                return TaskStatus.Running;
            }
        }

        public TaskStatus CancelAction()
        {
            if (_nextAction != null)
            {
                _nextAction.Reset();
            }

            return TaskStatus.Success;
        }

        public TaskStatus Idle()
        {
            var animation = Direction.X >= 0 ? "IdleRight" : "IdleLeft";
            if (!Animator.IsAnimationActive(animation))
            {
                Animator.Play(animation);
            }
            
            return TaskStatus.Success;
        }

        void OnDamageTaken(HealthComponent healthComponent)
        {
            var hurtAnimation = Direction.X >= 0 ? "HurtRight" : "HurtLeft";
            //Animator.Speed = Animator.Speed / 2;
            int plays = 0;
            Animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                plays += 1;
                if (plays >= 4)
                {
                    Animator.Stop();
                    Animator.OnAnimationCompletedEvent -= handler;
                    //Animator.Speed = Animator.Speed * 2;
                    _isHurt = false;
                }
                else
                {
                    Animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
                }
            }
            Animator.OnAnimationCompletedEvent += handler;
            _isHurt = true;
        }

        void OnHealthDepleted(HealthComponent healthComponent)
        {
            Entity.Destroy();
        }
    }
}
