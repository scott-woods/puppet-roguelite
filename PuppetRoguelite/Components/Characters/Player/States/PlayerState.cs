using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.GlobalManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player.States
{
    public class PlayerState : State<PlayerController>
    {
        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public bool TryTriggerTurn()
        {
            var gameStateManager = Game1.GameStateManager;
            if (gameStateManager.GameState == GameState.Combat && _context.ActionInput.IsPressed && _context.ActionPointComponent.ActionPoints > 0)
            {
                _machine.ChangeState<TurnState>();
                Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseTriggered);
                return true;
            }

            return false;
        }

        public bool TryDash()
        {
            if (!_context.Dash.IsOnCooldown && _context.DashInput.IsPressed)
            {
                _machine.ChangeState<DashState>();
                return true;
            }

            return false;
        }

        public bool TryMove()
        {
            if (_context.XAxisInput.Value != 0 || _context.YAxisInput.Value != 0)
            {
                _machine.ChangeState<MoveState>();
                return true;
            }

            return false;
        }

        public bool TryMelee()
        {
            if (Input.LeftMouseButtonPressed)
            {
                _machine.ChangeState<AttackState>();
                return true;
            }

            return false;
        }

        public bool TryIdle()
        {
            if (_context.XAxisInput.Value == 0 && _context.YAxisInput.Value == 0)
            {
                _machine.ChangeState<IdleState>();
                return true;
            }

            return false;
        }

        public bool TryCheck()
        {
            var gameStateManager = Game1.GameStateManager;
            if (gameStateManager.GameState == GameState.Exploration && _context.CheckInput.IsPressed)
            {
                var raycastHit = Physics.Linecast(_context.Entity.Position, _context.Entity.Position + _context.VelocityComponent.Direction * _context.RaycastDistance);

                if (raycastHit.Collider != null)
                {
                    if (raycastHit.Collider.Entity.TryGetComponent<Interactable>(out var interactable))
                    {
                        if (interactable.Active)
                        {
                            _machine.ChangeState<CutsceneState>();
                            interactable.Interact(HandleInteractionFinished);
                            return true;
                        }
                    }
                }
                else
                {
                    var overlap = Physics.OverlapRectangle(new RectangleF(_context.Entity.Position, new Vector2(16, 16)));
                    if (overlap != null)
                    {
                        if (overlap.Entity.TryGetComponent<Interactable>(out var interactable))
                        {
                            if (interactable.Active)
                            {
                                _machine.ChangeState<CutsceneState>();
                                interactable.Interact(HandleInteractionFinished);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        void HandleInteractionFinished()
        {
            Game1.Schedule(.1f, timer => _machine.ChangeState<IdleState>());
        }
    }
}
