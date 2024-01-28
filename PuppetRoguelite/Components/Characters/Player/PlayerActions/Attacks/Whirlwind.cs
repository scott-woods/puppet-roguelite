using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Helpers;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Tools;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    [PlayerActionInfo("Whirlwind", 1, PlayerActionCategory.Attack, "Spinning melee that hits enemies 360 degrees around you.")]
    public class Whirlwind : PlayerAction
    {
        //data
        const int _damage = 4;
        const float _radius = 12;
        const float _pushForce = 2.5f;

        //components
        PlayerSim _playerSim;
        CircleHitbox _hitbox;
        SpriteAnimator _animator;

        //misc
        List<int> _hitboxActiveFrames = new List<int>() { 0, 1, 2, 3, 4 };
        Direction _direction;
        List<int> _leftFlipFrames = new List<int>() { 0, 4, 5 };
        List<int> _rightFlipFrames = new List<int>() { 2 };
        List<int> _upFlipFrames = new List<int>() { 3 };
        List<int> _downFlipFrames = new List<int>() { 1 };

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
            var texture = Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Sci_fi_player_with_sword);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 65);
            _animator.AddAnimation("WhirlwindLeft", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 143, 210, 143, 54, 143, 144 }, true));
            _animator.AddAnimation("WhirlwindRight", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 143, 54, 143, 210, 143, 144 }, true));
            _animator.AddAnimation("WhirlwindUp", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 210, 143, 54, 143, 210, 211 }, true));
            _animator.AddAnimation("WhirlwindDown", AnimatedSpriteHelper.GetSpriteArray(sprites, new List<int> { 54, 143, 210, 143, 54, 55 }, true));

            //start sim loop
            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        public override void Execute()
        {
            base.Execute();

            _animator.SetColor(Color.White);
            _animator.Speed = 1.4f;
            _executionCoroutine = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            Debug.Log("starting Whirlwind execution coroutine");
            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Whirlwind_speen);
            _spin = _coroutineManager.StartCoroutine(Spin());
            yield return _spin;
            _spin = null;
            //yield return Spin();
            Debug.Log("whirlwind spin finished");
            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            _coroutineManager.Update();

            if (_spin != null)
            {
                var flipFrames = new List<int>();
                switch (_animator.CurrentAnimationName)
                {
                    case "WhirlwindUp":
                        flipFrames = _upFlipFrames; break;
                    case "WhirlwindDown":
                        flipFrames = _downFlipFrames; break;
                    case "WhirlwindRight":
                        flipFrames = _rightFlipFrames; break;
                    case "WhirlwindLeft":
                        flipFrames = _leftFlipFrames; break;
                }

                if (flipFrames.Contains(_animator.CurrentFrame + 1)
                    || (_animator.CurrentAnimationName == "WhirlwindLeft" && flipFrames.Contains(_animator.CurrentFrame)))
                {
                    _playerSim.VelocityComponent.SetDirection(Direction.Left);
                }
                else
                {
                    _playerSim.VelocityComponent.SetDirection(Direction.Right);
                }
            }

            if (State == PlayerActionState.Preparing)
            {
                //handle confirm
                if (Controls.Instance.Confirm.IsPressed)
                {
                    Reset();
                    HandlePreparationFinished(Position);
                    return;
                }

                var newDirection = CalculateDirection();

                if (_spin == null)
                {
                    _playerSim.VelocityComponent.SetDirection(newDirection);
                }

                //handle direction change
                if (newDirection != _direction)
                {
                    _direction = newDirection;

                    Reset();

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
            var secondsPerFrame = 1 / (_animator.CurrentAnimation.FrameRates[0] * _animator.Speed);
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
                _playerSim.Idle(_direction);
                yield return Coroutine.WaitForSeconds(.2f);

                //spin
                _animator.Speed = 1.4f;
                _spin = _coroutineManager.StartCoroutine(Spin());
                yield return _spin;
                _spin = null;
            }
        }

        public override void Reset()
        {
            Log.Debug("Resetting Whirlwind");

            //coroutines
            _simulationLoop?.Stop();
            _simulationLoop = null;
            _executionCoroutine?.Stop();
            _executionCoroutine = null;
            _spin?.Stop();
            _spin = null;

            //animator
            _animator.Stop();

            //hitbox
            _hitbox.SetEnabled(false);
        }

        Direction CalculateDirection()
        {
            if (!Game1.InputStateManager.IsUsingGamepad)
            {
                return DirectionHelper.GetDirectionToMouse(Position);
            }
            else
            {
                if (Controls.Instance.XAxisIntegerInput.Value > 0)
                    return Direction.Right;
                if (Controls.Instance.XAxisIntegerInput.Value < 0)
                    return Direction.Left;
                if (Controls.Instance.YAxisIntegerInput.Value > 0)
                    return Direction.Down;
                if (Controls.Instance.YAxisIntegerInput.Value < 0)
                    return Direction.Up;

                return _direction;
            }
        }
    }
}
