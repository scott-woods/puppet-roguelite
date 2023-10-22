using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.SceneComponents;
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

        public void TryTriggerTurn()
        {
            var gameStateManager = _context.Entity.Scene.GetOrCreateSceneComponent<GameStateManager>();
            if (gameStateManager.GameState == GameState.Combat && _context.ActionInput.IsPressed)
            {
                _machine.ChangeState<TurnState>();
                Emitters.CombatEventsEmitter.Emit(CombatEvents.TurnPhaseTriggered);
            }
        }

        public void TryDash()
        {
            if (!_context.Dash.IsOnCooldown && _context.DashInput.IsPressed)
            {
                _machine.ChangeState<DashState>();
            }
        }

        public void TryMove()
        {
            if (_context.XAxisInput.Value != 0 || _context.YAxisInput.Value != 0)
            {
                _machine.ChangeState<MoveState>();
            }
        }

        public void TryMelee()
        {
            if (Input.LeftMouseButtonPressed)
            {
                _machine.ChangeState<AttackState>();
            }
        }

        public void TryIdle()
        {
            if (_context.XAxisInput.Value == 0 && _context.YAxisInput.Value == 0)
            {
                _machine.ChangeState<IdleState>();
            }
        }

        public void TryCheck()
        {
            var gameStateManager = _context.Entity.Scene.GetOrCreateSceneComponent<GameStateManager>();
            if (gameStateManager.GameState == GameState.Exploration && _context.CheckInput.IsPressed)
            {
                var raycastHit = Physics.Linecast(_context.Entity.Position, _context.Entity.Position + _context.VelocityComponent.Direction * _context.RaycastDistance);

                if (raycastHit.Collider != null)
                {
                    if (raycastHit.Collider.Entity.TryGetComponent<Interactable>(out var interactable))
                    {
                        _machine.ChangeState<CutsceneState>();
                        interactable.Interact(() => _machine.ChangeState<IdleState>());
                    }
                }
                else
                {
                    var overlap = Physics.OverlapRectangle(new RectangleF(_context.Entity.Position, new Vector2(16, 16)));
                    if (overlap != null)
                    {
                        if (overlap.Entity.TryGetComponent<Interactable>(out var interactable))
                        {
                            _machine.ChangeState<CutsceneState>();
                            interactable.Interact(() => _machine.ChangeState<IdleState>());
                        }
                    }
                }
            }
        }
    }
}
