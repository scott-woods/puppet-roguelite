using Nez.Sprites;
using Nez.Systems;
using Nez;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Support
{
    [PlayerActionInfo("Swiftstrike", 2, PlayerActionCategory.Support)]
    public class AttackSpeedBoost : PlayerAction
    {
        //components
        PlayerSim _playerSim;

        //coroutines
        CoroutineManager _coroutineManager = new CoroutineManager();
        ICoroutine _executionCoroutine;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim());

            HandlePreparationFinished(Position);
        }

        public override void Execute()
        {
            base.Execute();

            _executionCoroutine = Core.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            //get animator from player sim
            var animator = _playerSim.GetComponent<SpriteAnimator>();

            _playerSim.Idle(Direction.Down);

            yield return Coroutine.WaitForSeconds(.1f);

            //play sound
            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Attack_speed_boost);

            //briefly change color
            animator.SetColor(Color.Red);
            yield return Coroutine.WaitForSeconds(.2f);
            animator.SetColor(Color.White);

            //apply speed buff
            PlayerController.Instance.MeleeAttack.Speed *= 1.6f;

            //after 7 seconds, remove buff
            Core.Schedule(7f, timer =>
            {
                Game1.AudioManager.PlaySound(Content.Audio.Sounds.Attack_speed_boost_end);
                PlayerController.Instance.MeleeAttack.Speed = 1;
            });

            yield return Coroutine.WaitForSeconds(.1f);

            HandleExecutionFinished();
        }

        public override void Reset()
        {

        }
    }
}
