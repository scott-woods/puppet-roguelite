using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Enums;
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
        Collider _hitboxCollider;
        Collider _collider;
        HealthComponent _healthComponent;
        Hitbox _hitbox;
        HurtComponent _hurtComponent;
        InvincibilityComponent _invincibilityComponent;

        //misc
        Vector2 _direction = new Vector2(0, 1);

        public Player()
        {
            StateMachine = new PlayerStateMachine(this, new PlayerIdle());
        }

        public override void Initialize()
        {
            base.Initialize();

            //add sprites
            SetupSprites();

            //add mover
            _mover = Entity.AddComponent(new Mover());

            //Add hitbox collider
            _hitboxCollider = Entity.AddComponent(new BoxCollider(10, 20));
            _hitboxCollider.IsTrigger = true;
            _hitboxCollider.PhysicsLayer = (int)PhysicsLayers.Hitbox;

            //Add hitbox
            _hitbox = Entity.AddComponent(new Hitbox());

            //Add health component
            _healthComponent = Entity.AddComponent(new HealthComponent(10));

            //Add hurt component
            _hurtComponent = Entity.AddComponent(new HurtComponent());

            //Add invincibility component
            _invincibilityComponent = Entity.AddComponent(new InvincibilityComponent(1));

            //Add observers
            _hitbox.Emitter.AddObserver(HitboxEventTypes.Hit, OnHitboxHit);

            //setup input
            SetupInput();
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
            var idleTexture = Entity.Scene.Content.LoadTexture("Content/Sprites/Player/hooded_knight_idle.png");
            var idleSprites = Sprite.SpritesFromAtlas(idleTexture, 64, 64);
            var runTexture = Entity.Scene.Content.LoadTexture("Content/Sprites/Player/hooded_knight_run.png");
            var runSprites = Sprite.SpritesFromAtlas(runTexture, 64, 64);
            _spriteAnimator = Entity.AddComponent(new SpriteAnimator());
            _spriteAnimator.AddAnimation("IdleLeft", new[]
            {
                idleSprites[9],
                idleSprites[10],
                idleSprites[11]
            });
            _spriteAnimator.AddAnimation("IdleRight", new[]
            {
                idleSprites[3],
                idleSprites[4],
                idleSprites[5]
            });
            _spriteAnimator.AddAnimation("IdleDown", new[]
            {
                idleSprites[0],
                idleSprites[1],
                idleSprites[2]
            });
            _spriteAnimator.AddAnimation("IdleUp", new[]
            {
                idleSprites[6],
                idleSprites[7],
                idleSprites[8]
            });
            _spriteAnimator.AddAnimation("RunLeft", new[]
            {
                runSprites[10],
                runSprites[11],
                runSprites[12],
                runSprites[13],
                runSprites[14],
                runSprites[15],
                runSprites[16],
                runSprites[17],
                runSprites[18],
                runSprites[19]
            });
            _spriteAnimator.AddAnimation("RunRight", new[]
            {
                runSprites[0],
                runSprites[1],
                runSprites[2],
                runSprites[3],
                runSprites[4],
                runSprites[5],
                runSprites[6],
                runSprites[7],
                runSprites[8],
                runSprites[9]
            });
            _spriteAnimator.AddAnimation("RunDown", new[]
            {
                runSprites[20],
                runSprites[21],
                runSprites[22],
                runSprites[23],
                runSprites[24],
                runSprites[25],
                runSprites[26],
                runSprites[27]
            });
            _spriteAnimator.AddAnimation("RunUp", new[]
            {
                runSprites[30],
                runSprites[31],
                runSprites[32],
                runSprites[33],
                runSprites[34],
                runSprites[35],
                runSprites[36],
                runSprites[37]
            });
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

                var movement = _direction * _moveSpeed * Time.DeltaTime;

                _mover.CalculateMovement(ref movement, out var result);
                _mover.ApplyMovement(movement);

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
            }
        }

        public void OnHitboxHit(Collider collider)
        {
            if (collider.Entity.TryGetComponent<DamageComponent>(out var damageComponent))
            {
                if (!_invincibilityComponent.IsInvincible)
                {
                    _healthComponent.DecrementHealth(damageComponent.Damage);
                    _hurtComponent.PlayHurtEffect();
                    _invincibilityComponent.Activate();
                }
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

    #endregion
}
