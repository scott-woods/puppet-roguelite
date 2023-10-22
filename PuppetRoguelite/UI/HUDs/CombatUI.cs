using Nez;
using Nez.Sprites;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Characters.Player;
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
        Table _topLeftTable;
        Table _topRightTable;
        Table _bottomMiddleTable;
        Table _apTable;
        Label _simTipLabel;
        List<ApProgressBar> _apProgressBars = new List<ApProgressBar>();

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
            _basicSkin = CustomSkins.CreateBasicSkin();

            //arrange elements
            ArrangeElements();

            //connect to emitters
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //player health
            var playerHealthComponent = Entity.Scene.FindComponentsOfType<HealthComponent>().FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
                playerHealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            }
        }

        void ArrangeElements()
        {
            //hp
            _topLeftTable = new Table().Top().Left().PadTop(10).PadLeft(10);
            _table.Add(_topLeftTable).Grow();
            _playerHealthLabel = new Label("HP: ", _basicSkin);
            _topLeftTable.Add(_playerHealthLabel);

            //currency
            _topRightTable = new Table().Top().Right().PadTop(10).PadRight(10);
            _table.Add(_topRightTable).Grow();
            _topRightTable.Add(new Label("$", _basicSkin));
            _topRightTable.Pack();

            _table.Row();

            var bottomTable = new Table();
            _table.Add(bottomTable).SetColspan(2).Grow().SetPadBottom(20);

            var bottomLeftTable = new Table();
            bottomTable.Add(bottomLeftTable).Width(480 * .75f).Expand().Bottom().Left();

            _apTable = new Table();
            _apTable.Defaults().SetSpaceRight(8);
            _apTable.SetVisible(false);
            bottomLeftTable.Add(_apTable).Width(480 * .5f).Expand().Right();

            _simTipLabel = new Label("* Press 'C' to view Action Sequence", _basicSkin);
            _simTipLabel.SetWrap(true);
            _simTipLabel.SetVisible(false);
            bottomTable.Add(_simTipLabel).SetPadLeft(480 * .025f).SetPadRight(480 * .025f).Width(480 * .2f).Expand().Bottom().Right();
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

        void OnDodgePhaseStarted()
        {
            //ap bars
            _apProgressBars.Clear();
            _apTable.ClearChildren();
            if (PlayerController.Instance.ActionPointComponent != null)
            {
                //setup ap progress bars
                for (int i = 0; i < PlayerController.Instance.ActionPointComponent.MaxActionPoints; i++)
                {
                    var bar = new ApProgressBar(PlayerController.Instance.ActionPointComponent, i, _basicSkin);
                    _apProgressBars.Add(bar);
                    var width = ((480 * .5f) / PlayerController.Instance.ActionPointComponent.MaxActionPoints) - ((8 * (PlayerController.Instance.ActionPointComponent.MaxActionPoints - 1) / PlayerController.Instance.ActionPointComponent.MaxActionPoints));
                    _apTable.Add(bar).Width(width);
                }
            }
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            _playerHealthLabel.SetText($"HP: {healthComponent.Health}/{healthComponent.MaxHealth}");
            _topLeftTable.Pack();
        }

        public void SetShowSimTipLabel(bool show)
        {
            _simTipLabel.SetVisible(show);
        }
    }
}
