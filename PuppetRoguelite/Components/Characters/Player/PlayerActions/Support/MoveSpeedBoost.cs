﻿using Nez.Sprites;
using Nez;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Components.Characters.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez.Systems;
using Microsoft.Xna.Framework;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;

namespace PuppetRoguelite.Components.Characters.Player.PlayerActions.Support
{
    [PlayerActionInfo("Haste", 1, PlayerActionCategory.Support, "Greatly increases Move Speed for a short time.")]
    public class MoveSpeedBoost : PlayerAction
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
            Game1.AudioManager.PlaySound(Content.Audio.Sounds.Haste);

            //briefly change color
            animator.SetColor(Color.Blue);
            yield return Coroutine.WaitForSeconds(.2f);
            animator.SetColor(Color.White);

            //apply speed buff
            PlayerController.Instance.MoveSpeed *= 1.35f;

            //apply sprite trail
            var trail = PlayerController.Instance.Entity.AddComponent(new SpriteTrail(PlayerController.Instance.SpriteAnimator));
            trail.FadeDuration = .1f;
            trail.MinDistanceBetweenInstances = 16;
            trail.InitialColor = new Color(Color.White.R, Color.White.G, Color.White.B, 32);

            //after 7 seconds, remove buff
            Core.Schedule(7f, timer =>
            {
                Game1.AudioManager.PlaySound(Content.Audio.Sounds.Haste_end);
                PlayerController.Instance.MoveSpeed = PlayerData.Instance.MovementSpeed;
                PlayerController.Instance.Entity.RemoveComponent(trail);
            });

            yield return Coroutine.WaitForSeconds(.1f);

            HandleExecutionFinished();
        }

        public override void Reset()
        {

        }
    }
}
