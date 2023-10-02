using Nez;
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

        int _health;
        public int Health
        {
            get => _health;
            set
            {
                _health = value;
                Emitter.Emit(HealthComponentEventType.HealthChanged, this);
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

        public int DecrementHealth(int amount)
        {
            Health -= amount;
            return Health;
        }

        /// <summary>
        /// returns true if health is 0 or less
        /// </summary>
        /// <returns></returns>
        public bool IsDepleted()
        {
            return _health <= 0;
        }
    }

    public enum HealthComponentEventType
    {
        HealthChanged,
        HealthDepleted
    }
}
