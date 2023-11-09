using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Svg;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
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
        const float _maxRadius = 128f;

        //components
        PlayerSim _playerSim;

        Vector2 _initialPosition, _finalPosition;

        public override Vector2 GetFinalPosition()
        {
            return _finalPosition;
        }

        public override void Prepare()
        {
            base.Prepare();

            _initialPosition = Position;

            _playerSim = AddComponent(new PlayerSim(Vector2.One));

            var animator = _playerSim.GetComponent<SpriteAnimator>();
            animator.SetColor(new Color(Color.White, 128));
            animator.Play("IdleDown");
        }

        public override void Execute()
        {
            base.Execute();

            Game1.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            Position = _initialPosition;

            yield return Coroutine.WaitForSeconds(.1f);

            var animator = _playerSim.GetComponent<SpriteAnimator>();
            animator.SetColor(Color.White);
            var tween = animator.TweenColorTo(Color.Transparent, .15f);
            tween.SetEaseType(Nez.Tweens.EaseType.QuintIn);
            tween.Start();
            yield return tween.WaitForCompletion();

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Player_teleport, .6f);

            Position = _finalPosition;

            tween = animator.TweenColorTo(Color.White, .1f);
            tween.Start();
            yield return tween.WaitForCompletion();

            yield return Coroutine.WaitForSeconds(.25f);

            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

            if (State == PlayerActionState.Preparing)
            {
                var mousePos = Scene.Camera.MouseToWorldPoint();
                var dist = Vector2.Distance(mousePos, _initialPosition);
                if (dist < _maxRadius)
                {
                    Position = mousePos;

                    if (Input.LeftMouseButtonPressed)
                    {
                        _finalPosition = Position;
                        HandlePreparationFinished(Position);
                    }
                }
            }
        }
    }
}
