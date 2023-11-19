using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class BasicApProgressBar : ProgressBar
    {
        public BasicApProgressBar(Skin skin, string styleName = null) : base(skin, styleName)
        {
            SetMinMax(0, 1f);

            SetStepSize(.01f);
        }

        //public void AddObservers()
        //{
        //    Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
        //    Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsChargeTimerChanged);

        //    Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
        //    Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        //}

        //public void RemoveObservers()
        //{
        //    Emitters.ActionPointEmitter.RemoveObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
        //    Emitters.ActionPointEmitter.RemoveObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsChargeTimerChanged);

        //    Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
        //    Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        //}

        //void UpdateStyle()
        //{
        //    int num = 0;
        //    if (PlayerController.Instance.ActionPointComponent.CurrentChargeTimer > Min)
        //    {
        //        var percentage = (PlayerController.Instance.ActionPointComponent.CurrentChargeTimer - Min) / (Max - Min);
        //        num = Math.Min(((int)(percentage * 10)), 9);
        //    }

        //    var style = $"progressBar_{num}";
        //    SetStyle(_skin.Get<ProgressBarStyle>(style));
        //}

        //void OnActionPointsChargeTimerChanged(ActionPointComponent actionPointComponent)
        //{
        //    //UpdateStyle();
        //    SetValue(actionPointComponent.CurrentChargeTimer);
        //}

        //void OnTurnPhaseTriggered()
        //{
        //    if (_index >= PlayerController.Instance.ActionPointComponent.ActionPoints)
        //    {
        //        Value = 0;
        //    }
        //}

        //void OnTurnPhaseCompleted()
        //{
        //    Value = 0;
        //}

        //void OnActionPointsChanged(ActionPointComponent actionPointComponent)
        //{
        //    if (actionPointComponent.ActionPoints <= _index)
        //    {
        //        SetValue(0);
        //    }
        //}
    }
}
