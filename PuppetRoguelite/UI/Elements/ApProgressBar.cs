using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.PlayerActions;
using PuppetRoguelite.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ApProgressBar : ProgressBar
    {
        int _index;
        float _min;
        float _max;

        ActionPointComponent _apComponent;

        public ApProgressBar(ActionPointComponent actionPointComponent, int index, Skin skin, string styleName = null) : base(skin, styleName)
        {
            _index = index;
            _apComponent = actionPointComponent;

            _min = _apComponent.ApThreshold * _index;
            _max = _apComponent.ApThreshold * (_index + 1);
            SetMinMax(_min, _max);

            SetStepSize(.1f);

            AddObservers();
        }

        void AddObservers()
        {
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsChargeTimerChanged);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        void OnActionPointsChargeTimerChanged(ActionPointComponent actionPointComponent)
        {
            SetValue(_apComponent.CurrentChargeTimer);
        }

        void OnTurnPhaseTriggered()
        {
            if (_index == _apComponent.ActionPoints)
            {
                Value = 0;
            }
        }

        void OnTurnPhaseCompleted()
        {
            Value = 0;
        }

        void OnActionPointsChanged(ActionPointComponent actionPointComponent)
        {
            if (actionPointComponent.ActionPoints <= _index)
            {
                SetValue(0);
            }
        }
    }
}
