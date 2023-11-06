using Microsoft.Xna.Framework;
using Nez;
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

        Vector2 _initialPosition;

        public Teleport(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler) : base(actionPrepFinishedHandler, actionPrepCanceledHandler, executionFinishedHandler)
        {
        }

        public override void Prepare()
        {
            base.Prepare();

            _initialPosition = Position;

            _playerSim = AddComponent(new PlayerSim(Vector2.One));
        }

        public override void Execute()
        {
            base.Execute();

            RemoveComponent(_playerSim);
            Game1.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            PlayerController.Instance.Entity.SetEnabled(true);
            PlayerController.Instance.Entity.SetPosition(_initialPosition);

            var cam = Scene.Camera.GetComponent<DeadzoneFollowCamera>();
            cam.SetFollowTarget(PlayerController.Instance.Entity);

            yield return Coroutine.WaitForSeconds(.3f);

            var tween = PlayerController.Instance.SpriteAnimator.TweenColorTo(Color.Transparent, .15f);
            tween.SetEaseType(Nez.Tweens.EaseType.QuintIn);
            tween.Start();
            yield return tween.WaitForCompletion();

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Player_teleport, .6f);

            PlayerController.Instance.Entity.SetPosition(Position);

            tween = PlayerController.Instance.SpriteAnimator.TweenColorTo(Color.White, .1f);
            tween.Start();
            yield return tween.WaitForCompletion();

            yield return Coroutine.WaitForSeconds(.2f);

            PlayerController.Instance.Entity.SetEnabled(false);

            cam.SetFollowTarget(this);

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
                        HandlePreparationFinished(Position);
                    }
                }
            }
        }
    }
}
