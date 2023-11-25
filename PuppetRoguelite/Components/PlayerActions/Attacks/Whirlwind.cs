using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Attacks
{
    [PlayerActionInfo("Whirlwind", 1, PlayerActionCategory.Attack)]
    public class Whirlwind : PlayerAction
    {
        //data
        const int _damage = 3;
        const float _radius = 12;
        const float _pushForce = 2.5f;

        //components
        PlayerSim _playerSim;
        CircleHitbox _hitbox;
        SpriteAnimator _animator;
        DirectionByMouse _dirByMouse;

        //misc
        List<int> _hitboxActiveFrames = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
        Direction _direction;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop, _executionCoroutine, _spin;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim());

            _hitbox = AddComponent(new CircleHitbox(_damage, 12));
            _hitbox.PushForce = _pushForce;
            _hitbox.SetEnabled(false);
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);

            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));
            _animator.Speed = 2f;
            var texture = Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);
            _animator.AddAnimation("WhirlwindRight", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int>
            {
                0, 1, 2, 19, 20, 14, 15, 28, 29, 0
            }, true));
            _animator.AddAnimation("WhirlwindLeft", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int>
            {
                9, 14, 15, 28, 29, 1, 2, 19, 20, 9
            }, true));
            _animator.AddAnimation("WhirlwindDown", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int>
            {
                18, 19, 20, 14, 15, 28, 29, 1, 2, 18
            }, true));
            _animator.AddAnimation("WhirlwindUp", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int>
            {
                27, 28, 29, 1, 2, 19, 20, 14, 15, 27
            }, true));

            _dirByMouse = AddComponent(new DirectionByMouse());

            //start sim loop
            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        public override void Execute()
        {
            base.Execute();

            _animator.SetColor(Color.White);
            _animator.Speed = 2.25f;
            _executionCoroutine = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            Debug.Log("starting Whirlwind execution coroutine");
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Whirlwind_speen);
            _spin = _coroutineManager.StartCoroutine(Spin());
            yield return _spin;
            //yield return Spin();
            Debug.Log("whirlwind spin finished");
            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            _coroutineManager.Update();

            if (State == PlayerActionState.Preparing)
            {
                //handle confirm
                if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E) || Input.LeftMouseButtonPressed)
                {
                    _animator.Stop();
                    _coroutineManager.StopAllCoroutines();
                    HandlePreparationFinished(Position);
                    return;
                }

                _direction = _dirByMouse.CurrentDirection;

                //handle direction change
                if (_dirByMouse.CurrentDirection != _dirByMouse.PreviousDirection)
                {
                    _coroutineManager.StopAllCoroutines();

                    //stop animator
                    _animator.Stop();

                    //restart sim loop
                    _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
                }
            }
            else //not in prep mode
            {
                var validAnims = new List<string> { "WhirlwindUp", "WhirlwindDown", "WhirlwindLeft", "WhirlwindRight" };
                if (validAnims.Contains(_animator.CurrentAnimationName))
                {
                    if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                    {
                        _hitbox.SetEnabled(true);
                    }
                    else _hitbox.SetEnabled(false);
                }
                else _hitbox.SetEnabled(false);
            }
        }

        IEnumerator Spin()
        {
            //determine animation and starting angle of hitbox based on direction
            var animation = "";
            var angle = MathHelper.ToRadians(0f);
            switch (_direction)
            {
                case Direction.Up:
                    animation = "WhirlwindUp";
                    angle = MathHelper.ToRadians(270f);
                    break;
                case Direction.Down:
                    animation = "WhirlwindDown";
                    angle = MathHelper.ToRadians(90f);
                    break;
                case Direction.Left:
                    animation = "WhirlwindLeft";
                    angle = MathHelper.ToRadians(180f);
                    break;
                case Direction.Right:
                    animation = "WhirlwindRight";
                    angle = MathHelper.ToRadians(0f);
                    break;
            }

            _animator.Play(animation, SpriteAnimator.LoopMode.Once);

            //determine total amount of time needed to move to target 
            var secondsPerFrame = 1 / (_animator.CurrentAnimation.FrameRate * _animator.Speed);
            var totalMovementTime = _animator.CurrentAnimation.Sprites.Length * secondsPerFrame;
            var movementTimeRemaining = totalMovementTime;

            //determine rotation speed based on total movement time
            var rotationSpeed = MathHelper.TwoPi / totalMovementTime;

            Debug.Log("starting whirlwind movement while loop");
            while (movementTimeRemaining > 0)
            {
                movementTimeRemaining -= Time.DeltaTime;

                angle += rotationSpeed * Time.DeltaTime;
                angle = angle % MathHelper.TwoPi;

                var newOffsetX = (float)Math.Cos(angle) * _radius;
                var newOffsetY = (float)Math.Sin(angle) * _radius;

                _hitbox.SetLocalOffset(new Vector2(newOffsetX, newOffsetY));

                yield return null;
            }

            Debug.Log("finished whirlwind movement while loop");
            yield break;
        }

        IEnumerator SimulationLoop()
        {
            while (State == PlayerActionState.Preparing)
            {
                //idle for a moment
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.2f);

                //spin
                _animator.Speed = 2f;
                _spin = _coroutineManager.StartCoroutine(Spin());
                yield return _spin;
            }
        }
    }
}
