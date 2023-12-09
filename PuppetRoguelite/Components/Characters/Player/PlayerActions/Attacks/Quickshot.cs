using Nez;
using Nez.Sprites;
using Nez.Systems;
using PuppetRoguelite.Components.Shared.Hitboxes;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez.UI;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    [PlayerActionInfo("Quickshot", 2, PlayerActionCategory.Attack)]
    public class Quickshot : PlayerAction
    {
        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop;
        ICoroutine _quickshotExecutionCoroutine;

        //components
        PlayerSim _playerSim;
        SpriteAnimator _animator;
        DirectionByMouse _dirByMouse;

        //gun
        Entity _gunEntity;
        PlayerGun1 _gun;

        //misc
        Vector2 _direction = Vector2.One;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim(true));

            //get animator from player sim
            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));

            _dirByMouse = AddComponent(new DirectionByMouse());

            _simulationLoop = _coroutineManager.StartCoroutine(SimulationLoop());
        }

        public override void Execute()
        {
            base.Execute();

            _quickshotExecutionCoroutine = _coroutineManager.StartCoroutine(QuickshotExecutionCoroutine());
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
                    Reset();
                    HandlePreparationFinished(FinalPosition);
                    return;
                }

                //idle in direction of mouse
                _playerSim.Idle(_dirByMouse.CurrentDirection);

                //calculate direction by mouse position
                var dir = Core.Scene.Camera.MouseToWorldPoint() - _playerSim.OriginComponent.Origin;
                dir.Normalize();

                _gun.SetDirection(dir);

                //save direction
                _direction = dir;
            }
        }

        IEnumerator SimulationLoop()
        {
            while (State == PlayerActionState.Preparing)
            {
                //create gun
                if (_gunEntity == null)
                {
                    _gunEntity = Scene.CreateEntity("gun");
                    _gunEntity.SetPosition(Position);

                    _gun = _gunEntity.AddComponent(new PlayerGun1(_playerSim, _direction, true));
                }

                //wait for a moment
                yield return Coroutine.WaitForSeconds(1f);

                //fire
                _gun.Fire();

                //idle at end point for a moment
                yield return Coroutine.WaitForSeconds(1f);
            }
        }

        IEnumerator QuickshotExecutionCoroutine()
        {
            //create gun
            if (_gunEntity == null)
            {
                _gunEntity = Scene.CreateEntity("gun");
                _gunEntity.SetPosition(Position);

                _gun = _gunEntity.AddComponent(new PlayerGun1(_playerSim, _direction, false));
            }

            //gun draw sound
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Quickshot_draw);

            //wait a moment
            yield return Coroutine.WaitForSeconds(.3f);

            //fire
            _gun.Fire();

            //fire sound
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Quickshot_fire);

            //wait a moment
            yield return Coroutine.WaitForSeconds(.1f);

            HandleExecutionFinished();
        }

        public override void Reset()
        {
            //coroutines
            _simulationLoop?.Stop();
            _simulationLoop = null;
            _quickshotExecutionCoroutine?.Stop();
            _quickshotExecutionCoroutine = null;

            //gun
            _gunEntity?.Destroy();
            _gunEntity = null;
        }
    }
}
