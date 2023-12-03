using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    [PlayerActionInfo("Dash", 2, PlayerActionCategory.Attack)]
    public class DashAttack : PlayerAction
    {
        int _damage = 6;
        int _range = 48;
        int _startFrame = 0;
        int[] _hitboxActiveFrames = new int[] { 0 };
        bool _isAttacking = false;

        Vector2 _direction = Vector2.One;
        Vector2 _initialOriginPos;

        //components
        PlayerSim _playerSim;
        CircleHitbox _hitbox;
        PrototypeSpriteRenderer _target;
        SpriteTrail _trail;
        SpriteAnimator _animator;
        DirectionByMouse _dirByMouse;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop;
        ICoroutine _execution;

        public override void Prepare()
        {
            base.Prepare();

            FinalPosition = Position + _direction * _range;

            //player sim
            _playerSim = AddComponent(new PlayerSim());

            _initialOriginPos = _playerSim.OriginComponent.Origin;

            //hitbox
            _hitbox = AddComponent(new CircleHitbox(_damage, 16));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.PushForce = 0f;
            _hitbox.SetEnabled(false);

            //target that shows where dash will go
            _target = AddComponent(new PrototypeSpriteRenderer(8, 8));

            //get animator from player sim
            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));
            //var dashTexture = Core.Scene.Content.LoadTexture(Content.Textures.Characters.Player.Sci_fi_player_with_sword);
            //var dashSprites = Sprite.SpritesFromAtlas(dashTexture, 64, 65);
            //_animator.AddAnimation("DashAttackRight", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 4, 4, 4, 5, 6, 7, 7, 7 }, true));
            //_animator.AddAnimation("DashAttackLeft", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 13, 13, 13, 14, 15, 16, 16, 16 }, true));
            //_animator.AddAnimation("DashAttackDown", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 22, 22, 22, 23, 24, 25, 25, 25 }, true));
            //_animator.AddAnimation("DashAttackUp", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 31, 31, 31, 32, 33, 34, 34, 34 }, true));

            //sprite trail
            _trail = AddComponent(new SpriteTrail(_animator));
            _trail.FadeDelay = 0f;
            _trail.FadeDuration = .2f;
            _trail.MinDistanceBetweenInstances = 12;

            //dir by mouse
            _dirByMouse = AddComponent(new DirectionByMouse(_initialOriginPos));

            //start playing animation
            //var animation = GetNextAnimation();
            //_animator.Play(animation);

            //start sim loop
            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        public override void Execute()
        {
            base.Execute();

            //move to initial position
            Position = InitialPosition;

            //animator adjustments
            _animator.Speed = 2;
            _animator.SetColor(Color.White);
            //_animator.OnAnimationCompletedEvent += OnAnimationFinished;

            //execute
            _execution = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            //idle for a moment
            _animator.Speed = 1f;
            _playerSim.Idle(_dirByMouse.CurrentDirection);
            yield return Coroutine.WaitForSeconds(.2f);

            var dashExecution = _coroutineManager.StartCoroutine(ExecuteDash());
            yield return dashExecution;

            _execution = null;
            _simulationLoop = null;
            HandleExecutionFinished();
        }

        void OnAnimationFinished(string animationName)
        {
            _animator.SetSprite(_animator.CurrentAnimation.Sprites.Last());
            _isAttacking = false;
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
                    _execution = null;
                    _simulationLoop = null;
                    _isAttacking = false;
                    HandlePreparationFinished(FinalPosition);
                    return;
                }

                //handle direction change
                if (_dirByMouse.CurrentDirection != _dirByMouse.PreviousDirection)
                {
                    Debug.Log("direction change");
                    _coroutineManager.StopAllCoroutines();

                    //stop animator
                    _animator.Stop();

                    //reset position
                    Position = InitialPosition;

                    _isAttacking = false;

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

                //position player at their origin instead of by entity
                //if (TryGetComponent<OriginComponent>(out var originComponent))
                //{
                //    var diff = Position - originComponent.Origin;
                //    newFinalPos += diff;
                //}

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

            //if (_animator.IsRunning)
            //{
            //    var animation = GetNextAnimation();
            //    if (_animator.CurrentAnimationName != animation)
            //    {
            //        var frame = _animator.CurrentFrame;
            //        _animator.Play(animation, SpriteAnimator.LoopMode.Once);
            //        _animator.CurrentFrame = frame;
            //    }
            //}
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
                _execution = _coroutineManager.StartCoroutine(ExecuteDash());
                yield return _execution;

                //idle at end point for a moment
                _animator.Speed = 1f;
                _playerSim.Idle(_dirByMouse.CurrentDirection);
                yield return Coroutine.WaitForSeconds(.25f);

                //reset position
                Position = InitialPosition;
            }
        }

        IEnumerator ExecuteDash()
        {
            _isAttacking = true;

            var animation = GetNextAnimation();
            _animator.Play(animation, SpriteAnimator.LoopMode.Once);
            _animator.OnAnimationCompletedEvent += OnAnimationFinished;

            //wait to reach frame where movement should start
            //while (_animator.CurrentFrame != _startFrame)
            //{
            //    yield return null;
            //}

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
            while (movementTimeRemaining > 0)
            {
                //lerp towards target position using progress towards total movement time
                movementTimeRemaining -= Time.DeltaTime;
                var progress = (totalMovementTime - movementTimeRemaining) / totalMovementTime;
                var lerpPosition = Vector2.Lerp(InitialPosition, FinalPosition, progress);
                Position = lerpPosition;

                yield return null;
            }

            while (_isAttacking)
            {
                yield return null;
            }
        }

        string GetNextAnimation()
        {
            var angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(InitialPosition, FinalPosition));
            angle = (angle + 360) % 360;
            //var animation = "DashAttackRight";
            //if (angle >= 45 && angle < 135) animation = "DashAttackDown";
            //else if (angle >= 135 && angle < 225) animation = "DashAttackLeft";
            //else if (angle >= 225 && angle < 315) animation = "DashAttackUp";
            var animation = "Thrust";
            if (angle >= 45 && angle < 135) animation = "ThrustDown";
            else if (angle >= 225 && angle < 315) animation = "ThrustUp";

            //if (angle >= 135 && angle < 225) _animator.FlipX = true;
            //else _animator.FlipX = false;

            return animation;
        }
    }
}
