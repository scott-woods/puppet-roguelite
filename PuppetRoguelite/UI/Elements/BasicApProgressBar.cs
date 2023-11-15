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
        int _index;
        Skin _skin;
        float _min, _max;

        public BasicApProgressBar(int index, Skin skin, string styleName = null) : base(skin, styleName)
        {
            _index = index;
            _skin = skin;

            _min = PlayerController.Instance.ActionPointComponent.ApThreshold * _index;
            _max = PlayerController.Instance.ActionPointComponent.ApThreshold * (_index + 1);
            SetMinMax(_min, _max);

            SetStepSize(.1f);
        }

        public void AddObservers()
        {
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsChargeTimerChanged);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

        public void RemoveObservers()
        {
            Emitters.ActionPointEmitter.RemoveObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
            Emitters.ActionPointEmitter.RemoveObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsChargeTimerChanged);

            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
        }

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

        void OnActionPointsChargeTimerChanged(ActionPointComponent actionPointComponent)
        {
            //UpdateStyle();
            SetValue(actionPointComponent.CurrentChargeTimer);
        }

        void OnTurnPhaseTriggered()
        {
            if (_index >= PlayerController.Instance.ActionPointComponent.ActionPoints)
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
