using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Svg;
using Nez.Systems;
using Nez.Tweens;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.StaticData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities
{
    [PlayerActionInfo("Teleport", 1, PlayerActionCategory.Utility, "Instantly teleport to a nearby location.")]
    public class Teleport : PlayerAction
    {
        const float _maxRadius = 100f;
        const float _speed = 250f;

        //components
        PlayerSim _playerSim;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _executionCoroutine;
        ICoroutine _tweenCoroutine;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim(Vector2.One));

            var animator = _playerSim.GetComponent<SpriteAnimator>();
            animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));
            animator.Play("IdleDown");
        }

        public override void Execute()
        {
            base.Execute();

            _executionCoroutine = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            Position = InitialPosition;

            var animator = _playerSim.GetComponent<SpriteAnimator>();
            animator.SetColor(Color.White);
            var tween = animator.TweenColorTo(Color.Transparent, .15f);
            tween.SetEaseType(EaseType.QuintIn);
            tween.Start();
            _tweenCoroutine = _coroutineManager.StartCoroutine(tween.WaitForCompletion());
            yield return _tweenCoroutine;
            //yield return tween.WaitForCompletion();

            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Player_teleport);

            Position = FinalPosition;

            tween = animator.TweenColorTo(Color.White, .1f);
            tween.Start();
            _tweenCoroutine = _coroutineManager.StartCoroutine(tween.WaitForCompletion());
            yield return _tweenCoroutine;
            //yield return tween.WaitForCompletion();

            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            _coroutineManager.Update();

            if (State == PlayerActionState.Preparing)
            {
                //get desired position
                var desiredPos = GetDesiredPosition();

                //first, distance to mouse must be within radius
                var dist = Vector2.Distance(desiredPos, InitialPosition);
                if (dist < _maxRadius)
                {
                    //get relative position by origin if possible
                    var relativePosition = desiredPos;
                    if (TryGetComponent<OriginComponent>(out var originComponent))
                    {
                        var diff = Position - originComponent.Origin;
                        relativePosition -= diff;
                    }

                    //next, check that we are in a combat area
                    if (CombatArea.IsPointInCombatArea(relativePosition))
                    {
                        //set position
                        Position = desiredPos;
                    }
                }

                //if left click at any point, continue using last valid position
                if (Controls.Instance.Confirm.IsPressed)
                {
                    FinalPosition = Position;
                    HandlePreparationFinished(Position);
                }
            }
        }

        public override void Reset()
        {

        }

        Vector2 GetDesiredPosition()
        {
            if (!Game1.InputStateManager.IsUsingGamepad)
            {
                return Scene.Camera.MouseToWorldPoint();
            }
            else
            {
                var movement = Controls.Instance.DirectionalInput.Value;
                if (movement != Vector2.Zero)
                {
                    movement.Normalize();
                    return Position + (movement * Time.DeltaTime * _speed);
                }
                else return Position;
            }
        }
    }
}
