using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Svg;
using Nez.Systems;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Utilities
{
    [PlayerActionInfo("Teleport", 1, PlayerActionCategory.Utility)]
    public class Teleport : PlayerAction
    {
        const float _maxRadius = 100f;

        //components
        PlayerSim _playerSim;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _executionCoroutine;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim(Vector2.One));

            var animator = _playerSim.GetComponent<SpriteAnimator>();
            animator.SetColor(new Color(Color.White, 128));
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

            yield return Coroutine.WaitForSeconds(.1f);

            var animator = _playerSim.GetComponent<SpriteAnimator>();
            animator.SetColor(Color.White);
            var tween = animator.TweenColorTo(Color.Transparent, .15f);
            tween.SetEaseType(Nez.Tweens.EaseType.QuintIn);
            tween.Start();
            yield return tween.WaitForCompletion();

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Player_teleport, .6f);

            Position = FinalPosition;

            tween = animator.TweenColorTo(Color.White, .1f);
            tween.Start();
            yield return tween.WaitForCompletion();

            yield return Coroutine.WaitForSeconds(.25f);

            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            _coroutineManager.Update();

            if (State == PlayerActionState.Preparing)
            {
                //get mouse position
                var mousePos = Scene.Camera.MouseToWorldPoint();

                //first, distance to mouse must be within radius
                var dist = Vector2.Distance(mousePos, InitialPosition);
                if (dist < _maxRadius)
                {
                    //next, check that we are in a combat area
                    if (CombatArea.IsPointInCombatArea(mousePos))
                    {
                        //mouse is within radius and in a combat area
                        var targetPos = mousePos;

                        //position player at their origin instead of by entity
                        if (TryGetComponent<OriginComponent>(out var originComponent))
                        {
                            var diff = Position - originComponent.Origin;
                            targetPos += diff;
                        }

                        //set position
                        Position = targetPos;
                    }
                }

                //if left click at any point, continue using last valid position
                if (Input.LeftMouseButtonPressed)
                {
                    FinalPosition = Position;
                    HandlePreparationFinished(Position);
                }
            }
        }
    }
}
