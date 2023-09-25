using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters
{
    public class Player : Component, IUpdatable
    {
        internal PlayerStateMachine StateMachine;

        //input
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;

        //stats
        float _moveSpeed = 100f;

        //components
        Mover _mover;
        SpriteAnimator _spriteAnimator;
        Collider _collider;
        HealthComponent _healthComponent;
        Hurtbox _hurtbox;
        HurtComponent _hurtComponent;
        InvincibilityComponent _invincibilityComponent;
        HitHandler _hitHandler;

        //misc
        Vector2 _direction = new Vector2(0, 1);
        SubpixelVector2 _subPixelV2 = new SubpixelVector2();

        public Player()
        {
            StateMachine = new PlayerStateMachine(this, new PlayerIdle());
        }

        public override void Initialize()
        {
            base.Initialize();

            AddComponents();
            AddObservers();
            SetupSprites();
            SetupInput();
        }

        public void AddComponents()
        {
            //add sprites
            _spriteAnimator = Entity.AddComponent(new SpriteAnimator());

            //add mover
            _mover = Entity.AddComponent(new Mover());

            //Add hurtbox
            var hurtboxCollider = Entity.AddComponent(new BoxCollider(10, 20));
            hurtboxCollider.IsTrigger = true;
            hurtboxCollider.PhysicsLayer = (int)PhysicsLayers.PlayerHurtbox;
            _hurtbox = Entity.AddComponent(new Hurtbox(hurtboxCollider, 1));

            //add collision box
            _collider = Entity.AddComponent(new BoxCollider(-5, 5, 10, 8));

            //Add health component
            _healthComponent = Entity.AddComponent(new HealthComponent(10));

            //Add hurt component
            _hurtComponent = Entity.AddComponent(new HurtComponent());

            //Add invincibility component
            _invincibilityComponent = Entity.AddComponent(new InvincibilityComponent(1));

            //hit handler
            _hitHandler = Entity.AddComponent(new HitHandler());
        }

        public void AddObservers()
        {
            //_hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHitboxHit);
            _hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, _hitHandler.OnHurtboxHit);

            _hitHandler.Emitter.AddObserver(HitHandlerEventType.Hurt, OnHurt);
            _hitHandler.Emitter.AddObserver(HitHandlerEventType.Killed, OnKilled);
        }

        public void OnHurt()
        {
            var hurtAnimation = _direction.X >= 0 ? "HurtRight" : "HurtLeft";
            _spriteAnimator.Play(hurtAnimation, SpriteAnimator.LoopMode.Once);
            StateMachine.ChangeState<PlayerHurt>();
        }

        public void OnKilled()
        {
            var deathAnimation = _direction.X >= 0 ? "DeathRight" : "DeathLeft";
            _spriteAnimator.Play(deathAnimation, SpriteAnimator.LoopMode.Once);
            StateMachine.ChangeState<PlayerDying>();
        }

        public void CheckForDeathCompletion()
        {
            if (new[] { "DeathRight", "DeathLeft" }.Contains(_spriteAnimator.CurrentAnimationName))
            {
                if (_spriteAnimator.AnimationState == SpriteAnimator.State.Completed)
                {
                    Game1.Exit();
                }
            }
            else
            {
                Game1.Exit();
            }
        }

        public void Update()
        {
            StateMachine.Update(Time.DeltaTime);
        }

        void SetupInput()
        {
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _xAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));

            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _yAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down));
        }

        void SetupSprites()
        {
            //idle
            var idleTexture = Entity.Scene.Content.LoadTexture("Content/Sprites/Player/hooded_knight_idle.png");
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            _spriteAnimator.AddAnimation("IdleDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 0, 2));
            _spriteAnimator.AddAnimation("IdleRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 3, 5));
            _spriteAnimator.AddAnimation("IdleUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 6, 8));
            _spriteAnimator.AddAnimation("IdleLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(idleSprites, 9, 11));

            //run
            var runTexture = Entity.Scene.Content.LoadTexture("Content/Sprites/Player/hooded_knight_run.png");
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            _spriteAnimator.AddAnimation("RunRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 0, 9));
            _spriteAnimator.AddAnimation("RunLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 10, 19));
            _spriteAnimator.AddAnimation("RunDown", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 20, 27));
            _spriteAnimator.AddAnimation("RunUp", AnimatedSpriteHelper.GetSpriteArrayFromRange(runSprites, 30, 37));

            //hurt
            var hurtTexture = Entity.Scene.Content.LoadTexture("Content/Sprites/Player/hooded_knight_hurt.png");
            var hurtSprites = Sprite.SpritesFromAtlas(hurtTexture, 64, 64);
            _spriteAnimator.AddAnimation("HurtRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 0, 4));
            _spriteAnimator.AddAnimation("HurtLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(hurtSprites, 5, 9));

            //death
            var deathTexture = Entity.Scene.Content.LoadTexture("Content/Sprites/Player/hooded_knight_death.png");
            var deathSprites = Sprite.SpritesFromAtlas(deathTexture, 64, 64);
            _spriteAnimator.AddAnimation("DeathRight", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 0, 9));
            _spriteAnimator.AddAnimation("DeathLeft", AnimatedSpriteHelper.GetSpriteArrayFromRange(deathSprites, 10, 19));
        }

        public void Idle()
        {
            if (_xAxisInput.Value != 0 || _yAxisInput.Value != 0)
            {
                StateMachine.ChangeState<PlayerRunning>();
                return;
            }

            //handle animation
            var animation = "IdleDown";
            if (_direction.X < 0)
            {
                animation = "IdleLeft";
            }
            else if (_direction.X > 0)
            {
                animation = "IdleRight";
            }
            if (_direction.Y < 0)
            {
                animation = "IdleUp";
            }
            else if (_direction.Y > 0)
            {
                animation = "IdleDown";
            }

            if (!_spriteAnimator.IsAnimationActive(animation))
            {
                _spriteAnimator.Play(animation);
            }
        }

        public void Move()
        {
            var newDirection = new Vector2(_xAxisInput.Value, _yAxisInput.Value);

            if (newDirection == Vector2.Zero)
            {
                StateMachine.ChangeState<PlayerIdle>();
                return;
            }
            else
            {
                _direction = newDirection;

                //animation
                var animation = "RunDown";
                if (_direction.X < 0)
                {
                    animation = "RunLeft";
                }
                else if (_direction.X > 0)
                {
                    animation = "RunRight";
                }
                if (_direction.Y < 0)
                {
                    animation = "RunUp";
                }
                else if (_direction.Y > 0)
                {
                    animation = "RunDown";
                }

                if (!_spriteAnimator.IsAnimationActive(animation))
                {
                    _spriteAnimator.Play(animation);
                }

                //move
                //var movementX = (int)Math.Round(_direction.X * _moveSpeed * Time.DeltaTime);
                //var movementY = (int)Math.Round(_direction.Y * _moveSpeed * Time.DeltaTime);
                //var movement = new Vector2(movementX, movementY);
                _direction.Normalize();
                var movement = _direction * _moveSpeed * Time.DeltaTime;

                _mover.CalculateMovement(ref movement, out var result);
                _subPixelV2.Update(ref movement);
                _mover.ApplyMovement(movement);
            }
        }

        public void OnHitboxHit(Hitbox hitbox)
        {
            _healthComponent.DecrementHealth(hitbox.Damage);
            if (_healthComponent.IsDepleted())
            {

            }
            
            _healthComponent.DecrementHealth(hitbox.Damage);
            _hurtComponent.PlayHurtEffect();
            _invincibilityComponent.Activate();
        }

        public void CheckForHurtCompletion()
        {
            if (new[] { "HurtRight", "HurtLeft" }.Contains(_spriteAnimator.CurrentAnimationName))
            {
                if (_spriteAnimator.AnimationState == SpriteAnimator.State.Completed)
                {
                    StateMachine.ChangeState<PlayerIdle>();
                }
            }
            else
            {
                StateMachine.ChangeState<PlayerIdle>();
            }
        }
    }

    #region STATE MACHINE

    public class PlayerStateMachine : StateMachine<Player>
    {
        public PlayerStateMachine(Player context, State<Player> initialState) : base(context, initialState)
        {
            AddState(new PlayerIdle());
            AddState(new PlayerRunning());
            AddState(new PlayerHurt());
            AddState(new PlayerDying());
        }
    }

    public class PlayerIdle : State<Player>
    {
        public override void Update(float deltaTime)
        {
            _context.Idle();
        }
    }

    public class PlayerRunning : State<Player>
    {
        public override void Update(float deltaTime)
        {
            _context.Move();
        }
    }

    public class PlayerHurt : State<Player>
    {
        public override void Update(float deltaTime)
        {
            _context.CheckForHurtCompletion();
        }
    }

    public class PlayerDying : State<Player>
    {
        public override void Update(float deltaTime)
        {
            _context.CheckForDeathCompletion();
        }
    }

    #endregion
}
