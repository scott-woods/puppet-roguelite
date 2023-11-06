using Microsoft.Xna.Framework;
using Nez;
using PuppetRoguelite.Components.Characters.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.PlayerActions.Support
{
    [PlayerActionInfo("Healing Aura", 3, PlayerActionCategory.Support)]
    public class HealingAura : PlayerAction
    {
        public HealingAura(Action<PlayerAction, Vector2> actionPrepFinishedHandler, Action<PlayerAction> actionPrepCanceledHandler, Action<PlayerAction> executionFinishedHandler) : base(actionPrepFinishedHandler, actionPrepCanceledHandler, executionFinishedHandler)
        {
        }

        public override void Prepare()
        {
            base.Prepare();

            HandlePreparationFinished(Position);
        }

        public override void Execute()
        {
            base.Execute();

            Game1.StartCoroutine(ExecutionCoroutine());
        }

        IEnumerator ExecutionCoroutine()
        {
            //enable player
            PlayerController.Instance.Entity.SetEnabled(true);
            PlayerController.Instance.Entity.SetPosition(Position);

            yield return Coroutine.WaitForSeconds(.1f);

            //heal player
            PlayerController.Instance.HealthComponent.Health += 2;
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Heal);

            //briefly change color
            PlayerController.Instance.SpriteAnimator.SetColor(Color.Green);
            yield return Coroutine.WaitForSeconds(.2f);
            PlayerController.Instance.SpriteAnimator.SetColor(Color.White);

            yield return Coroutine.WaitForSeconds(.1f);

            PlayerController.Instance.Entity.SetEnabled(false);

            HandleExecutionFinished();
        }
    }
}
