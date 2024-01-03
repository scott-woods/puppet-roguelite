using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.StaticData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities
{
    [PlayerActionInfo("Decoy", 2, PlayerActionCategory.Utility, "Summon a Decoy to draw away enemy Attention.")]
    public class Decoy : PlayerAction
    {
        const float _maxRadius = 100f;
        const float _speed = 250f;

        Entity _targetEntity;
        DecoyPlayer _decoyPlayer;

        public override void Prepare()
        {
            base.Prepare();

            //keep player or sim player visible
            var turnPlayerSim = Scene.FindEntity("turn-player-sim");
            if (turnPlayerSim != null)
            {
                if (PlayerController.Instance.Entity.Position == turnPlayerSim.Position)
                {
                    PlayerController.Instance.Entity.SetEnabled(true);
                }
                else
                {
                    turnPlayerSim.SetEnabled(true);
                }
            }

            _targetEntity = Scene.CreateEntity("decoy-entity");
            _targetEntity.Position = Position;
            _decoyPlayer = _targetEntity.AddComponent(new DecoyPlayer(Vector2.One));

            var animator = _decoyPlayer.GetComponent<SpriteAnimator>();
            animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));
            _decoyPlayer.Idle(Direction.Down);
        }

        public override void Execute()
        {
            base.Execute();

            _targetEntity.SetEnabled(false);

            PlayerController.Instance.Entity.SetEnabled(true);

            Game1.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            yield return Coroutine.WaitForSeconds(.25f);

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Decoy_appear);
            _targetEntity.SetEnabled(true);
            _decoyPlayer.StartTimer();

            yield return Coroutine.WaitForSeconds(.25f);

            PlayerController.Instance.Entity.SetEnabled(false);

            HandleExecutionFinished();
        }

        public override void Update()
        {
            base.Update();

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
                    if (_targetEntity.TryGetComponent<OriginComponent>(out var originComponent))
                    {
                        var diff = _targetEntity.Position - originComponent.Origin;
                        relativePosition -= diff;
                    }

                    //next, check that we are in a combat area
                    if (CombatArea.IsPointInCombatArea(relativePosition))
                    {
                        //set position
                        _targetEntity.Position = desiredPos;
                    }
                }

                //if left click at any point, continue using last valid position
                if (Controls.Instance.Confirm.IsPressed)
                {
                    //FinalPosition = Position;
                    //_targetEntity.SetEnabled(false);
                    HandlePreparationFinished(Position);
                }
            }
        }

        public override void HandlePreparationCanceled()
        {
            base.HandlePreparationCanceled();

            _targetEntity.Destroy();
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
                    return _targetEntity.Position + (movement * Time.DeltaTime * _speed);
                }
                else return _targetEntity.Position;
            }
        }
    }
}
