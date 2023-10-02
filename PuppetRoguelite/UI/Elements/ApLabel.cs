using Nez;
using Nez.UI;
using PuppetRoguelite.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class ApLabel : Label
    {
        public ApLabel(string text, Skin skin, string styleName = null) : base(text, skin, styleName)
        {
            Setup();
            AddObservers();
        }

        void Setup()
        {

        }

        void AddObservers()
        {
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.MaxActionPointsChanged, OnMaxActionPointsChanged);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        void OnActionPointsChanged(ActionPointComponent actionPointComponent)
        {
            var text = GetText();
            var slashIndex = text.IndexOf("/");
            var newString = actionPointComponent.ActionPoints.ToString() + text.Substring(slashIndex);
            SetText(newString);
        }

        void OnMaxActionPointsChanged(ActionPointComponent actionPointComponent)
        {
            var text = GetText();
            var slashIndex = text.IndexOf("/");
            var newString = text.Substring(0, slashIndex + 1) + actionPointComponent.MaxActionPoints.ToString();
            SetText(newString);
        }

        void OnDodgePhaseStarted()
        {
            //get initial action point values
            var actionPointComponent = Game1.Scene.FindComponentOfType<ActionPointComponent>();
            if (actionPointComponent != null)
            {
                OnActionPointsChanged(actionPointComponent);
                OnMaxActionPointsChanged(actionPointComponent);
            }
        }
    }
}
