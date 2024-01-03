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
using Serilog;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Helpers;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks
{
    [PlayerActionInfo("Quickshot", 1, PlayerActionCategory.Attack, "Fire a fast-moving projectile, dealing heavy damage to a single enemy.")]
    public class Quickshot : PlayerAction
    {
        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _simulationLoop;
        ICoroutine _quickshotExecutionCoroutine;

        //components
        PlayerSim _playerSim;
        SpriteAnimator _animator;

        //gun
        Entity _gunEntity;
        PlayerGun1 _gun;

        //misc
        Vector2 _direction = Vector2.One;
        float _rotationSpeed = .025f;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim(true));

            //get animator from player sim
            _animator = _playerSim.GetComponent<SpriteAnimator>();
            _animator.SetColor(new Color(Color.White.R, Color.White.G, Color.White.B, 128));

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
                if (Controls.Instance.Confirm.IsPressed)
                {
                    Reset();
                    HandlePreparationFinished(FinalPosition);
                    return;
                }

                //idle in direction of mouse
                _playerSim.Idle(DirectionHelper.GetDirectionByVector2(_direction));

                //calculate direction by mouse position
                var dir = CalculateDirection();
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
            Log.Debug("Resetting Quickshot");

            //coroutines
            _simulationLoop?.Stop();
            _simulationLoop = null;
            _quickshotExecutionCoroutine?.Stop();
            _quickshotExecutionCoroutine = null;

            //gun
            _gunEntity?.Destroy();
            _gunEntity = null;
        }

        Vector2 CalculateDirection()
        {
            if (!Game1.InputStateManager.IsUsingGamepad)
            {
                return Core.Scene.Camera.MouseToWorldPoint() - _playerSim.OriginComponent.Origin;
            }
            else
            {
                float currentAngle = (float)Math.Atan2(_direction.Y, _direction.X);

                if (Controls.Instance.XAxisIntegerInput.Value > 0)
                    currentAngle += _rotationSpeed;
                else if (Controls.Instance.XAxisIntegerInput.Value < 0)
                    currentAngle -= _rotationSpeed;

                Vector2 newDirection = new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle));

                return newDirection;
            }
        }
    }
}
