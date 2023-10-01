using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components
{
    public class ActionPointComponent : Component, IUpdatable
    {
        public Emitter<ActionPointEventType, int> ActionPointEmitter;
        public Emitter<ActionPointEventType, float> ActionPointTimerEmitter;

        int _maxActionPoints;
        public int MaxActionPoints { get => _maxActionPoints; }
        int _actionPoints = 0;
        public int ActionPoints { get => _actionPoints; }
        float _totalChargeTime;
        float _chargeRate;
        float _currentChargeTimer = 0;
        bool _active = false;

        public ActionPointComponent(int maxActionPoints, float totalChargeTime)
        {
            ActionPointEmitter = new Emitter<ActionPointEventType, int>();
            ActionPointTimerEmitter = new Emitter<ActionPointEventType, float>();

            _maxActionPoints = maxActionPoints;
            _totalChargeTime = totalChargeTime;
            _chargeRate = _totalChargeTime / _maxActionPoints;

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.PlayerActionEmitter.AddObserver(PlayerActionEvents.ActionExecuting, OnPlayerActionExecuting);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            ActionPointEmitter.Emit(ActionPointEventType.ActionPointGained, _actionPoints);
            ActionPointEmitter.Emit(ActionPointEventType.MaxActionPointsChanged, _maxActionPoints);
        }

        public void Update()
        {
            if (_active)
            {
                if (_actionPoints < _maxActionPoints)
                {
                    //increment timer and emit
                    _currentChargeTimer += Time.DeltaTime;
                    ActionPointTimerEmitter.Emit(ActionPointEventType.TimeChanged, _currentChargeTimer);

                    if (_currentChargeTimer >= _chargeRate)
                    {
                        _actionPoints++;
                        ActionPointEmitter.Emit(ActionPointEventType.ActionPointGained, _actionPoints);
                        if (_actionPoints < _maxActionPoints)
                        {
                            _currentChargeTimer = 0;
                            ActionPointTimerEmitter.Emit(ActionPointEventType.TimeChanged, _currentChargeTimer);
                            ActionPointTimerEmitter.Emit(ActionPointEventType.TimerStarted, _chargeRate);
                        }
                    }
                }
            }
        }

        public void Charge()
        {
            ActionPointTimerEmitter.Emit(ActionPointEventType.TimerStarted, _chargeRate);
            _active = true;
        }

        public void StopCharging()
        {
            _active = false;
        }

        void OnPlayerActionExecuting(IPlayerAction action)
        {
            _actionPoints -= PlayerActionUtils.GetApCost(action.GetType());
            _actionPoints = _actionPoints < 0 ? 0 : _actionPoints;
            ActionPointEmitter.Emit(ActionPointEventType.ActionPointGained, _actionPoints);
        }

        void OnTurnPhaseCompleted()
        {
            _actionPoints = 0;
            _currentChargeTimer = 0;
            ActionPointEmitter.Emit(ActionPointEventType.ActionPointGained, _actionPoints);
            ActionPointTimerEmitter.Emit(ActionPointEventType.TimeChanged, _currentChargeTimer);
        }
    }

    public enum ActionPointEventType
    {
        ActionPointGained = 1,
        TimeChanged = 2,
        TimerStarted = 3,
        MaxActionPointsChanged = 4
    }
}
