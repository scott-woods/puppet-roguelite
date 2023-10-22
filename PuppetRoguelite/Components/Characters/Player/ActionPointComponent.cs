using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.PlayerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class ActionPointComponent : Component, IUpdatable
    {
        int _maxActionPoints;
        int _actionPoints;
        float _totalChargeTime;
        float _currentChargeTimer;
        bool _active = false;

        public int MaxActionPoints
        {
            get => _maxActionPoints;
            set
            {
                _maxActionPoints = value;
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.MaxActionPointsChanged, this);
            }
        }
        public int ActionPoints
        {
            get => _actionPoints;
            set
            {
                _actionPoints = Math.Max(0, Math.Min(value, MaxActionPoints));
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsChanged, this);
            }
        }
        public float ChargeRate;
        public float CurrentChargeTimer
        {
            get => _currentChargeTimer;
            set
            {
                _currentChargeTimer = value;
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsTimerUpdated, this);
            }
        }

        public ActionPointComponent(int maxActionPoints, float totalChargeTime)
        {
            MaxActionPoints = maxActionPoints;
            _totalChargeTime = totalChargeTime;
            ChargeRate = _totalChargeTime / MaxActionPoints;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionFinishedPreparing, OnPlayerActionFinishedPreparing);
        }

        public void Update()
        {
            if (_active)
            {
                if (ActionPoints < MaxActionPoints)
                {
                    //increment timer
                    CurrentChargeTimer += Time.DeltaTime;

                    //if timer has finished
                    if (CurrentChargeTimer >= ChargeRate)
                    {
                        ActionPoints++;

                        //if we haven't hit the max ap
                        if (ActionPoints < MaxActionPoints)
                        {
                            StartTimer();
                        }
                    }
                }
            }
        }

        void StartTimer()
        {
            CurrentChargeTimer = 0;
            Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsTimerStarted, this);
        }

        /// <summary>
        /// decrement ap when an action is successfully prepared
        /// </summary>
        /// <param name="action"></param>
        void OnPlayerActionFinishedPreparing(IPlayerAction action)
        {
            ActionPoints -= PlayerActionUtils.GetApCost(action.GetType());
        }

        void OnDodgePhaseStarted()
        {
            ActionPoints = 0;
            StartTimer();
            _active = true;
        }

        void OnTurnPhaseTriggered()
        {
            CurrentChargeTimer = 0;
            _active = false;
        }
    }
}
