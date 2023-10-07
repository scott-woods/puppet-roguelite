using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
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

        public ApProgressBar(int index, Skin skin, string styleName = null) : base(skin, styleName)
        {
            _index = index;

            Setup();
            AddObservers();
        }

        void Setup()
        {
            SetStepSize(.1f);
        }

        void AddObservers()
        {
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerStarted, OnActionPointsTimerStarted);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsTimerUpdated);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        void OnActionPointsTimerStarted(ActionPointComponent actionPointComponent)
        {
            if (actionPointComponent.ActionPoints == _index)
            {
                SetMinMax(0, actionPointComponent.ChargeRate);
            }
        }

        void OnActionPointsTimerUpdated(ActionPointComponent actionPointComponent)
        {
            if (actionPointComponent.ActionPoints == _index)
            {
                SetValue(actionPointComponent.CurrentChargeTimer);
            }
        }

        void OnTurnPhaseTriggered()
        {
            //SetIsVisible(false);
            var actionPointComponent = Game1.Scene.FindComponentOfType<ActionPointComponent>();
            if (actionPointComponent != null)
            {
                if (_index == actionPointComponent.ActionPoints)
                {
                    Value = 0;
                }
            }
        }

        void OnTurnPhaseCompleted()
        {
            //SetIsVisible(true);
            Value = 0;
        }

        void OnDodgePhaseStarted()
        {
            //get initial action point values
            var actionPointComponent = Game1.Scene.FindComponentOfType<ActionPointComponent>();
            if (actionPointComponent != null)
            {
                SetMinMax(0, actionPointComponent.ChargeRate);
                SetValue(actionPointComponent.CurrentChargeTimer);
            }
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
