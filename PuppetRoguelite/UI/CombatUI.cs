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
            var healthComponents = Entity.Scene.FindComponentsOfType<HealthComponent>();
            var playerHealthComponent = healthComponents.FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
                playerHealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            }

            //player ap
            var apComponents = Entity.Scene.FindComponentsOfType<ActionPointComponent>();
            var playerApComponent = apComponents.FirstOrDefault(a => a.Entity.Name == "player");
            if (playerApComponent != null)
            {
                playerApComponent.ActionPointEmitter.AddObserver(ActionPointEventType.ActionPointGained, OnPlayerApGained);
                playerApComponent.ActionPointTimerEmitter.AddObserver(ActionPointEventType.TimeChanged, OnPlayerApTimerUpdated);
                playerApComponent.ActionPointTimerEmitter.AddObserver(ActionPointEventType.TimerStarted, OnPlayerApTimerStarted);
                playerApComponent.ActionPointEmitter.AddObserver(ActionPointEventType.MaxActionPointsChanged, OnPlayerMaxApChanged);
            }

            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnTurnPhaseCompleted);
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

        public void OnPlayerApGained(int apAmount)
        {
            var text = _playerApLabel.GetText();
            var slashIndex = text.IndexOf("/");
            var newString = apAmount.ToString() + text.Substring(slashIndex);
            _playerApLabel.SetText(newString);
        }

        public void OnPlayerApTimerUpdated(float time)
        {
            _apProgressBar.Value = time;
        }

        public void OnPlayerApTimerStarted(float maxTime)
        {
            _apProgressBar.SetMinMax(0, maxTime);
        }

        public void OnPlayerMaxApChanged(int maxAp)
        {
            var text = _playerApLabel.GetText();
            var slashIndex = text.IndexOf("/");
            var newString = text.Substring(0, slashIndex + 1) + maxAp.ToString();
            _playerApLabel.SetText(newString);
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            _playerHealthLabel.SetText($"HP: {healthComponent.Health}/{healthComponent.MaxHealth}");
        }
    }
}
