using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.UI.Elements;
using System.Collections.Generic;

namespace PuppetRoguelite.UI.HUDs
{
    public class CombatUI : UICanvas
    {
        //elements
        Table _table;
        Label _playerHealthLabel;
        Table _topLeftTable;
        Table _topRightTable;
        Table _apTable;
        Label _simTipLabel;
        Label _dollahLabel;
        List<BasicApProgressBar> _apProgressBars = new List<BasicApProgressBar>();

        //skins
        Skin _basicSkin;

        #region LIFECYCLE

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);
            _table.SetDebug(false);

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //arrange elements
            ArrangeElements();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //connect to emitters
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);

            //ap progress bars
            if (PlayerController.Instance.ActionPointComponent != null)
            {
                //setup ap progress bars
                for (int i = 0; i < PlayerController.Instance.ActionPointComponent.MaxActionPoints; i++)
                {
                    var bar = new BasicApProgressBar(i, _basicSkin);
                    bar.AddObservers();
                    _apProgressBars.Add(bar);
                    var width = ((Game1.DesignResolution.X * .5f) / PlayerController.Instance.ActionPointComponent.MaxActionPoints) - ((8 * (PlayerController.Instance.ActionPointComponent.MaxActionPoints - 1) / PlayerController.Instance.ActionPointComponent.MaxActionPoints));
                    _apTable.Add(bar).Width(width);
                }
            }

            //player health
            if (PlayerController.Instance.Entity.TryGetComponent<HealthComponent> (out var healthComponent))
            {
                OnPlayerHealthChanged(healthComponent);
                healthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            }

            //dollahs
            if (PlayerController.Instance.Entity.TryGetComponent<DollahInventory>(out var dollahInventory))
            {
                OnDollahsChanged(dollahInventory);
                dollahInventory.Emitter.AddObserver(DollahInventoryEvents.DollahsChanged, OnDollahsChanged);
            }
        }

        public override void OnRemovedFromEntity()
        {
            //connect to emitters
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            //Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);

            //player health
            //if (PlayerController.Instance.Entity.TryGetComponent<HealthComponent>(out var healthComponent))
            //{
            //    healthComponent.Emitter.RemoveObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            //}

            foreach (var bar in _apProgressBars)
            {
                bar.RemoveObservers();
            }

            _apProgressBars.Clear();
            _apTable.ClearChildren();

            //dollahs
            //if (PlayerController.Instance.Entity.TryGetComponent<DollahInventory>(out var dollahInventory))
            //{
            //    dollahInventory.Emitter.RemoveObserver(DollahInventoryEvents.DollahsChanged, OnDollahsChanged);
            //}
        }

        #endregion

        void ArrangeElements()
        {
            //hp
            _topLeftTable = new Table().Top().Left().PadTop(10).PadLeft(10);
            _table.Add(_topLeftTable).Grow();
            _playerHealthLabel = new Label("HP: ", _basicSkin, "default_xxl");
            _topLeftTable.Add(_playerHealthLabel);

            //currency
            _topRightTable = new Table().Top().Right().PadTop(10).PadRight(10);
            _table.Add(_topRightTable).Grow();
            _dollahLabel = new Label("$: ", _basicSkin, "default_xxl");
            _topRightTable.Add(_dollahLabel);
            _topRightTable.Pack();

            _table.Row();

            var bottomTable = new Table();
            _table.Add(bottomTable).SetColspan(2).Grow().SetPadBottom(20);

            var bottomLeftTable = new Table();
            bottomTable.Add(bottomLeftTable).Width(Game1.DesignResolution.X * .75f).Expand().Bottom();

            _apTable = new Table();
            _apTable.Defaults().SetSpaceRight(50);
            _apTable.SetVisible(false);
            bottomLeftTable.Add(_apTable).Expand();

            //_simTipLabel = new Label("* Press 'C' to view Action Sequence", _basicSkin);
            //_simTipLabel.SetWrap(true);
            //_simTipLabel.SetVisible(false);
            //bottomTable.Add(_simTipLabel).SetPadLeft(Game1.DesignResolution.X * .025f).SetPadRight(Game1.DesignResolution.X * .025f).Width(Game1.DesignResolution.X * .2f).Expand().Bottom().Right();
        }

        public void SetShowSimTipLabel(bool show)
        {
            _simTipLabel.SetVisible(show);
        }

        #region OBSERVERS

        void OnDollahsChanged(DollahInventory inv)
        {
            _dollahLabel.SetText($"$: {inv.Dollahs}");
        }

        void OnEncounterStarted()
        {
            //get initial hp values
            var playerHealthComponent = PlayerController.Instance.Entity.GetComponent<HealthComponent>();
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
            }

            //show relevant elements
            _apTable.SetVisible(true);
        }

        void OnEncounterEnded()
        {
            //hide elements
            _apTable.SetVisible(false);
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            _playerHealthLabel.SetText($"HP: {healthComponent.Health}/{healthComponent.MaxHealth}");
            _topLeftTable.Pack();
        }

        #endregion
    }
}
