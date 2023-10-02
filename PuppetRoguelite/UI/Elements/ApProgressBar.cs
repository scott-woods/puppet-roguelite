using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
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
        public ApProgressBar(Skin skin, string styleName = null) : base(skin, styleName)
        {
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

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        void OnActionPointsTimerStarted(ActionPointComponent actionPointComponent)
        {
            SetMinMax(0, actionPointComponent.ChargeRate);
        }

        void OnActionPointsTimerUpdated(ActionPointComponent actionPointComponent)
        {
            SetValue(actionPointComponent.CurrentChargeTimer);
        }

        void OnTurnPhaseTriggered()
        {
            SetIsVisible(false);
        }

        void OnTurnPhaseCompleted()
        {
            SetIsVisible(true);
            Value = 0;
        }

        void OnDodgePhaseStarted()
        {
            //get initial action point values
            var actionPointComponent = Game1.Scene.FindComponentOfType<ActionPointComponent>();
            if (actionPointComponent != null)
            {
                OnActionPointsTimerStarted(actionPointComponent);
                OnActionPointsTimerUpdated(actionPointComponent);
            }
        }
    }
}
