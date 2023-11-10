using Nez;
using Nez.Systems;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.Player
{
    public class ActionPointComponent : Component, IUpdatable
    {
        //const float _baseChargeRate = 1.25f;
        //const float _baseDamageMultiplier = 4f;
        const float _baseChargeRate = 0f;
        const float _baseDamageMultiplier = 4f;

        bool _isCharging = false;

        HealthComponent _healthComponent;

        public float MaxCharge = 100f;

        public float ApThreshold
        {
            get
            {
                return 100f / _maxActionPoints;
            }
        }

        int _maxActionPoints;
        public int MaxActionPoints
        {
            get => _maxActionPoints;
            set => _maxActionPoints = value;
        }

        int _actionPoints;
        public int ActionPoints
        {
            get => _actionPoints;
            set
            {
                _actionPoints = Math.Max(0, Math.Min(value, MaxActionPoints));
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsChanged, this);
            }
        }

        float _currentChargeTimer;
        public float CurrentChargeTimer
        {
            get => _currentChargeTimer;
            set
            {
                _currentChargeTimer = Math.Max(0, Math.Min(value, MaxCharge));
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsTimerUpdated, this);
            }
        }

        public ActionPointComponent(int maxAp, HealthComponent healthComponent)
        {
            MaxActionPoints = maxAp;
            _healthComponent = healthComponent;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            if (Entity.TryGetComponent<MeleeAttack>(out var meleeAttack))
            {
                meleeAttack.Emitter.AddObserver(MeleeAttackEvents.Hit, OnMeleeAttackHit);
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            if (Entity.TryGetComponent<MeleeAttack>(out var meleeAttack))
            {
                meleeAttack.Emitter.RemoveObserver(MeleeAttackEvents.Hit, OnMeleeAttackHit);
            }
        }

        public void Update()
        {
            if (_isCharging && (ActionPoints < MaxActionPoints))
            {
                float healthMultiplier = 2 - (_healthComponent.Health / _healthComponent.MaxHealth);
                CurrentChargeTimer += Time.DeltaTime * (_baseChargeRate * healthMultiplier);

                //if timer has finished
                if (CurrentChargeTimer >= ApThreshold * (ActionPoints + 1))
                {
                    ActionPoints++;
                }
            }
        }

        public void DecrementActionPoints(int actionPoints)
        {
            ActionPoints -= actionPoints;
        }

        #region OBSERVERS

        void OnDodgePhaseStarted()
        {
            ActionPoints = 0;
            _isCharging = true;
            CurrentChargeTimer = 0;
        }

        void OnTurnPhaseTriggered()
        {
            _isCharging = false;
        }

        void OnMeleeAttackHit(int damageAmount)
        {
            if (ActionPoints < MaxActionPoints)
            {
                CurrentChargeTimer += (_baseDamageMultiplier * damageAmount);
            }
        }

        #endregion
    }
}
