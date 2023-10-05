﻿using Nez;
using Nez.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class HealthComponent : Component
    {
        public Emitter<HealthComponentEventType, HealthComponent> Emitter;

        Hurtbox _hurtbox;

        int _health;
        public int Health
        {
            get => _health;
            set
            {
                //if new health is less, emit damage taken signal
                if (_health > value)
                {
                    Emitter.Emit(HealthComponentEventType.DamageTaken, this);
                }
                else if (_health < value) //if we gained health, emit HealthGained
                {
                    Emitter.Emit(HealthComponentEventType.HealthGained, this);
                }
                
                //update health value
                _health = value;

                //emit health changed signal to update ui
                Emitter.Emit(HealthComponentEventType.HealthChanged, this);

                //emit health depleted if health is 0 or less
                if (_health <= 0)
                {
                    Emitter.Emit(HealthComponentEventType.HealthDepleted, this);
                }
            }
        }

        int _maxHealth;
        public int MaxHealth
        {
            get => _maxHealth;
        }

        public HealthComponent(int health, int maxHealth)
        {
            _health = health;
            _maxHealth = maxHealth;
        }

        public override void Initialize()
        {
            base.Initialize();

            Emitter = new Emitter<HealthComponentEventType, HealthComponent>();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //connect to hurtbox hit event if one exists on the entity
            _hurtbox = Entity.GetComponent<Hurtbox>();
            if (_hurtbox != null )
            {
                _hurtbox.Emitter.AddObserver(HurtboxEventTypes.Hit, OnHurtboxHit);
            }
        }

        void OnHurtboxHit(Hitbox hitbox)
        {
            Health -= hitbox.Damage;
        }

        //public int DecrementHealth(int amount)
        //{
        //    Health -= amount;
        //    return Health;
        //}

        ///// <summary>
        ///// returns true if health is 0 or less
        ///// </summary>
        ///// <returns></returns>
        //public bool IsDepleted()
        //{
        //    return _health <= 0;
        //}
    }

    public enum HealthComponentEventType
    {
        HealthChanged,
        HealthDepleted,
        DamageTaken,
        HealthGained
    }
}