﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class AttackState : State<PlayerController>
    {
        public override void Begin()
        {
            base.Begin();

            _context.ExecuteMeleeAttack(AttackSequenceComplete);
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void End()
        {
            base.End();

            _context.MeleeAttack.CancelAttack();
        }

        void AttackSequenceComplete()
        {
            _machine.ChangeState<IdleState>();
        }
    }
}
