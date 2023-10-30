﻿using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class DyingState : State<PlayerController>
    {
        public override void Begin()
        {
            base.Begin();

            Emitters.PlayerEventsEmitter.Emit(PlayerEvents.PlayerDied);

            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._69_Die_02, 1.1f);

            var deathAnimation = _context.VelocityComponent.Direction.X >= 0 ? "DeathRight" : "DeathLeft";
            _context.SpriteAnimator.Play(deathAnimation, SpriteAnimator.LoopMode.Once);
            _context.SpriteAnimator.OnAnimationCompletedEvent += HandleAnimationCompleted;
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            _context.SpriteAnimator.OnAnimationCompletedEvent -= HandleAnimationCompleted;
        }

        void HandleAnimationCompleted(string animationName)
        {
            if (animationName == "DeathRight" || animationName == "DeathLeft")
            {
                _context.Entity.Destroy();
            }
        }
    }
}