using Nez;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class ComboManager
    {
        const int _extraPerCombo = 2;

        public int ComboCounter = 0;

        List<HealthComponent> _connectedHealthComponents = new List<HealthComponent>();
        List<DollahDropper> _connectedDollahDroppers = new List<DollahDropper>();

        public void StartCounting(List<Enemy> enemies)
        {
            ComboCounter = 0;

            foreach (var enemy in enemies)
            {
                if (enemy.Entity.TryGetComponent<HealthComponent>(out var hc))
                {
                    hc.Emitter.AddObserver(HealthComponentEventType.HealthDepleted, OnEnemyHealthDepleted);
                    _connectedHealthComponents.Add(hc);
                }
            }
        }

        public void Reset()
        {
            foreach (var hc in _connectedHealthComponents)
            {
                hc.Emitter.RemoveObserver(HealthComponentEventType.HealthDepleted, OnEnemyHealthDepleted);
            }

            _connectedHealthComponents.Clear();
            _connectedDollahDroppers.Clear();
        }

        void OnEnemyHealthDepleted(HealthComponent hc)
        {
            ComboCounter += 1;
            if (hc.Entity.TryGetComponent<DollahDropper>(out var dollahDropper))
            {
                _connectedDollahDroppers.Add(dollahDropper);
            }

            foreach (var dropper in _connectedDollahDroppers)
            {
                dropper.SetBonus(ComboCounter * _extraPerCombo);
            }
        }
    }
}
