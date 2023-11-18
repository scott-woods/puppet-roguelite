using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Utilities
{
    [PlayerActionInfo("Stasis Field", 2, PlayerActionCategory.Utility)]
    public class StasisField : PlayerAction
    {
        //data
        const int _targetRadius = 80;
        const int _fieldRadius = 64;
        const float _freezeDuration = 3f;

        //entities
        Entity _targetEntity;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _executionCoroutine;

        //components
        PrototypeSpriteRenderer _targetRenderer;
        PlayerSim _playerSim;

        public override void Prepare()
        {
            base.Prepare();

            _targetEntity = Scene.CreateEntity("stasis-field-target");
            _targetRenderer = _targetEntity.AddComponent(new PrototypeSpriteRenderer(8, 8));

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
            var enemies = Scene.FindComponentsOfType<Enemy>().Where(e => Vector2.Distance(e.Entity.Position, _targetEntity.Position) <= _fieldRadius);
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
                var mousePos = Scene.Camera.MouseToWorldPoint();
                if (Vector2.Distance(mousePos, InitialPosition) <= _targetRadius)
                {
                    if (CombatArea.IsPointInCombatArea(mousePos))
                    {
                        _targetEntity.Position = mousePos;
                    }

                    if (Input.LeftMouseButtonPressed)
                    {
                        HandlePreparationFinished(Position);
                    }
                }
            }
        }

        public override void HandlePreparationCanceled()
        {
            base.HandlePreparationCanceled();

            _targetEntity.Destroy();
        }
    }
}
