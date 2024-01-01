using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using PuppetRoguelite.StaticData;
using Serilog;
using System;
using System.Collections;
using System.Linq;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    [PlayerActionInfo("Dash", 2, PlayerActionCategory.Attack, "Slice along a straight line, damaging any enemies in your path.")]
    public class DashAttack : PlayerAction
    {
        //constants
        const int _damage = 4;
        const int _range = 48;
        const int _startFrame = 0;
        int[] _hitboxActiveFrames = new int[] { 0 };

        //misc
        bool _isAttacking = false;
        Vector2 _direction = Vector2.One;
        Vector2 _initialOriginPos;
        bool _canConfirm = true;

        //components
        PlayerSim _playerSim;
        CircleHitbox _hitbox;
        SpriteRenderer _target;
        SpriteTrail _trail;
        SpriteAnimator _animator;
        DirectionByMouse _dirByMouse;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop;
        ICoroutine _dashAttackExecutionCoroutine;
        ICoroutine _executeDashCoroutine;

        public override void Prepare()
        {
            base.Prepare();

            //set final position
            FinalPosition = Position + _direction * _range;

            //player sim
            _playerSim = AddComponent(new PlayerSim());

            //set initial origin pos
            _initialOriginPos = _playerSim.OriginComponent.Origin;

            //hitbox
            _hitbox = AddComponent(new CircleHitbox(_damage, 8));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.PushForce = 0f;
            _hitbox.SetEnabled(false);

            //target that shows where dash will go
            var targetTexture = Scene.Content.LoadTexture(Nez.Content.Textures.UI.Crosshair005);
            _target = AddComponent(new SpriteRenderer(targetTexture));

            //get animator from player sim
            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));

            //sprite trail
            _trail = AddComponent(new SpriteTrail(_animator));
            _trail.FadeDelay = 0f;
            _trail.FadeDuration = .2f;
            _trail.MinDistanceBetweenInstances = 12;

            //dir by mouse
            _dirByMouse = AddComponent(new DirectionByMouse(_initialOriginPos));

            //start sim loop
            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        public override void Execute()
        {
            base.Execute();

            Log.Information("Executing DashAttack");

            //disable target
            _target.SetEnabled(false);

            //move to initial position
            Position = InitialPosition;

            //animator adjustments
            _animator.Speed = 2;
            _animator.SetColor(Color.White);
            //_animator.OnAnimationCompletedEvent += OnAnimationFinished;

            //execute
            Log.Debug("Starting DashAttackExecutionCoroutine");
            _dashAttackExecutionCoroutine = _coroutineManager.StartCoroutine(DashAttackExecutionCoroutine());
        }

        public override void Update()
        {
            base.Update();

            _coroutineManager.Update();

            if (State == PlayerActionState.Preparing)
            {
                //handle confirm
                if (Controls.Instance.Confirm.IsPressed)
                {
                    if (_canConfirm)
                    {
                        Reset();
                        HandlePreparationFinished(FinalPosition);
                        return;
                    }
                }

                //handle direction change
                if (_dirByMouse.CurrentDirection != _dirByMouse.PreviousDirection)
                {
                    Log.Debug("DashAttack: direction change");
                    Reset();

                    //restart sim loop
                    _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
                    return;
                }

                //calculate direction by mouse position
                _direction = Core.Scene.Camera.MouseToWorldPoint() - _initialOriginPos;
                _direction.Normalize();
                _playerSim.VelocityComponent.SetDirection(_direction);

                //get first valid position
                var newFinalPos = InitialPosition;
                var range = _range;
                while (range > 0)
                {
                    var testPos = InitialPosition + _direction * range;
                    var translatedOriginPos = _initialOriginPos + _direction * range;
                    if (CombatArea.IsPointInCombatArea(translatedOriginPos))
                    {
                        newFinalPos = testPos;
                        break;
                    }
                    range--;
                }

                FinalPosition = new Vector2((float)Math.Round(newFinalPos.X), (float)Math.Round(newFinalPos.Y));

                //update target
                _target.SetLocalOffset(FinalPosition - Position);
            }
            else if (State == PlayerActionState.Executing && _isAttacking)
            {
                if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                {
                    if (!_hitbox.Enabled)
                    {
                        _hitbox.SetEnabled(true);
                    }
                }
                else _hitbox.SetEnabled(false);
            }
        }

        IEnumerator SimulationLoop()
        {
            while (State == PlayerActionState.Preparing)
            {
                //idle for a moment
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.5f);

                //dash
                _animator.Speed = 1f;
                _executeDashCoroutine = _coroutineManager.StartCoroutine(ExecuteDash());
                yield return _executeDashCoroutine;
                _executeDashCoroutine = null;

                //idle at end point for a moment
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.25f);

                //reset position
                Position = InitialPosition;
            }
        }

        IEnumerator DashAttackExecutionCoroutine()
        {
            //idle for a moment
            _animator.Speed = 1f;
            _playerSim.Idle(_dirByMouse.CurrentDirection);
            yield return Coroutine.WaitForSeconds(.2f);

            Log.Debug("Starting ExecuteDash Coroutine");
            _executeDashCoroutine = _coroutineManager.StartCoroutine(ExecuteDash());
            yield return _executeDashCoroutine;
            _executeDashCoroutine = null;
            Log.Debug("Finished ExecuteDash Coroutine");

            HandleExecutionFinished();
        }

        IEnumerator ExecuteDash()
        {
            _isAttacking = true;

            var animation = GetNextAnimation();
            _animator.Play(animation, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAnimationFinished;

            //if not in prep mode, play sound
            if (State == PlayerActionState.Executing)
            {
                Game1.AudioManager.PlaySound(Content.Audio.Sounds._20_Slash_02);
            }

            //determine total amount of time needed to move to target 
            var secondsPerFrame = 1 / (_animator.CurrentAnimation.FrameRate * _animator.Speed);
            var movementFrames = 1;
            var totalMovementTime = movementFrames * secondsPerFrame;
            var movementTimeRemaining = movementFrames * secondsPerFrame;

            //move entity
            Log.Debug($"ExecuteDash: Waiting for {movementTimeRemaining}");
            while (movementTimeRemaining > 0)
            {
                //lerp towards target position using progress towards total movement time
                movementTimeRemaining -= Time.DeltaTime;
                var progress = (totalMovementTime - movementTimeRemaining) / totalMovementTime;
                var lerpPosition = Vector2.Lerp(InitialPosition, FinalPosition, progress);
                Position = lerpPosition;

                yield return null;
            }
            Log.Debug($"ExecuteDash: Finished waiting");

            Log.Debug($"ExecuteDash: Waiting for _isAttacking to be false");
            while (_isAttacking)
            {
                yield return null;
            }
            Log.Debug("ExecuteDash finished");
        }

        void OnAnimationFinished(string animationName)
        {
            Log.Debug($"DashAttack: Animation Finished: {animationName}");
            _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
            _isAttacking = false;
        }

        string GetNextAnimation()
        {
            var angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(InitialPosition, FinalPosition));
            angle = (angle + 360) % 360;
            var animation = "Thrust";
            if (angle >= 45 && angle < 135) animation = "ThrustDown";
            else if (angle >= 225 && angle < 315) animation = "ThrustUp";

            return animation;
        }

        public override void Reset()
        {
            Log.Debug("Resetting DashAttack");

            //handle coroutines
            _simulationLoop?.Stop();
            _simulationLoop = null;
            _executeDashCoroutine?.Stop();
            _executeDashCoroutine = null;
            _dashAttackExecutionCoroutine?.Stop();
            _dashAttackExecutionCoroutine = null;

            //fields
            _isAttacking = false;

            //hitbox
            _hitbox.SetEnabled(false);

            //reset position
            Position = InitialPosition;
            
            //animator
            _animator.Stop();
            _animator.OnAnimationCompletedEvent -= OnAnimationFinished;
        }

        public void SetConfirmationEnabled(bool enabled)
        {
            _canConfirm = enabled;
        }
    }
}
