using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.EnemyActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System.Collections.Generic;
using System.Linq;
using TaskStatus = Nez.AI.BehaviorTrees.TaskStatus;

namespace PuppetRoguelite.Components.Characters.ChainBot
{
    public class ChainBot : Enemy, IUpdatable
    {
        //stats
        float _moveSpeed = 75f;
        int _hp = 3;
        int _maxHp = 3;

        //behavior tree
        BehaviorTree<ChainBot> _tree;

        //actions
        EnemyAction _currentAction;
        ChainBotMelee _chainBotMelee;

        //components
        public Mover Mover;
        public SpriteAnimator Animator;
        Hurtbox _hurtbox;
        HealthComponent _healthComponent;
        public PathfindingComponent Pathfinder;
        Collider _collider;
        YSorter _ySorter;
        public VelocityComponent VelocityComponent;
        public CombatComponent CombatComponent;

        //properties
        public bool IsDamaged = false;

        #region SETUP

        public ChainBot(string mapId) : base(mapId)
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
            //Mover
            Mover = Entity.AddComponent(new Mover());

            //hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(8, 18));
            hurtboxCollider.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtboxCollider.PhysicsLayer, (int)PhysicsLayers.EnemyHurtbox);
            Flags.SetFlagExclusive(ref hurtboxCollider.CollidesWithLayers, (int)PhysicsLayers.PlayerHitbox);
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            //health
            _healthComponent = Entity.AddComponent(new HealthComponent(_hp, _maxHp));
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.DamageTaken, OnDamageTaken);
            _healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnHealthDepleted);

            //velocity
            VelocityComponent = Entity.AddComponent(new VelocityComponent(Mover, _moveSpeed, new Vector2(1, 0)));

            //pathfinding
            Pathfinder = Entity.AddComponent(new PathfindingComponent(VelocityComponent, MapId));

            //collider
            _collider = Entity.AddComponent(new BoxCollider(0, 5, 8, 5));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.EnemyCollider);
            Flags.SetFlagExclusive(ref _collider.CollidesWithLayers, (int)PhysicsLayers.Environment);

            //animator
            Animator = Entity.AddComponent(new SpriteAnimator());
            AddAnimations();

            //y sorter
            _ySorter = Entity.AddComponent(new YSorter(Animator, 12));

            //actions
            _chainBotMelee = Entity.AddComponent(new ChainBotMelee(this));
            _chainBotMelee.SetEnabled(false);

            //combat
            CombatComponent = Entity.AddComponent(new CombatComponent());
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
            _tree = BehaviorTreeBuilder<ChainBot>.Begin(this)
                .Selector(AbortTypes.Self)
                    .ConditionalDecorator(c => !c.IsDamaged, true)
                    .Sequence()
                        .Selector()
                            .Sequence()
                                .Conditional(c => c.CombatComponent.IsInCombat)
                                .ParallelSelector()
                                    .Action(c => c.Move())
                                    .Action(c => EnemyTasks.ChasePlayer(c.Pathfinder, c.VelocityComponent))
                                .EndComposite()
                                .Action(c => c.MeleeAttack())
                            .EndComposite()
                            .Sequence()
                                .Conditional(c => !c.CombatComponent.IsInCombat)
                                .Action(c => c.Idle())
                            .EndComposite()
                        .EndComposite()
                    .EndComposite()
                .EndComposite()
                .Build();

            _tree.UpdatePeriod = 0;
        }

        #endregion

        public void Update()
        {
            _tree.Tick();
        }

        #region TASKS

        TaskStatus MeleeAttack()
        {
            if (!_chainBotMelee.Enabled)
            {
                  _chainBotMelee.SetEnabled(true);
                _currentAction = _chainBotMelee;
            }

            if (_chainBotMelee.Execute())
            {
                _chainBotMelee.SetEnabled(false);
                _currentAction = null;
                return TaskStatus.Success;
            }
            else return TaskStatus.Running;
        }

        TaskStatus Idle()
        {
            var animation = VelocityComponent.Direction.X >= 0 ? "IdleRight" : "IdleLeft";

            if (!Animator.IsAnimationActive(animation))
            {
                Animator.Play(animation);
            }

            return TaskStatus.Running;
        }

        TaskStatus Move()
        {
            var animation = VelocityComponent.Direction.X >= 0 ? "RunRight" : "RunLeft";

            if (!Animator.IsAnimationActive(animation))
            {
                Animator.Play(animation);
            }
            
            return TaskStatus.Running;
        }

        #endregion

        #region OBSERVERS

        void OnDamageTaken(HealthComponent healthComponent)
        {
            if (_currentAction != null)
            {
                _currentAction.SetEnabled(false);
                _currentAction = null;
            }

            var hurtAnimation = VelocityComponent.Direction.X >= 0 ? "HurtRight" : "HurtLeft";
            int plays = 0;
            Animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            void handler(string obj)
            {
                plays += 1;
                if (plays >= 4)
                {
                    Animator.Stop();
                    Animator.OnAnimationCompletedEvent -= handler;
                    IsDamaged = false;
                }
                else
                {
                    Animator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
                }
            }
            Animator.OnAnimationCompletedEvent += handler;
            IsDamaged = true;
        }

        void OnHealthDepleted(HealthComponent healthComponent)
        {
            Entity.Destroy();
        }

        #endregion
    }
}
