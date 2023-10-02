using Nez;
using Nez.Sprites;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.HUDs
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
            _playerApLabel = new ApLabel("/", _basicSkin);
            apTable.Add(_playerApLabel).SetSpaceBottom(5);
            apTable.Row();
            _apProgressBar = new ApProgressBar(_basicSkin);
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
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        void OnDodgePhaseStarted()
        {
            //get initial hp values
            var playerHealthComponent = Entity.Scene.FindComponentsOfType<HealthComponent>().FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
            }
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            _playerHealthLabel.SetText($"HP: {healthComponent.Health}/{healthComponent.MaxHealth}");
        }
    }
}
