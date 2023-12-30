﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.UI.Menus;
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

        public bool TryShowStats()
        {
            if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Tab))
            {
                _machine.ChangeState<CutsceneState>();

                var statsMenuEntity = Game1.Scene.CreateEntity("stats-menu");
                statsMenuEntity.AddComponent(new StatsMenu(OnStatsMenuClosed));

                return true;
            }

            return false;
        }

        public bool TryTriggerTurn()
        {
            var gameStateManager = Game1.GameStateManager;
            if ((gameStateManager.GameState == GameState.Combat || gameStateManager.GameState == GameState.AttackTutorial) && _context.ActionInput.IsPressed && _context.ActionPointComponent.ActionPoints > 0)
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
            if (_context.IsMeleeEnabled && Input.LeftMouseButtonPressed && Game1.GameStateManager.GameState != GameState.Paused)
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
            if (gameStateManager.GameState != GameState.Combat && _context.CheckInput.IsPressed)
            {
                var hitArray = new RaycastHit[10];
                var raycastHits = Physics.LinecastAll(_context.OriginComponent.Origin, _context.OriginComponent.Origin + _context.VelocityComponent.Direction * _context.RaycastDistance, hitArray, 1 << (int)PhysicsLayers.Interactable);

                foreach (var raycastHit in hitArray)
                {
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
                        var overlap = Physics.OverlapRectangle(new RectangleF(_context.Entity.Position, new Vector2(16, 16)), 1 << (int)PhysicsLayers.Interactable);
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
            }

            return false;
        }

        void HandleInteractionFinished()
        {
            Game1.Schedule(.1f, timer => _machine.ChangeState<IdleState>());
        }

        void OnStatsMenuClosed()
        {
            Game1.Schedule(.1f, timer => _machine.ChangeState<IdleState>());
        }
    }
}
