using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
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
    [PlayerActionInfo("Dash", 2, PlayerActionCategory.Attack)]
    public class DashAttack : PlayerAction
    {
        int _damage = 3;
        int _range = 48;
        int _startFrame = 3;
        int[] _hitboxActiveFrames = new int[] { 3, 4, 5 };

        Vector2 _direction = Vector2.One;

        //components
        SpriteAnimator _animator;
        CircleHitbox _hitbox;
        PrototypeSpriteRenderer _target;
        YSorter _ySorter;
        SpriteTrail _trail;
        OriginComponent _originComponent;

        public Vector2 InitialPosition, FinalPosition;

        ICoroutine _simulationLoop;

        public DashAttack(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler) : base(actionPrepFinishedHandler, actionPrepCanceledHandler, executionFinishedHandler)
        {
        }

        public override void Prepare()
        {
            base.Prepare();

            InitialPosition = Position;
            FinalPosition = Position + (_direction * _range);

            //hitbox
            _hitbox = AddComponent(new CircleHitbox(_damage, 16));
            Flags.SetFlagExclusive(ref _hitbox.PhysicsLayer, (int)PhysicsLayers.PlayerHitbox);
            Flags.SetFlagExclusive(ref _hitbox.CollidesWithLayers, (int)PhysicsLayers.EnemyHurtbox);
            _hitbox.PushForce = 0f;

            //target that shows where dash will go
            _target = AddComponent(new PrototypeSpriteRenderer(8, 8));

            _animator = AddComponent(new SpriteAnimator());
            _animator.SetColor(new Color(Color.White, 128));
            var dashTexture = Game1.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Player.Hooded_knight_attack);
            var dashSprites = Sprite.SpritesFromAtlas(dashTexture, 64, 64);
            _animator.AddAnimation("DashRight", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 4, 4, 4, 5, 6, 7, 7, 7 }, true));
            _animator.AddAnimation("DashLeft", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 13, 13, 13, 14, 15, 16, 16, 16 }, true));
            _animator.AddAnimation("DashDown", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 22, 22, 22, 23, 24, 25, 25, 25 }, true));
            _animator.AddAnimation("DashUp", AnimatedSpriteHelper.GetSpriteArray(dashSprites, new List<int>() { 31, 31, 31, 32, 33, 34, 34, 34 }, true));

            //sprite trail
            _trail = AddComponent(new SpriteTrail(_animator));
            _trail.FadeDelay = 0f;
            _trail.FadeDuration = .2f;
            _trail.MinDistanceBetweenInstances = 12;

            _originComponent = AddComponent(new OriginComponent(new Vector2(0, 12)));

            _ySorter = AddComponent(new YSorter(_animator, _originComponent));

            var animation = GetNextAnimation();
            _animator.Play(animation);
            _simulationLoop = Game1.StartCoroutine(SimulationLoop());
        }

        public override void Execute()
        {
            base.Execute();

            //reattach entity to scene
            Position = InitialPosition;

            //animator adjustments
            _animator.Speed = 2;
            _animator.SetColor(Color.White);
            _animator.OnAnimationCompletedEvent += OnAnimationFinished;

            //execute
            Game1.StartCoroutine(ExecuteDash());
        }

        void OnAnimationFinished(string animationName)
        {
            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            if (State == PlayerActionState.Preparing)
            {
                //handle confirm
                if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E) || Input.LeftMouseButtonPressed)
                {
                    _animator.Stop();
                    _simulationLoop.Stop();
                    HandlePreparationFinished(FinalPosition);
                    return;
                }

                //calculate direction by mouse position
                _direction = Game1.Scene.Camera.MouseToWorldPoint() - InitialPosition;
                _direction.Normalize();

                //get new final position rounded to ints
                var newFinalPos = InitialPosition + (_direction * _range);
                FinalPosition = new Vector2((float)Math.Round(newFinalPos.X), (float)Math.Round(newFinalPos.Y));

                //update target
                _target.SetLocalOffset(FinalPosition - Position);
            }
            else
            {
                if (_hitboxActiveFrames.Contains(_animator.CurrentFrame))
                {
                    _hitbox.SetEnabled(true);
                }
                else _hitbox.SetEnabled(false);
            }

            if (_animator.IsRunning)
            {
                var animation = GetNextAnimation();
                if (_animator.CurrentAnimationName != animation)
                {
                    var frame = _animator.CurrentFrame;
                    _animator.Play(animation, SpriteAnimator.LoopMode.Once);
                    _animator.CurrentFrame = frame;
                }
            }
        }

        IEnumerator SimulationLoop()
        {
            while (State == PlayerActionState.Preparing)
            {
                yield return ExecuteDash();
                yield return Coroutine.WaitForSeconds(.25f);
                Position = InitialPosition;
            }
        }

        IEnumerator ExecuteDash()
        {
            _animator.Play(GetNextAnimation(), SpriteAnimator.LoopMode.Once);

            //wait to reach frame where movement should start
            while (_animator.CurrentFrame != _startFrame)
            {
                yield return null;
            }

            //if not in prep mode, play sound
            if (State == PlayerActionState.Executing)
            {
                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._20_Slash_02, .5f);
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
        }

        string GetNextAnimation()
        {
            var angle = MathHelper.ToDegrees(Mathf.AngleBetweenVectors(InitialPosition, FinalPosition));
            angle = (angle + 360) % 360;
            var animation = "DashRight";
            if (angle >= 45 && angle < 135) animation = "DashDown";
            else if (angle >= 135 && angle < 225) animation = "DashLeft";
            else if (angle >= 225 && angle < 315) animation = "DashUp";

            return animation;
        }
    }
}
