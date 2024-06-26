﻿using Nez;
using PuppetRoguelite.Components.Shared;
using System;

namespace PuppetRoguelite.Components.Characters.Player.PlayerComponents
{
    public class ActionPointComponent : Component, IUpdatable
    {
        //const float _baseChargeRate = 1.25f;
        //const float _baseDamageMultiplier = 4f;
        const float _baseChargeRate = 0f;
        const float _baseDamageMultiplier = 4f;
        const int _damageRequiredPerPoint = 12;

        int _maxDamagePerHit = 3;
        int _minDamagePerHit = 1;
        int _currentDamagePerHit = 3;

        public int DamageRequiredPerPoint { get => _damageRequiredPerPoint; }

        int _damageAccumulated = 0;
        public int DamageAccumulated { get => _damageAccumulated; }

        bool _isCharging = false;

        HealthComponent _healthComponent;

        //public float MaxCharge = 100f;

        //public float ApThreshold
        //{
        //    get
        //    {
        //        return 100f / _maxActionPoints;
        //    }
        //}

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
                var newApValue = Math.Max(0, Math.Min(value, MaxActionPoints));

                if (newApValue == MaxActionPoints)
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Ap_full);
                else if (newApValue > _actionPoints)
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Ap_gain);

                _actionPoints = newApValue;

                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsChanged, this);
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsProgressChanged, this);
            }
        }

        //float _currentChargeTimer;
        //public float CurrentChargeTimer
        //{
        //    get => _currentChargeTimer;
        //    set
        //    {
        //        _currentChargeTimer = Math.Max(0, Math.Min(value, MaxCharge));
        //        Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsTimerUpdated, this);
        //    }
        //}

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
            //if (_isCharging && (ActionPoints < MaxActionPoints))
            //{
            //    float healthMultiplier = 2 - (_healthComponent.Health / _healthComponent.MaxHealth);
            //    CurrentChargeTimer += Time.DeltaTime * (_baseChargeRate * healthMultiplier);

            //    //if timer has finished
            //    if (CurrentChargeTimer >= ApThreshold * (ActionPoints + 1))
            //    {
            //        ActionPoints++;
            //    }
            //}
        }

        public void DecrementActionPoints(int actionPoints)
        {
            ActionPoints -= actionPoints;
            Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsProgressChanged, this);
        }

        #region OBSERVERS

        void OnTurnPhaseTriggered()
        {
            _damageAccumulated = 0;
            Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsProgressChanged, this);
        }

        void OnDodgePhaseStarted()
        {
            ActionPoints = 0;
            _damageAccumulated = 0;
            Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsProgressChanged, this);
        }

        void OnMeleeAttackHit(int damageAmount)
        {
            _damageAccumulated += _currentDamagePerHit;
            var newDamagePerHit = Math.Max(_minDamagePerHit, _currentDamagePerHit - 1);
            _currentDamagePerHit = newDamagePerHit;
            Core.Schedule(.5f, timer =>
            {
                var res = Math.Clamp(_currentDamagePerHit + 1, _minDamagePerHit, _maxDamagePerHit);
                _currentDamagePerHit = res;
            });

            if (_damageAccumulated >= _damageRequiredPerPoint && ActionPoints < MaxActionPoints)
            {
                _damageAccumulated -= _damageRequiredPerPoint;
                ActionPoints += 1;
            }
            else
            {
                //only emit here if AP didn't change, otherwise it will be emit from the ActionPoints setter
                Emitters.ActionPointEmitter.Emit(ActionPointEvents.ActionPointsProgressChanged, this);
            }
        }

        #endregion
    }
}
