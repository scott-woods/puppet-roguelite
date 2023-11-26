using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Support
{
    [PlayerActionInfo("Healing Aura", 3, PlayerActionCategory.Support)]
    public class HealingAura : PlayerAction
    {
        //components
        PlayerSim _playerSim;

        public override void Prepare()
        {
            base.Prepare();

            _playerSim = AddComponent(new PlayerSim());

            HandlePreparationFinished(Position);
        }

        public override void Execute()
        {
            base.Execute();

            Core.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            //get animator from player sim
            var animator = _playerSim.GetComponent<SpriteAnimator>();

            //play idle
            animator.Play("IdleRight");

            yield return Coroutine.WaitForSeconds(.1f);

            //heal player
            PlayerController.Instance.HealthComponent.Health += 2;
            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Heal);

            //briefly change color
            animator.SetColor(Color.Green);
            yield return Coroutine.WaitForSeconds(.2f);
            animator.SetColor(Color.White);

            yield return Coroutine.WaitForSeconds(.1f);

            HandleExecutionFinished();
        }
    }
}
