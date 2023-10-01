using Nez;
using Nez.Sprites;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI
{
    public class CombatUI : UICanvas
    {
        //elements
        Table _table;
        Label _playerHealthLabel;
        ProgressBar _apProgressBar;
        Label _playerApLabel;

        //skins
        Skin _basicSkin;

        public override void Initialize()
        {
            base.Initialize();

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);
            _table.SetDebug(false);

            //load skin
            _basicSkin = Skin.CreateDefaultSkin();

            //arrange elements
            ArrangeElements();

            ConnectToGlobalEmitters();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            ConnectToEmitters();
        }

        void ArrangeElements()
        {
            //status section in top left
            var statusTable = new Table();
            statusTable.Defaults().Left().SetSpaceBottom(5);
            _table.Add(statusTable).Expand().Top().Left().SetPadTop(5).SetPadLeft(5);
            _playerHealthLabel = new Label("HP: ", _basicSkin);
            statusTable.Add(_playerHealthLabel);
            statusTable.Row();
            statusTable.Add(new Label("$", _basicSkin));
            statusTable.Row();
            statusTable.Add(new Label("&", _basicSkin));

            //ap table
            _table.Row();
            var apTable = new Table();
            apTable.Defaults().Center();
            _table.Add(apTable).Expand().Bottom().SetPadBottom(10);
            _playerApLabel = new Label("/", _basicSkin);
            apTable.Add(_playerApLabel).SetSpaceBottom(5);
            apTable.Row();
            _apProgressBar = new ProgressBar(_basicSkin);
            _apProgressBar.SetStepSize(.1f);
            apTable.Add(_apProgressBar);
        }

        void ConnectToEmitters()
        {
            //player health
            var playerHealthComponent = Entity.Scene.FindComponentsOfType<HealthComponent>().FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
                playerHealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            }
        }

        void ConnectToGlobalEmitters()
        {
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerStarted, OnActionPointsTimerStarted);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsTimerUpdated, OnActionPointsTimerUpdated);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsChanged, OnActionPointsChanged);
            Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.MaxActionPointsChanged, OnMaxActionPointsChanged);

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        void OnActionPointsTimerStarted(ActionPointComponent actionPointComponent)
        {
            _apProgressBar.SetMinMax(0, actionPointComponent.ChargeRate);
        }

        void OnActionPointsTimerUpdated(ActionPointComponent actionPointComponent)
        {
            _apProgressBar.SetValue(actionPointComponent.CurrentChargeTimer);
        }

        void OnActionPointsChanged(ActionPointComponent actionPointComponent)
        {
            var text = _playerApLabel.GetText();
            var slashIndex = text.IndexOf("/");
            var newString = actionPointComponent.ActionPoints.ToString() + text.Substring(slashIndex);
            _playerApLabel.SetText(newString);
        }

        void OnMaxActionPointsChanged(ActionPointComponent actionPointComponent)
        {
            var text = _playerApLabel.GetText();
            var slashIndex = text.IndexOf("/");
            var newString = text.Substring(0, slashIndex + 1) + actionPointComponent.MaxActionPoints.ToString();
            _playerApLabel.SetText(newString);
        }

        void OnTurnPhaseTriggered()
        {
            _apProgressBar.SetIsVisible(false);
        }

        void OnTurnPhaseCompleted()
        {
            _apProgressBar.SetIsVisible(true);
            _apProgressBar.Value = 0;
        }

        void OnDodgePhaseStarted()
        {
            //get initial hp values
            var playerHealthComponent = Entity.Scene.FindComponentsOfType<HealthComponent>().FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
            }

            //get initial action point values
            var actionPointComponent = Entity.Scene.FindComponentOfType<ActionPointComponent>();
            if (actionPointComponent != null )
            {
                OnActionPointsChanged(actionPointComponent);
                OnActionPointsTimerStarted(actionPointComponent);
                OnActionPointsTimerUpdated(actionPointComponent);
                OnMaxActionPointsChanged(actionPointComponent);
            }
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            _playerHealthLabel.SetText($"HP: {healthComponent.Health}/{healthComponent.MaxHealth}");
        }
    }
}
