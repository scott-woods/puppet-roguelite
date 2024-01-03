using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Enemies;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using PuppetRoguelite.StaticData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities
{
    [PlayerActionInfo("Stasis Field", 2, PlayerActionCategory.Utility, "Freeze any enemies caught in its radius for a short time.")]
    public class StasisField : PlayerAction
    {
        //data
        const int _targetRadius = 80;
        const int _fieldRadius = 64;
        const float _freezeDuration = 3f;
        const float _speed = 250f;

        //entities
        Entity _targetEntity;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _executionCoroutine;

        //components
        SpriteRenderer _targetRenderer;
        PlayerSim _playerSim;

        public override void Prepare()
        {
            base.Prepare();

            var targetTexture = Scene.Content.LoadTexture(Nez.Content.Textures.UI.Crosshair005);
            _targetEntity = Scene.CreateEntity("stasis-field-target");
            _targetEntity.Position = Position;
            _targetRenderer = _targetEntity.AddComponent(new SpriteRenderer(targetTexture));

            _playerSim = AddComponent(new PlayerSim());

            _playerSim.Idle(Direction.Down);
        }

        public override void Execute()
        {
            base.Execute();

            _executionCoroutine = _coroutineManager.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            var enemies = Scene.FindComponentsOfType<EnemyBase>().Where(e => Vector2.Distance(e.Entity.Position, _targetEntity.Position) <= _fieldRadius);
            foreach (var enemy in enemies)
            {
                enemy.AddComponent(new FreezeComponent(_freezeDuration));
            }

            _targetEntity.Destroy();

            yield return Coroutine.WaitForSeconds(.25f);

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
                if (dist < _targetRadius)
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

                if (Controls.Instance.Confirm.IsPressed)
                {
                    HandlePreparationFinished(Position);
                }
            }
        }

        public override void Reset()
        {
            _targetEntity?.Destroy();
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
