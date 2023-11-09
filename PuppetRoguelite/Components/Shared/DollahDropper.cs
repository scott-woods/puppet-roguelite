using Nez;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Shared
{
    public class DollahDropper : Component
    {
        int _baseAmount = 0;
        int _variation = 0;
        int _bonus = 0;

        public DollahDropper(int baseAmount, int variation)
        {
            _baseAmount = baseAmount;
            _variation = variation;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<DeathComponent>(out var dc))
            {
                dc.OnDeathStarted += OnDeathStarted;
            }
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();

            if (Entity.TryGetComponent<DeathComponent>(out var dc))
            {
                dc.OnDeathStarted -= OnDeathStarted;
            }
        }

        public void DropDollahs()
        {
            var amount = _baseAmount + Nez.Random.Range(-_variation, _variation + 1);
            amount += _bonus;

            for (int i = 0; i < amount; i++)
            {
                var dollah = Entity.Scene.CreateEntity("dollah");
                var dollahComp = dollah.AddComponent(new Dollah());
                dollah.SetPosition(Entity.Position);
                dollahComp.Launch();
            }
        }

        public void SetBonus(int bonus)
        {
            _bonus = bonus;
        }

        void OnDeathStarted(Entity entity)
        {
            if (Entity.Scene.GetType() != typeof(BossRoom))
            {
                DropDollahs();
            }
        }
    }
}
